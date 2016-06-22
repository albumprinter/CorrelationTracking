using System;
using System.ServiceModel.Configuration;

namespace Albumprinter.CorrelationTracking.Tracing.WCF
{
    public sealed class ServiceLog4NetBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(Log4NetServiceBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new Log4NetServiceBehavior();
        }
    }
}