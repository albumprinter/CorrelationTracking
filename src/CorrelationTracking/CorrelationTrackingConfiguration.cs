using System;
using Albumprinter.CorrelationTracking.Correlation.Core;
using JetBrains.Annotations;

namespace Albumprinter.CorrelationTracking
{
    [PublicAPI]
    public static class CorrelationTrackingConfiguration
    {
        private static bool initialized;

        public static void Initialize(params ICorrelationScopeInterceptor[] interceptors)
        {
            if (interceptors == null)
            {
                throw new ArgumentNullException(nameof(interceptors));
            }

            if (interceptors.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(interceptors));
            }

            if (!initialized)
            {
                initialized = true;
                CorrelationManager.Instance.ScopeInterceptors.AddRange(interceptors);
            }
        }
    }
}
