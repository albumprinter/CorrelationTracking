using Albumprinter.CorrelationTracking.Correlation.Core;
using Serilog.Context;

namespace Albumprinter.CorrelationTracking.Correlation.Serilog
{
    public sealed class SerilogCorrelationScopeInterceptor : ICorrelationScopeInterceptor
    {
        public void Enter(CorrelationManager manager, CorrelationScope scope)
        {
            SetLogicalProperties(scope);
        }

        public void Exit(CorrelationManager manager, CorrelationScope scope)
        {
            SetLogicalProperties(scope);
        }

        private static void SetLogicalProperties(CorrelationScope scope)
        {
            LogContext.PushProperty(CorrelationKeys.ProcessId, scope.ProcessId);
            LogContext.PushProperty(CorrelationKeys.CorrelationId, scope.CorrelationId);
            LogContext.PushProperty(CorrelationKeys.RequestId, scope.RequestId);
            LogContext.PushProperty(CorrelationKeys.ActivityId, System.Diagnostics.Trace.CorrelationManager.ActivityId);
        }
    }
}
