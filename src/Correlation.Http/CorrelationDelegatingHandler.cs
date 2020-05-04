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
            var currentScope = CorrelationScope.Current;
            if (currentScope != null)
            {
                request?.Headers.Add(CorrelationKeys.CorrelationId, currentScope.CorrelationId.ToString());
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
