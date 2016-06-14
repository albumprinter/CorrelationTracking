using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Correlation.Http
{
    public sealed class CorrelationDelegatingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request != null)
            {
                request.Headers.Add(@"X-CorrelationId", CorrelationScope.Current.CorrelationId.ToString());
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
