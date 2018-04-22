using Albumprinter.CorrelationTracking.Correlation.Core;
using Serilog;

namespace Albumprinter.CorrelationTracking.Correlation.Serilog
{
    public static class Extensions
    {
        public static LoggerConfiguration AddCorrelationProperties(this LoggerConfiguration loggeerConfiguration)
        {
            CorrelationManager.Instance.ScopeInterceptors.Add(new SerilogCorrelationScopeInterceptor());
            return loggeerConfiguration;
        }
    }
}
