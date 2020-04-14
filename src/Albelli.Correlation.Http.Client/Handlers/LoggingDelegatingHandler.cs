using Albelli.Correlation.Http.Client.Configuration;
using Albumprinter.CorrelationTracking.Correlation.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Albelli.Correlation.Http.Client.Handlers
{
    public sealed class LoggingDelegatingHandler : DelegatingHandler
    {
        public static class ContextKeys
        {
            public const string Url = "Albelli.Correlation.Http.Client.Url";
            public const string StatusCode = "Albelli.Correlation.Http.Client.StatusCode";
            public const string Duration = "Albelli.Correlation.Http.Client.Duration";
        }

        private readonly IHttpClientLoggingConfiguration _config;
        private readonly ILogger _logger;

        public LoggingDelegatingHandler([NotNull] ILoggerFactory loggerFactory, IHttpClientLoggingConfiguration config)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<LoggingDelegatingHandler>();
            _config = config;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri?.ToString() ?? "<null>";
            var operationId = Guid.NewGuid();
            using (_logger.BeginScope(new Dictionary<string, object> { [ContextKeys.Url] = uri }))
            {
                if (_config.LogRequest(request))
                {
                    var output = new StringBuilder();
                    output.Append("BeforeSendRequest: ");
                    output.Append("Method: ").Append(request.Method);
                    output.Append(", RequestUri: '").Append(uri);
                    output.Append("', Version: ").Append(request.Version);
                    output.AppendLine(", Headers: {");
                    output.AppendLine(GetHeaders(_config.AllowedHeaders, request.Headers, request.Content?.Headers));
                    output.Append("}");
                    if (_config.LogRequestContent(request))
                    {
                        output.Append(", Content: ");
                        output.Append(request.Content == null ? "<null>" : await request.Content.ReadAsStringAsync().ConfigureAwait(false));
                    }

                    using (_logger.BeginScope(new Dictionary<string, object> { [CorrelationKeys.OperationId] = operationId }))
                    {
                        _logger.Log(LogLevel.Debug, output.ToString());
                    }
                }

                var stopWatch = Stopwatch.StartNew();
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                stopWatch.Stop();
                if (_config.LogResponse(response))
                {
                    var output = new StringBuilder();
                    output.Append("AfterReceiveResponse: ");
                    output.Append("StatusCode: ").Append((int)response.StatusCode);
                    output.Append(", ReasonPhrase: '").Append(response.ReasonPhrase ?? "<null>");
                    output.Append("', Version: ").Append(response.Version);
                    output.AppendLine(", Headers: {");
                    output.AppendLine(GetHeaders(_config.AllowedHeaders, response.Headers, response.Content?.Headers));
                    output.Append("}");
                    if (_config.LogResponseContent(response))
                    {
                        output.Append(", Content: ");
                        output.Append(response.Content == null ? "<null>" : await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    }

                    using (_logger.BeginScope(new Dictionary<string, object>
                    {
                        [CorrelationKeys.OperationId] = operationId,
                        [ContextKeys.Duration] = (int)Math.Ceiling(stopWatch.Elapsed.TotalMilliseconds),
                        [ContextKeys.StatusCode] = (int)response.StatusCode
                    }))
                    {
                        _logger.Log(ToLevel(response.StatusCode), output.ToString());
                    }
                }

                return response;
            }
        }

        private static string GetHeaders(ICollection<string> allowedHeaders, HttpHeaders httpHeaders, HttpHeaders contentHeaders = null)
        {
            if (httpHeaders == null)
            {
                return null;
            }

            var headers = httpHeaders
                .Concat(contentHeaders ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                .Where(h => allowedHeaders == null || allowedHeaders.Contains(h.Key))
                .Select(h => $"{h.Key}: {string.Join(" ", h.Value)}");

            return string.Join(Environment.NewLine, headers);
        }

        private LogLevel ToLevel(HttpStatusCode? status)
        {
            if (!status.HasValue)
            {
                return LogLevel.Error;
            }

            if (_config.WhiteListedCodes.Contains(status.Value))
            {
                return LogLevel.Information;
            }

            var code = (int)status.Value;

            if (code >= 200 && code < 300)
            {
                return LogLevel.Information;
            }

            if (code >= 400 && code < 600)
            {
                return LogLevel.Error;
            }

            return LogLevel.Warning; // 1xx, 3xx, >=600
        }
    }
}
