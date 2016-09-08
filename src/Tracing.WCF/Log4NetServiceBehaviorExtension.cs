using System;
using System.ServiceModel.Configuration;

namespace Albumprinter.CorrelationTracking.Tracing.WCF
{
    public sealed class Log4NetServiceBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof(Log4NetServiceBehavior);

        protected override object CreateBehavior() => new Log4NetServiceBehavior();
    }
}