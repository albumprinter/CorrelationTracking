using System;
using System.ServiceModel.Configuration;

namespace Albumprinter.CorrelationTracking.Correlation.IIS
{
    public sealed class ServiceCorrelationBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(CorrelationServiceBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new CorrelationServiceBehavior();
        }
    }
}