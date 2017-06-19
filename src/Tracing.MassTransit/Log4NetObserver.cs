using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MassTransit;
using MassTransit.Pipeline;
using Newtonsoft.Json;

namespace Albumprinter.CorrelationTracking.Tracing.MassTransit
{
    public sealed class Log4NetObserver : IPublishObserver, ISendObserver, IReceiveObserver, IConsumeObserver
    {
        private static readonly Task Done = Task.FromResult(true);
        private static readonly ILog DefaultLog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            Log = DefaultLog;
        }

        public ILog Log { get; set; }
        public bool LogEnvelope { get; set; }
        public bool LogMessage { get; set; }
        
        /// <summary>
        /// 0 is no limit, positive value is amount of characters logged in the message (exception and other objects are not truncated)
        /// </summary>
        public int MaxMessageSize { get; set; }

        private string TruncateMessage(string original)
        {
            if (MaxMessageSize > 0 && MaxMessageSize < original.Length)
                return original.Remove(MaxMessageSize) + $" //LOG TRUNCATED from {original.Length} to {MaxMessageSize} characters";
            return original;
        }

        public Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            if (LogMessage)
            {
                try
                {
                    var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                    Log.Debug(TruncateMessage(@"PrePublish: " + snapshot));
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
            return Done;
        }

        public Task PostPublish<T>(PublishContext<T> context) where T : class
        {
            if (LogMessage)
            {
                try
                {
                    var snapshot = GetTransportMessage(context);
                    Log.Debug(TruncateMessage(@"PostPublish: " + snapshot));
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
            return Done;
        }

        public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            if (LogMessage)
            {
                try
                {
                    var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                    Log.Error(TruncateMessage(@"PublishFault: " + snapshot), exception);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
            return Done;
        }

        public Task PreSend<T>(SendContext<T> context) where T : class
        {
            if (LogMessage)
            {
                try
                {
                    var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                    Log.Debug(TruncateMessage(@"PreSend: " + snapshot));
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
            return Done;
        }

        public Task PostSend<T>(SendContext<T> context) where T : class
        {
            if (LogMessage)
            {
                try
                {
                    var snapshot = GetTransportMessage(context);
                    Log.Debug(TruncateMessage(@"PostSend: " + snapshot));
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
            return Done;
        }

        public Task SendFault<T>(SendContext<T> context, Exception exception) where T : class
        {
            if (LogMessage)
            {
                try
                {
                    var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                    Log.Error(TruncateMessage(@"SendFault: " + snapshot), exception);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
            return Done;
        }

        Task IReceiveObserver.PreReceive(ReceiveContext context)
        {
            if (LogEnvelope)
            {
                try
                {
                    var message = new StreamReader(context.GetBody()).ReadToEnd();
                    Log.Debug(TruncateMessage(message));
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
            return context.CompleteTask;
        }

        Task IReceiveObserver.PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType)
        {
            if (LogMessage)
            {
                try
                {
                    Log.Debug(TruncateMessage($@"PostConsume: duration={duration}, consumerType={consumerType}"));
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }

            return context.CompleteTask;
        }

        Task IReceiveObserver.PostReceive(ReceiveContext context)
        {
            return context.CompleteTask;
        }

        Task IReceiveObserver.ConsumeFault<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception)
        {
            return context.CompleteTask;
        }

        Task IReceiveObserver.ReceiveFault(ReceiveContext context, Exception exception)
        {
            return context.CompleteTask;
        }

        Task IConsumeObserver.PreConsume<T>(ConsumeContext<T> context)
        {
            if (LogMessage)
            {
                try
                {
                    var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                    Log.Debug(TruncateMessage(@"PreConsume: " + snapshot));
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
            return Done;
        }

        Task IConsumeObserver.PostConsume<T>(ConsumeContext<T> context)
        {
            return Done;
        }

        Task IConsumeObserver.ConsumeFault<T>(ConsumeContext<T> context, Exception exception)
        {
            if (LogMessage)
            {
                try
                {
                    var snapshot = JsonConvert.SerializeObject(context.Message, JsonSerializerSettings);
                    Log.Error(TruncateMessage(@"ConsumeFault: " + snapshot), exception);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
            return Done;
        }

        private static string GetTransportMessage<T>(SendContext<T> context) where T : class
        {
            using (var memoryStream = new MemoryStream())
            {
                context.Serializer.Serialize(memoryStream, context);
                var snapshot = Encoding.UTF8.GetString(memoryStream.ToArray());
                return snapshot;
            }
        }
    }
}
