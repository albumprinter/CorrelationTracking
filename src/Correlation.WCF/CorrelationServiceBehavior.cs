using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace Albumprinter.CorrelationTracking.Correlation.WCF
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CorrelationServiceBehavior : Attribute, IServiceBehavior, IDispatchMessageInspector
    {
        [DefaultValue(false)]
        public bool ReuseAspNetScope { get; set; }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public void AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            ReuseAspNetScope = ReuseAspNetScope ||
                serviceDescription.Behaviors.OfType<AspNetCompatibilityRequirementsAttribute>()
                    .Any(x => x.RequirementsMode != AspNetCompatibilityRequirementsMode.NotAllowed);

            foreach (ChannelDispatcher chDisp in serviceHostBase.ChannelDispatchers)
            {
                foreach (var epDisp in chDisp.Endpoints)
                {
                    epDisp.DispatchRuntime.MessageInspectors.Add(this);
                }
            }
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            var correlationId = Guid.Empty;
            if (request.Headers.FindHeader(CorrelationKeys.CorrelationId, CorrelationKeys.Namespace) > -1)
            {
                correlationId = request.Headers.GetHeader<Guid>(CorrelationKeys.CorrelationId, CorrelationKeys.Namespace);
            }
            if (ReuseAspNetScope)
            {
                var context = HttpContext.Current;
                if (context != null && context.Items.Contains(typeof(CorrelationScope).Name))
                {
                    var iisScope = context.Items[typeof(CorrelationScope).Name] as CorrelationScope ?? CorrelationScope.Initial;
                    if (iisScope != CorrelationScope.Initial)
                    {
                        if (correlationId == Guid.Empty)
                        {
                            correlationId = iisScope.CorrelationId;
                        }
                        return CorrelationManager.Instance.UseScope(correlationId, iisScope.RequestId);
                    }
                }
            }
            if (correlationId == Guid.Empty)
            {
                correlationId = Guid.NewGuid();
            }
            return CorrelationManager.Instance.UseScope(correlationId);
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var wcfScope = CorrelationScope.Current;
            if (wcfScope != CorrelationScope.Initial && reply.Version != MessageVersion.None)
            {
                var header = MessageHeader.CreateHeader(CorrelationKeys.CorrelationId, CorrelationKeys.Namespace, wcfScope.CorrelationId);
                reply.Headers.Add(header);
            }

            var disposable = correlationState as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}