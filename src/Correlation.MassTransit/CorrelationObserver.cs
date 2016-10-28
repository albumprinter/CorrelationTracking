using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using MassTransit;

namespace Albumprinter.CorrelationTracking.Correlation.MassTransit
{
    public sealed class CorrelationObserver : IPublishObserver, ISendObserver, IReceiveObserver
    {
        private static readonly Task Done = Task.FromResult(true);
        public static readonly CorrelationObserver Instance = new CorrelationObserver();

        public Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            context.Headers.Set(CorrelationKeys.CorrelationId, CorrelationScope.Current.CorrelationId.ToString());
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
            context.Headers.Set(CorrelationKeys.CorrelationId, CorrelationScope.Current.CorrelationId.ToString());

            // BUG: Masstransit.Rabbitmq copies context.headers to transport.headers before calling PreSend method :(
            if (context.GetType().Name.StartsWith("RabbitMq", StringComparison.OrdinalIgnoreCase))
            {
                var headers = ((dynamic) context).BasicProperties?.Headers as IDictionary<string, object>;
                headers?.Add(CorrelationKeys.CorrelationId, CorrelationScope.Current.CorrelationId.ToString());
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
            object value;
            var correlationId = context.TransportHeaders.TryGetHeader(CorrelationKeys.CorrelationId, out value) ? Guid.Parse((string)value) : Guid.NewGuid();
            CorrelationManager.Instance.UseScope(correlationId);
            return Done;
        }

        Task IReceiveObserver.PostReceive(ReceiveContext context)
        {
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

        Task IReceiveObserver.ReceiveFault(ReceiveContext context, Exception exception)
        {
            return Done;
        }
    }
}