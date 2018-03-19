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

        public static void Initialize(ICorrelationScopeInterceptor interceptor)
        {
            if (interceptor == null)
            {
                throw new ArgumentNullException(nameof(interceptor));
            }

            if (!initialized)
            {
                initialized = true;
                CorrelationManager.Instance.ScopeInterceptors.Add(interceptor);
                CorrelationManager.Instance.UseScope(Guid.Empty, Guid.Empty);
            }
        }
    }
}
