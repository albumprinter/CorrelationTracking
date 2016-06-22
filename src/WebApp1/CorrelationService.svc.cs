using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Correlation.WCF;
using Albumprinter.CorrelationTracking.Tracing.WCF;
using log4net;

namespace WebApp1
{
    [ServiceBehavior/*, CorrelationServiceBehavior, Log4NetServiceBehavior*/]
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
