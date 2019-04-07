using Albelli.Correlation.Http;
using log4net.Core;
using log4net.Util;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;

namespace Albelli.Correlation.Logging.log4net
{
    public class HttpClientLogger : IHttpClientLogger
    {
        private const string DurationKey = "Duration";
        private const string MessageCodeKey = "MessageCode";
        private const string OperationKey = "OperationId";
        private const string UrlKey = "Url";
        private const string HeadersKey = "Headers";
        private const string ContentKey = "Content";
        private const string MethodKey = "Method";

        // ReSharper disable MemberCanBePrivate.Global
        protected readonly ILogger _logger;
        protected readonly string _domain;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        // ReSharper restore MemberCanBePrivate.Global

        public HttpClientLogger(ILogger logger, IHttpContextAccessor httpContextAccessor, string domain)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _domain = domain;
        }

        public HttpClientLogger(ILogger logger, string domain) : this(logger, null, domain)
        {
        }

        public void Log(HttpClientCommunicationRequest request)
        {
            var eventData = new LoggingEventData
            {
                Level = Level.Debug,
                Message = request.ToString(),
                TimeStampUtc = DateTime.UtcNow,
                Domain = _domain,
                Properties = new PropertiesDictionary
                {
                    [MessageCodeKey] = nameof(HttpClientCommunicationRequest),
                    [OperationKey] = request.OperationId,
                    [MethodKey] = request.Method,
                    [UrlKey] = request.Url,
                    [HeadersKey] = request.Headers,
                    [ContentKey] = request.Content
                }
            };

            eventData = PopulateLoggingEventWithContextData(eventData);

            var logEvent = new LoggingEvent(eventData);

            _logger.Log(logEvent);
        }

        public void Log(HttpClientCommunicationResponse response)
        {
            var eventData = new LoggingEventData
            {
                Level = ToLevel(response.StatusCode),
                Message = response.ToString(),
                TimeStampUtc = DateTime.UtcNow,
                Domain = _domain,
                Properties = new PropertiesDictionary
                {
                    [MessageCodeKey] = nameof(HttpClientCommunicationResponse),
                    [OperationKey] = response.OperationId,
                    [DurationKey] = (long)response.Duration.TotalMilliseconds,
                    [UrlKey] = response.Url,
                    [HeadersKey] = response.Headers,
                    [ContentKey] = response.Content
                }
            };

            eventData = PopulateLoggingEventWithContextData(eventData);

            var logEvent = new LoggingEvent(eventData);

            _logger.Log(logEvent);
        }

        protected virtual LoggingEventData PopulateLoggingEventWithContextData(LoggingEventData loggingEvent, string userName = null)
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            loggingEvent.UserName = userName ?? httpContext?.User?.Identity?.Name;

            return loggingEvent;
        }

        private static Level ToLevel(HttpStatusCode? status)
        {
            if (!status.HasValue)
            {
                return Level.Error;
            }

            var code = (int)status.Value;

            if (code >= 200 && code < 300)
            {
                return Level.Debug;
            }

            if (code == 404)
            {
                return Level.Info;
            }

            if (code >= 500)
            {
                return Level.Error;
            }

            return Level.Warn;
        }
    }
}
