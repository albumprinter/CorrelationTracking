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

namespace Albumprinter.CorrelationTracking.Correlation.IIS
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CorrelationBehavior : Attribute, IServiceBehavior, IDispatchMessageInspector
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
            var сorrelationId = Guid.Empty;

            if (
                request.Headers.FindHeader(
                    CorrelationKeys.CorrelationId,
                    @"http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics") > -1)
            {
                сorrelationId = request.Headers.GetHeader<Guid>(
                    CorrelationKeys.CorrelationId,
                    @"http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics");
            }
            else
            {
                if (ReuseAspNetScope)
                {
                    var context = HttpContext.Current;
                    if (context != null && context.Items.Contains(typeof (CorrelationScope).Name))
                    {
                        var iisScope = context.Items[typeof (CorrelationScope).Name] as CorrelationScope;
                        if (iisScope != CorrelationScope.Zero)
                        {
                            return CorrelationManager.Instance.UseScope(iisScope);
                        }
                    }
                }
            }
            if (сorrelationId == Guid.Empty)
            {
                сorrelationId = Guid.NewGuid();
            }
            return CorrelationManager.Instance.UseScope(сorrelationId);
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var wcfScope = CorrelationScope.Current;
            if (wcfScope != CorrelationScope.Zero)
            {
                reply.Headers.Add(
                    MessageHeader.CreateHeader(CorrelationKeys.CorrelationId,
                        @"http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics",
                        wcfScope.CorrelationId));
            }

            var disposable = correlationState as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}