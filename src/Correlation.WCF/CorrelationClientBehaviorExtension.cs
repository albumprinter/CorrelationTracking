using System;
using System.ServiceModel.Configuration;

namespace Albumprinter.CorrelationTracking.Correlation.WCF
{
    public sealed class CorrelationClientBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof(CorrelationClientBehavior);

        protected override object CreateBehavior() => new CorrelationClientBehavior();
    }
}