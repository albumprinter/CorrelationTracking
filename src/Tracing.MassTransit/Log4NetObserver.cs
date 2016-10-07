using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using MassTransit;
using MassTransit.Pipeline;
using Newtonsoft.Json;

namespace Albumprinter.CorrelationTracking.Tracing.MassTransit
{
    public sealed class Log4NetObserver : IPublishObserver, ISendObserver, IConsumeObserver
    {
        private static readonly Task Done = Task.FromResult(true);
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly Log4NetObserver Instance;
        public static readonly JsonSerializerSettings JsonSerializerSettings;

        static Log4NetObserver()
        {
            Instance = new Log4NetObserver();
            JsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
        }

        public Log4NetObserver()
        {
            LogEnvelope = LogMessage = true;
        }

        public bool LogEnvelope { get; set; }
        public bool LogMessage { get; set; }

        public Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            if (LogMessage)
            {
                var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                Log.Debug(@"PrePublish: " + snapshot);
            }
            return Done;
        }

        public Task PostPublish<T>(PublishContext<T> context) where T : class
        {
            if (LogMessage)
            {
                var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                Log.Debug(@"PostPublish: " + snapshot);
            }
            return Done;
        }

        public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            if (LogMessage)
            {
                var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                Log.Error(@"PublishFault: " + snapshot, exception);
            }
            return Done;
        }

        public Task PreSend<T>(SendContext<T> context) where T : class
        {
            if (LogMessage)
            {
                var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                Log.Debug(@"PreSend: " + snapshot);
            }
            return Done;
        }

        public Task PostSend<T>(SendContext<T> context) where T : class
        {
            if (LogMessage)
            {
                var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                Log.Debug(@"PostSend: " + snapshot);
            }
            return Done;
        }

        public Task SendFault<T>(SendContext<T> context, Exception exception) where T : class
        {
            if (LogMessage)
            {
                var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                Log.Error(@"SendFault: " + snapshot, exception);
            }
            return Done;
        }

        public Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            if (LogEnvelope)
            {
                Log.Debug(new StreamReader(context.ReceiveContext.GetBody()).ReadToEnd());
            }
            if (LogMessage)
            {
                var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                Log.Debug(@"PreConsume: " + snapshot);
            }
            return Done;
        }

        public Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
            if (LogMessage)
            {
                var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                Log.Debug(@"PostConsume: " + snapshot);
            }
            return Done;
        }

        public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            if (LogMessage)
            {
                var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                Log.Error(@"ConsumeFault: " + snapshot, exception);
            }
            return Done;
        }
    }
}
