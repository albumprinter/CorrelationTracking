using System;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Correlation.LibLog;

namespace Albumprinter.CorrelationTracking
{
    public static class CorrelationTrackingConfiguration
    {
        private static bool initialized;

        public static void Initialize()
        {
            Initialize(new LibLogCorrelationScopeInterceptor());
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
}
