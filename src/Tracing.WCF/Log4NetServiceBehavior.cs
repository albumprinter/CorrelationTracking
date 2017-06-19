using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using log4net;

namespace Albumprinter.CorrelationTracking.Tracing.WCF
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class Log4NetServiceBehavior : Attribute, IServiceBehavior, IDispatchMessageInspector, IErrorHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 0 is no limit, positive value is amount of characters logged in the message (exception and other objects are not truncated)
        /// </summary>
        public int MaxMessageSize { get; }

        private string TruncateMessage(string original)
        {
            if (MaxMessageSize > 0 && MaxMessageSize < original.Length)
                return original.Remove(MaxMessageSize) + $" //LOG TRUNCATED from {original.Length} to {MaxMessageSize} characters";
            return original;
        }

        public Log4NetServiceBehavior(int maxMessageSize)
        {
            MaxMessageSize = maxMessageSize;
        }

        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var chDisp in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>())
            {
                chDisp.ErrorHandlers.Add(this);
                foreach (var epDisp in chDisp.Endpoints)
                {
                    epDisp.DispatchRuntime.MessageInspectors.Add(this);
                }
            }
        }

        object IDispatchMessageInspector.AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            Log.Debug(TruncateMessage(@"AfterReceiveRequest: " + request));
            return null;
        }

        void IDispatchMessageInspector.BeforeSendReply(ref Message reply, object correlationState)
        {
            Log.Debug(TruncateMessage(@"BeforeSendReply: " + reply));
        }

        bool IErrorHandler.HandleError(Exception error)
        {
            return true;
        }

        void IErrorHandler.ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            Log.Error(TruncateMessage(@"ProvideFault: " + fault), error);
        }
    }
}