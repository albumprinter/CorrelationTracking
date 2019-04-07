using Albumprinter.CorrelationTracking.Correlation.Core;
using log4net;

namespace Albelli.Correlation.Logging.log4net
{
    public sealed class Log4NetCorrelationScopeInterceptor : ICorrelationScopeInterceptor
    {
        public Log4NetCorrelationScopeInterceptor()
        {
            SetLogicalPropertiesOnExit = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to set the logical properties on exiting to restore WCF flow tracing context on end-request in IIS.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [restore WCF scope]; otherwise, <c>false</c>.
        /// </value>
        public bool SetLogicalPropertiesOnExit { get; set; }

        public void Enter(CorrelationManager manager, CorrelationScope scope)
        {
            SetLogicalProperties(scope);
        }

        public void Exit(CorrelationManager manager, CorrelationScope scope)
        {
            if (SetLogicalPropertiesOnExit)
            {
                SetLogicalProperties(scope);
            }
        }

        private static void SetLogicalProperties(CorrelationScope scope)
        {
            LogicalThreadContext.Properties[CorrelationKeys.ProcessId] = scope.ProcessId;
            LogicalThreadContext.Properties[CorrelationKeys.CorrelationId] = scope.CorrelationId;
            LogicalThreadContext.Properties[CorrelationKeys.RequestId] = scope.RequestId;
            LogicalThreadContext.Properties[CorrelationKeys.ActivityId] = System.Diagnostics.Trace.CorrelationManager.ActivityId;
        }
    }
}
