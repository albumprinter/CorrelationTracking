using System;
using System.ServiceModel.Configuration;

namespace Albumprinter.CorrelationTracking.Correlation.WCF
{
    public sealed class CorrelationServiceBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof(CorrelationServiceBehavior);

        protected override object CreateBehavior() => new CorrelationServiceBehavior();
    }
}