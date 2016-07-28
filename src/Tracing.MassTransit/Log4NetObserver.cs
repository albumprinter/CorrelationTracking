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
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        };
        public static readonly Log4NetObserver Instance = new Log4NetObserver();

        public Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
            Log.Debug(@"PrePublish: " + snapshot);
            return Done;
        }

        public Task PostPublish<T>(PublishContext<T> context) where T : class
        {
            var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
            Log.Debug(@"PostPublish: " + snapshot);
            return Done;
        }

        public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
            Log.Error(@"PublishFault: " + snapshot, exception);
            return Done;
        }

        public Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
            Log.Debug(@"PreConsume: " + snapshot);
            return Done;
        }

        public Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
            var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
            Log.Debug(@"PostConsume: " + snapshot);
            return Done;
        }

        public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
            Log.Error(@"ConsumeFault: " + snapshot, exception);
            return Done;
        }
    }
}
