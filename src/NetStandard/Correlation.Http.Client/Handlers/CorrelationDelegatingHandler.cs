using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;

namespace Albelli.Correlation.Http.Client.Handlers
{
    public sealed class CorrelationDelegatingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request != null)
            {
                request.Headers.Add(CorrelationKeys.CorrelationId, CorrelationScope.Current.CorrelationId.ToString());
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
