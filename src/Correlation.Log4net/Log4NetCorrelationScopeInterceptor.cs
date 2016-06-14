using System.Diagnostics;
using log4net;

namespace Albumprinter.CorrelationTracking.Correlation.Log4net
{
    public sealed class Log4NetCorrelationScopeInterceptor : ICorrelationScopeInterceptor
    {
        public void Enter(CorrelationManager manager, CorrelationScope scope)
        {
            LogicalThreadContext.Properties[@"X-CorrelationId"] = scope.CorrelationId;
            LogicalThreadContext.Properties[@"X-RequestId"] = scope.RequestId;
            LogicalThreadContext.Properties[@"X-ActivityId"] = Trace.CorrelationManager.ActivityId;
        }

        public void Exit(CorrelationManager manager, CorrelationScope scope)
        {
            // NOTE: to restore WCF flow tracing context on end-request in IIS
            LogicalThreadContext.Properties[@"X-CorrelationId"] = scope.CorrelationId;
            LogicalThreadContext.Properties[@"X-RequestId"] = scope.RequestId;
            LogicalThreadContext.Properties[@"X-ActivityId"] = Trace.CorrelationManager.ActivityId;
        }
    }
}