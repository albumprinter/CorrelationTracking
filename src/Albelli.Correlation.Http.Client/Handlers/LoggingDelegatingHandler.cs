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

namespace Albelli.Correlation.Http.Client.Handlers
{
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        public static class ContextKeys
        {
            public const string OperationId = "Albelli.Correlation.OperationId"; // TODO: move me in base project
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
            using (currentLogProvider.OpenMappedContext(ContextKeys.OperationId, Guid.NewGuid()))
            using (currentLogProvider.OpenMappedContext(ContextKeys.Url, uri))
            {
                if (_config.LogRequest)
                {
                    var output = new StringBuilder();
                    output.Append("BeforeSendRequest: ");
                    output.Append("Method: ").Append(request.Method);
                    output.Append(", RequestUri: '").Append(uri);
                    output.Append("', Version: ").Append(request.Version);
                    output.AppendLine(", Headers: {");
                    output.AppendLine(GetHeaders(_config.AllowedHeaders, request.Headers, request.Content?.Headers));
                    output.Append("}");
                    if (_config.LogRequestContent)
                    {
                        output.Append(", Content: ");
                        output.Append(request.Content == null ? "<null>" : await request.Content.ReadAsStringAsync().ConfigureAwait(false));
                    }

                    _log.Log(LogLevel.Debug, () => output.ToString());
                }

                var stopWatch = Stopwatch.StartNew();
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                var elapsed = stopWatch.Elapsed;
                if (_config.LogResponse)
                {
                    var output = new StringBuilder();
                    output.Append("AfterReceiveResponse: ");
                    output.Append("StatusCode: ").Append((int)response.StatusCode);
                    output.Append(", ReasonPhrase: '").Append(response.ReasonPhrase ?? "<null>");
                    output.Append("', Version: ").Append(response.Version);
                    output.AppendLine(", Headers: {");
                    output.AppendLine(GetHeaders(_config.AllowedHeaders, response.Headers, response.Content?.Headers));
                    output.Append("}");
                    if (_config.LogResponseContent)
                    {
                        output.Append(", Content: ");
                        output.Append(response.Content == null ? "<null>" : await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    }

                    using (currentLogProvider.OpenMappedContext(ContextKeys.Duration, elapsed))
                    using (currentLogProvider.OpenMappedContext(ContextKeys.StatusCode, response.StatusCode))
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

        private static LogLevel ToLevel(HttpStatusCode? status)
        {
            if (!status.HasValue)
            {
                return LogLevel.Error;
            }

            var code = (int)status.Value;

            if (code >= 200 && code < 300)
            {
                return LogLevel.Debug;
            }

            if (code == 404)
            {
                return LogLevel.Info;
            }

            if (code >= 500)
            {
                return LogLevel.Error;
            }

            return LogLevel.Warn;
        }
    }
}
