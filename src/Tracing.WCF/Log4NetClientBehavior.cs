using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using log4net;

namespace Albumprinter.CorrelationTracking.Tracing.WCF
{
    public sealed class Log4NetClientBehavior : IEndpointBehavior, IClientMessageInspector
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        /// <summary>
        /// 0 is no limit, positive value is amount of characters logged in the message (exception and other objects are not truncated)
        /// </summary>
        public int MaxMessageSize { get; }

        private string TruncateMessage(string original)
        {
            if (MaxMessageSize > 0 && MaxMessageSize < original.Length)
                return original.Remove(MaxMessageSize) + $"// LOG TRUNCATED from {original.Length} to {MaxMessageSize} characters";
            return original;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxMessageSize">0 is no limit, positive value is amount of characters logged in the message (exception and other objects are not truncated)</param>
        public Log4NetClientBehavior(int maxMessageSize)
        {
            MaxMessageSize = maxMessageSize;
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
            Log.Debug(TruncateMessage(@"BeforeSendRequest: " + request));
            return null;
        }

        void IClientMessageInspector.AfterReceiveReply(ref Message reply, object correlationState)
        {
            Log.Debug(TruncateMessage(@"AfterReceiveReply: " + reply));
        }
    }
}
