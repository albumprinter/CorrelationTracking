using Albelli.Correlation.Http.Client.Configuration;
using Albelli.Correlation.Http.Client.Logging;
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
using Albumprinter.CorrelationTracking.Correlation.Core;

namespace Albelli.Correlation.Http.Client.Handlers
{
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        public static class ContextKeys
        {
            public const string Url = "Albelli.Correlation.Http.Url";
            public const string StatusCode = "Albelli.Correlation.Http.StatusCode";
            public const string Duration = "Albelli.Correlation.Http.Duration";
        }

        private readonly IHttpClientLoggingConfiguration _config;
        private static readonly ILog _log = LogProvider.GetCurrentClassLogger();

        public LoggingDelegatingHandler(IHttpClientLoggingConfiguration config)
        {
            _config = config;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var currentLogProvider = LogProvider.CurrentLogProvider;

            var uri = request.RequestUri?.ToString() ?? "<null>";
            var operationId = Guid.NewGuid();
            using (currentLogProvider?.OpenMappedContext(ContextKeys.Url, uri))
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

                    using (currentLogProvider?.OpenMappedContext(CorrelationKeys.OperationId, operationId))
                    {
                        _log.Log(LogLevel.Debug, () => output.ToString());
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

                    using (currentLogProvider?.OpenMappedContext(CorrelationKeys.OperationId, operationId))
                    using (currentLogProvider?.OpenMappedContext(ContextKeys.Duration, stopWatch.Elapsed))
                    using (currentLogProvider?.OpenMappedContext(ContextKeys.StatusCode, response.StatusCode))
                    {
                        _log.Log(ToLevel(response.StatusCode), () => output.ToString());
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
                return LogLevel.Info;
            }

            var code = (int)status.Value;

            if (code >= 200 && code < 300)
            {
                return LogLevel.Info;
            }

            if (code >= 400 &&  code < 600)
            {
                return LogLevel.Error;
            }

            return LogLevel.Warn; // 1xx, 3xx, >=600
        }
    }
}
