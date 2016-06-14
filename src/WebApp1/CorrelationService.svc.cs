using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Albumprinter.CorrelationTracking;
using Albumprinter.CorrelationTracking.Correlation.IIS;
using Albumprinter.CorrelationTracking.Tracing.IIS;
using log4net;

namespace WebApp1
{
    [ServiceBehavior, CorrelationBehavior, TracingBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public sealed class CorrelationService : ICorrelationService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Guid GetCorrelationId()
        {
            try
            {
                Log.Info("WCF: on start");
                return CorrelationScope.Current.CorrelationId;
            }
            finally
            {
                Log.Info("WCF: on end");
            }
        }

        public void ThrowError()
        {
            throw new NotSupportedException("TEST_ERROR!");
        }
    }
}
