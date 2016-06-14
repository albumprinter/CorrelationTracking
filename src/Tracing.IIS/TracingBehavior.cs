﻿using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using log4net;

namespace Albumprinter.CorrelationTracking.Tracing.IIS
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TracingBehavior : Attribute, IServiceBehavior, IDispatchMessageInspector, IErrorHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            foreach (ChannelDispatcher chDisp in serviceHostBase.ChannelDispatchers)
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
            Log.Info(request);
            return null;
        }

        void IDispatchMessageInspector.BeforeSendReply(ref Message reply, object correlationState)
        {
            Log.Info(reply);
        }

        bool IErrorHandler.HandleError(Exception error)
        {
            return true;
        }

        void IErrorHandler.ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            Log.Error(fault, error);
        }
    }
}