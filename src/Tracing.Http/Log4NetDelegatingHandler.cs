using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Albumprinter.CorrelationTracking.Tracing.Http
{
    public sealed class Log4NetDelegatingHandler : DelegatingHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Log4NetDelegatingHandler(bool logAll)
        {
            LogRequest = LogRequestContent = LogResponse = LogResponseContent = logAll;
            AllowedHeaders = new List<string> { "Accept", "Content-Type", "X-CorrelationId", "X-RequestId" };
        }

        public List<string> AllowedHeaders { get; private set; }
        public bool LogRequest { get; set; }
        public bool LogRequestContent { get; set; }
        public bool LogResponse { get; set; }
        public bool LogResponseContent { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (LogRequest)
            {
                var output = new StringBuilder();
                output.Append(@"BeforeSendRequest: ");
                output.Append("Method: ").Append(request.Method);
                output.Append(", RequestUri: '").Append(request.RequestUri?.ToString() ?? "<null>");
                output.Append("', Version: ").Append(request.Version);
                output.AppendLine(", Headers: {");
                output.AppendLine(GetHeaders(AllowedHeaders, request.Headers, request.Content?.Headers));
                output.Append("}");
                if (LogRequestContent)
                {
                    output.Append(", Content: ");
                    output.Append(request.Content == null ? "<null>" : await request.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                Log.Info(output.ToString());
            }
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (LogResponse)
            {
                var output = new StringBuilder();
                output.Append("AfterReceiveResponse: ");
                output.Append("StatusCode: ").Append((int) response.StatusCode);
                output.Append(", ReasonPhrase: '").Append(response.ReasonPhrase ?? "<null>");
                output.Append("', Version: ").Append(response.Version);
                output.AppendLine(", Headers: {");
                output.AppendLine(GetHeaders(AllowedHeaders, response.Headers, response.Content?.Headers));
                output.Append("}");
                if (LogResponseContent)
                {
                    output.Append(", Content: ");
                    output.Append(response.Content == null ? "<null>" : await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                Log.Info(output.ToString());
            }
            return response;
        }

        private static string GetHeaders(ICollection<string> allowedHeaders, HttpHeaders httpHeaders, HttpHeaders contentHeaders = null)
        {
            if (allowedHeaders == null)
            {
                throw new ArgumentNullException(nameof(allowedHeaders));
            }
            if (httpHeaders == null)
            {
                throw new ArgumentNullException(nameof(httpHeaders));
            }
            var headers = httpHeaders.Concat(contentHeaders ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>());
            return string.Join(
                Environment.NewLine,
                headers.Where(h => allowedHeaders.Contains(h.Key)).Select(h => $"{h.Key}: {string.Join(" ", h.Value)}"));
        }
    }
}
