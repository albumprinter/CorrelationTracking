using System;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using MassTransit;
using MassTransit.Pipeline;

namespace Albumprinter.CorrelationTracking.Correlation.MassTransit
{
    public sealed class CorrelationObserver : IPublishObserver, IConsumeObserver
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

        public Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            object value;
            var correlationId = context.Headers.TryGetHeader(CorrelationKeys.CorrelationId, out value) ? Guid.Parse((string) value) : Guid.NewGuid();
            CorrelationManager.Instance.UseScope(correlationId);
            return Done;
        }

        public Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
            return Done;
        }

        public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            return Done;
        }
    }
}