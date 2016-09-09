using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Albumprinter.CorrelationTracking.Correlation.Core;

namespace Albumprinter.CorrelationTracking.Correlation.WCF
{
    public sealed class CorrelationClientBehavior : IEndpointBehavior, IClientMessageInspector
    {
        public bool AddWcfHeader { get; set; }
        public bool AddHttpHeader { get; set; }

        public CorrelationClientBehavior() : this(true, true)
        {
        }

        public CorrelationClientBehavior(bool addWcfHeader, bool addHttpHeader)
        {
            AddWcfHeader = addWcfHeader;
            AddHttpHeader = addHttpHeader;
        }

        void IEndpointBehavior.AddBindingParameters(
            ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }

        object IClientMessageInspector.BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            if (AddWcfHeader)
            {
                var header = MessageHeader.CreateHeader(CorrelationKeys.CorrelationId, CorrelationKeys.Namespace, CorrelationScope.Current.CorrelationId);
                request.Headers.Add(header);
            }

            if (AddHttpHeader)
            {
                // Add HTTP header
                HttpRequestMessageProperty httpRequestMessage;
                object httpRequestMessageObject;
                if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
                {
                    httpRequestMessage = (HttpRequestMessageProperty) httpRequestMessageObject;
                }
                else
                {
                    httpRequestMessage = new HttpRequestMessageProperty();
                    request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
                }
                if (string.IsNullOrEmpty(httpRequestMessage.Headers[CorrelationKeys.CorrelationId]))
                {
                    httpRequestMessage.Headers[CorrelationKeys.CorrelationId] = CorrelationScope.Current.CorrelationId.ToString();
                }
            }

            return null;
        }

        void IClientMessageInspector.AfterReceiveReply(ref Message reply, object correlationState)
        {
        }
    }
}