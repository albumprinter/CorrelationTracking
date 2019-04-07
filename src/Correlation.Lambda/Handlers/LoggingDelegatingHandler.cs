using Albumprinter.CorrelationTracking.Correlation.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Albumprinter.CorrelationTracking.Correlation.Handlers
{
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILoggingConfiguration _config;

        public LoggingDelegatingHandler(ILoggingConfiguration config)
        {
            _config = config;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var operationId = Guid.NewGuid();
            if (_config.LogRequest)
            {
                var requestLogEntity = new HttpClientCommunicationRequest
                {
                    OperationId = operationId,
                    Method = request.Method?.ToString(),
                    Headers = GetHeaders(_config.AllowedHeaders, request.Headers, request.Content?.Headers),
                    Url = request.RequestUri?.ToString(),
                    Content = _config.LogRequestContent && request.Content != null ? await request.Content.ReadAsStringAsync().ConfigureAwait(false) : null
                };

                _config.Logger.Log(requestLogEntity);
            }
            var stopWatch = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (_config.LogResponse)
            {
                var responseLogEntity = new HttpClientCommunicationResponse
                {
                    OperationId = operationId,
                    Duration = stopWatch.Elapsed,
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Headers = GetHeaders(_config.AllowedHeaders, response.Headers, response.Content?.Headers),
                    Content = _config.LogResponseContent && response.Content != null ? await response.Content.ReadAsStringAsync().ConfigureAwait(false) : null
                };
                _config.Logger.Log(responseLogEntity);
            }
            return response;
        }

        private static List<KeyValuePair<string, string>> GetHeaders(ICollection<string> allowedHeaders, HttpHeaders httpHeaders, HttpHeaders contentHeaders = null)
        {
            if (httpHeaders == null)
            {
                return null;
            }

            var headers = httpHeaders.Concat(contentHeaders ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>());

            return headers
                .Where(h => allowedHeaders == null || allowedHeaders.Contains(h.Key))
                .Select(h => new KeyValuePair<string, string>(h.Key, string.Join(" ", h.Value)))
                .ToList();
        }
    }
}
