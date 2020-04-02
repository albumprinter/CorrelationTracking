using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using MassTransit;
using MassTransit.Pipeline;

namespace Albumprinter.CorrelationTracking.Correlation.MassTransit
{
    public sealed class CorrelationObserver : IPublishObserver, ISendObserver, IReceiveObserver, IConsumeObserver
    {
        private static readonly Task Done = Task.FromResult(true);
        public static readonly CorrelationObserver Instance = new CorrelationObserver();

        public CorrelationObserver()
        {
            ReceiveContextManager = new ReceiveContextManager();
            InjectOnPrePublish = true;
            InjectOnPreSend = true;
            InjectOnPreSendToRabbitMqProperties = true;
            RestoreOnPreReceive = true;
            RestoreOnPreConsume = true;
        }

        private ReceiveContextManager ReceiveContextManager { get; }
        public bool InjectOnPrePublish { get; set; }
        public bool InjectOnPreSend { get; set; }
        /// <summary>
        /// BUG: Masstransit.Rabbitmq v3.3.5 copies context.headers to transport.headers before calling PreSend method :(
        /// </summary>
        public bool InjectOnPreSendToRabbitMqProperties { get; set; }
        public bool RestoreOnPreReceive { get; set; }
        public bool RestoreOnPreConsume { get; set; }

        public Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            if (InjectOnPrePublish)
            {
                context.Headers.Set(CorrelationKeys.CorrelationId, CorrelationScope.Current.CorrelationId.ToString());
            }
            return Done;
        }

        public Task PostPublish<T>(PublishContext<T> context) where T : class
        {
            return Done;
        }

        public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            return Done;
        }

        public Task PreSend<T>(SendContext<T> context) where T : class
        {
            if (InjectOnPreSend)
            {
                context.Headers.Set(CorrelationKeys.CorrelationId, CorrelationScope.Current.CorrelationId.ToString());

                if (InjectOnPreSendToRabbitMqProperties)
                {
                    // BUG: Masstransit.Rabbitmq v3.3.5 copies context.headers to transport.headers before calling PreSend method :(
                    if (context.GetType().Name.StartsWith("RabbitMq", StringComparison.OrdinalIgnoreCase))
                    {
                        var headers = ((dynamic) context).BasicProperties?.Headers as IDictionary<string, object>;
                        if (headers != null)
                        {
                            headers[CorrelationKeys.CorrelationId] = CorrelationScope.Current.CorrelationId.ToString();
                        }
                    }
                }
            }
            return Done;
        }

        public Task PostSend<T>(SendContext<T> context) where T : class
        {
            return Done;
        }

        public Task SendFault<T>(SendContext<T> context, Exception exception) where T : class
        {
            return Done;
        }

        Task IReceiveObserver.PreReceive(ReceiveContext context)
        {
            if (RestoreOnPreReceive)
            {
                object value;
                var correlationId = context.TransportHeaders.TryGetHeader(CorrelationKeys.CorrelationId, out value) ? Guid.Parse((string)value) : Guid.NewGuid();
                ReceiveContextManager.Capture(context, CorrelationManager.Instance.UseScope(correlationId));
            }
            return Done;
        }

        Task IReceiveObserver.PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType)
        {
            return Done;
        }

        Task IReceiveObserver.ConsumeFault<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception)
        {
            return Done;
        }

        Task IReceiveObserver.PostReceive(ReceiveContext context)
        {
            ReceiveContextManager.Release(context);
            return Done;
        }

        Task IReceiveObserver.ReceiveFault(ReceiveContext context, Exception exception)
        {
            ReceiveContextManager.Release(context);
            return Done;
        }

        Task IConsumeObserver.PreConsume<T>(ConsumeContext<T> context)
        {
            if (RestoreOnPreConsume)
            {
                // NOTE: The last chance to restore CorrelationId from the headers of message.
                object value;
                var correlationScope = CorrelationScope.Current;
                var correlationId =
                        context.Headers.TryGetHeader(CorrelationKeys.CorrelationId, out value) ||
                        context.ReceiveContext.TransportHeaders.TryGetHeader(CorrelationKeys.CorrelationId, out value)
                            ? Guid.Parse((string) value)
                            : Guid.NewGuid();
                if (correlationId != correlationScope.CorrelationId)
                {
                    ReceiveContextManager.Release(context.ReceiveContext);
                    // NOTE: RequestId to link all related logs before reassigning the scope.
                    var requestId = correlationScope.RequestId != Guid.Empty ? correlationScope.RequestId : Guid.NewGuid();
                    ReceiveContextManager.Capture(context.ReceiveContext, CorrelationManager.Instance.UseScope(correlationId, requestId));
                }
            }

            return context.ConsumeCompleted;
        }

        Task IConsumeObserver.PostConsume<T>(ConsumeContext<T> context)
        {

            return context.ConsumeCompleted;
        }

        Task IConsumeObserver.ConsumeFault<T>(ConsumeContext<T> context, Exception exception)
        {
            return context.ConsumeCompleted;
        }
    }
}