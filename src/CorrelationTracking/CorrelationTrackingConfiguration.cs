using System;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Correlation.Log4net;

namespace Albumprinter.CorrelationTracking
{
    public static class CorrelationTrackingConfiguration
    {
        private static bool initialized;

        public static void Initialize()
        {
            if (!initialized)
            {
                initialized = true;
                CorrelationManager.Instance.ScopeInterceptors.Add(new Log4NetCorrelationScopeInterceptor());
                CorrelationManager.Instance.UseScope(Guid.Empty, Guid.Empty);
            }
        }
    }
}