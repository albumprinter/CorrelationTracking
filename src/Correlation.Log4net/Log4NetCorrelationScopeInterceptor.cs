using System.Diagnostics;
using log4net;

namespace Albumprinter.CorrelationTracking.Correlation.Log4net
{
    public sealed class Log4NetCorrelationScopeInterceptor : ICorrelationScopeInterceptor
    {
        public void Enter(CorrelationManager manager, CorrelationScope scope)
        {
            SetLogicalProperties(scope);
        }

        public void Exit(CorrelationManager manager, CorrelationScope scope)
        {
            // NOTE: to restore WCF flow tracing context on end-request in IIS
            SetLogicalProperties(scope);
        }

        private static void SetLogicalProperties(CorrelationScope scope)
        {
            LogicalThreadContext.Properties[CorrelationKeys.ProcessId] = scope.ProcessId;
            LogicalThreadContext.Properties[CorrelationKeys.CorrelationId] = scope.CorrelationId;
            LogicalThreadContext.Properties[CorrelationKeys.RequestId] = scope.RequestId;
            LogicalThreadContext.Properties[CorrelationKeys.ActivityId] = Trace.CorrelationManager.ActivityId;
        }
    }
}