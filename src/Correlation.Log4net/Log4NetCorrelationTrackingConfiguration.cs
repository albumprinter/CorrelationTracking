using Albumprinter.CorrelationTracking.Correlation.Core;

namespace Albumprinter.CorrelationTracking.Correlation.Log4net
{
    public sealed class Log4NetCorrelationTrackingConfiguration : ICorrelationTrackingConfiguration
    {
        public void Configure()
        {
            CorrelationManager.Instance.ScopeInterceptors.Add(Log4NetCorrelationScopeInterceptor.Instance);
        }
    }
}