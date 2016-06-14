using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Albumprinter.CorrelationTracking.Tracing.Http.Log4net
{
    public sealed class Log4NetDelegatingHandler : DelegatingHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Log4NetDelegatingHandler(bool enabled)
        {
            LogRequest = LogRequestContent = LogResponse = LogResponseContent = enabled;
        }

        public bool LogRequest { get; set; }
        public bool LogRequestContent { get; set; }
        public bool LogResponse { get; set; }
        public bool LogResponseContent { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (LogRequest)
            {
                var output = request.ToString();
                if (LogRequestContent && request.Content != null)
                {
                    output = string.Concat(output, Environment.NewLine, await request.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                Log.Info(output);
            }
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (LogResponse)
            {
                var output = response.ToString();
                if (LogResponseContent && response.Content != null)
                {
                    output = string.Concat(output, Environment.NewLine, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                Log.Info(output);
            }
            return response;
        }
    }
}
