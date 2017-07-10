using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Correlation.Core.Providers;

namespace Albumprinter.CorrelationTracking.Correlation.WCF
{
    public sealed class WcfCorrelationTrackingConfiguration : ICorrelationTrackingConfiguration
    {
        public void Configure()
        {
            CorrelationScopeProviders.Default.Providers.Insert(0, WcfCorrelationScopeProvider.Instance);
        }
    }
}