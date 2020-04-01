using System;
using System.Runtime.CompilerServices;
using Albumprinter.CorrelationTracking.Correlation.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Albumprinter.CorrelationTracking
{
    public static class CorrelationTrackingConfiguration
    {
        private static bool initialized;
        //
        // public static void Initialize([NotNull] ILoggerProvider loggerProvider)
        // {
        //     if (loggerProvider == null) throw new ArgumentNullException(nameof(loggerProvider));
        //     Initialize(new LoggingCorrelationScopeInterceptor(new CorrelationLoggerProvider(loggerProvider)));
        // }

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

    [PublicAPI]
    public sealed class LoggingCorrelationScopeInterceptor : ICorrelationScopeInterceptor
    {
        private readonly CorrelationLoggerProvider _correlationLoggerProvider;

        public LoggingCorrelationScopeInterceptor([NotNull] CorrelationLoggerProvider correlationLoggerProvider)
        {
            _correlationLoggerProvider = correlationLoggerProvider ?? throw new ArgumentNullException(nameof(correlationLoggerProvider));
        }

        public void Enter(CorrelationManager manager, CorrelationScope scope)
        {
            SetLogicalProperties(scope);
        }

        public void Exit(CorrelationManager manager, CorrelationScope scope)
        {
        }

        private void SetLogicalProperties(CorrelationScope scope)
        {
            _correlationLoggerProvider.CorrelationProperties[CorrelationKeys.ProcessId] = scope.ProcessId;
            _correlationLoggerProvider.CorrelationProperties[CorrelationKeys.CorrelationId] = scope.CorrelationId;
            _correlationLoggerProvider.CorrelationProperties[CorrelationKeys.RequestId] = scope.RequestId;
            _correlationLoggerProvider.CorrelationProperties[CorrelationKeys.ActivityId] =
                System.Diagnostics.Trace.CorrelationManager.ActivityId;
        }
    }
}
