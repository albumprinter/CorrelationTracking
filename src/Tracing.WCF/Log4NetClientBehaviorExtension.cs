using System;
using System.ServiceModel.Configuration;

namespace Albumprinter.CorrelationTracking.Tracing.WCF
{
    public sealed class Log4NetClientBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof(Log4NetClientBehavior);

        protected override object CreateBehavior() => new Log4NetClientBehavior();
    }
}