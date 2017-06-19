using System;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace Albumprinter.CorrelationTracking.Tracing.WCF
{
    public sealed class Log4NetClientBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof(Log4NetClientBehavior);

        protected override object CreateBehavior() => new Log4NetClientBehavior(MaxMessageSize);

        const string MAX_MESSAGE_SIZE_PROPERTY_NAME = "maxMessageSize";

        [ConfigurationProperty(MAX_MESSAGE_SIZE_PROPERTY_NAME)]
        public int MaxMessageSize
        {
            get
            {
                return (int)base[MAX_MESSAGE_SIZE_PROPERTY_NAME];
            }
            set
            {
                base[MAX_MESSAGE_SIZE_PROPERTY_NAME] = value;
            }
        }
    }
}