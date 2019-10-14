using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Correlation.LibLog.Logging;

namespace Albumprinter.CorrelationTracking.Correlation.LibLog
{
    public sealed class LibLogCorrelationScopeInterceptor : ICorrelationScopeInterceptor
    {
        static LibLogCorrelationScopeInterceptor()
        {
            if (LogProvider.CurrentLogProvider == null)
            {
                LogProvider.SetCurrentLogProvider(LogProvider.ResolveLogProvider());
            }
        }

        public LibLogCorrelationScopeInterceptor()
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
            LogProvider.OpenMappedContext(CorrelationKeys.ProcessId, scope.ProcessId);
            LogProvider.OpenMappedContext(CorrelationKeys.CorrelationId, scope.CorrelationId);
            LogProvider.OpenMappedContext(CorrelationKeys.RequestId, scope.RequestId);

#if NETSTANDARD1_3
            LogProvider.OpenMappedContext(CorrelationKeys.ActivityId, System.Diagnostics.Activity.Current.Id);
#else
            LogProvider.OpenMappedContext(CorrelationKeys.ActivityId, System.Diagnostics.Trace.CorrelationManager.ActivityId);
#endif

        }
    }
}
