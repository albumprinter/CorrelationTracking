using System;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using MassTransit;
using MassTransit.Pipeline;
using Newtonsoft.Json;

namespace Albumprinter.CorrelationTracking.Tracing.MassTransit
{
    public sealed class Log4NetObserver : IPublishObserver, IConsumeObserver
    {
        private static readonly Task Done = Task.FromResult(true);
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            Log.Info(@"PrePublish: " + JsonConvert.SerializeObject(context.Message));
            return Done;
        }

        public Task PostPublish<T>(PublishContext<T> context) where T : class
        {
            return Done;
        }

        public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            Log.Error(@"PublishFault: " + JsonConvert.SerializeObject(context.Message), exception);
            return Done;
        }

        public Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            Log.Info(@"PreConsume: " + JsonConvert.SerializeObject(context.Message));
            return Done;
        }

        public Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
            return Done;
        }

        public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            Log.Error(@"ConsumeFault: " + JsonConvert.SerializeObject(context.Message), exception);
            return Done;
        }
    }
}
