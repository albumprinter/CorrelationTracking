using System;
using Albumprinter.CorrelationTracking.Correlation.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Albumprinter.CorrelationTracking
{
    public static class CorrelationTrackingConfiguration
    {
        private static bool initialized;

        public static void Initialize([NotNull] ILoggerProvider loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Initialize(new LoggingCorrelationScopeInterceptor(new CorrelationLoggerProvider(loggerFactory)));
        }

        public static void Initialize(params ICorrelationScopeInterceptor[] interceptors)
        {
            if (interceptors == null)
            {
                throw new ArgumentNullException(nameof(interceptors));
            }

            if (interceptors.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(interceptors), "Should contain at least one interceptor");
            }

            if (!initialized)
            {
                initialized = true;
                CorrelationManager.Instance.ScopeInterceptors.AddRange(interceptors);
                CorrelationManager.Instance.UseScope(Guid.Empty, Guid.Empty);
            }
        }
    }

    public sealed class LoggingCorrelationScopeInterceptor : ICorrelationScopeInterceptor
    {
        public LoggingCorrelationScopeInterceptor(CorrelationLoggerProvider correlationLoggerProvider)
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
            LogProvider.OpenMappedContext(CorrelationKeys.ActivityId, System.Diagnostics.Trace.CorrelationManager.ActivityId);

        }
    }
}
