using System;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Correlation.Log4net;
using log4net.Config;

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
                XmlConfigurator.Configure();
                CorrelationManager.Instance.ScopeInterceptors.Add(new Log4NetCorrelationScopeInterceptor());
                CorrelationManager.Instance.UseScope(Guid.Empty, Guid.Empty);
            }
        }
    }
}