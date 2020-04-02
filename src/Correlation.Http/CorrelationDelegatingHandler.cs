using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using JetBrains.Annotations;

namespace Albumprinter.CorrelationTracking.Correlation.Http
{
    [PublicAPI]
    public sealed class CorrelationDelegatingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request?.Headers.Add(CorrelationKeys.CorrelationId, CorrelationScope.Current.CorrelationId.ToString());
            return base.SendAsync(request, cancellationToken);
        }
    }
}
