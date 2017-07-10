using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Correlation.Core.Providers;

namespace Albumprinter.CorrelationTracking.Correlation.IIS
{
    public sealed class IisCorrelationTrackingConfiguration : ICorrelationTrackingConfiguration
    {
        public void Configure()
        {
            CorrelationScopeProviders.Default.Providers.Insert(0, IisCorrelationScopeProvider.Instance);
        }
    }
}