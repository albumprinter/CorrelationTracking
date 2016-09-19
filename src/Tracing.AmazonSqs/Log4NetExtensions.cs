using System.Collections.Generic;
using System.Reflection;
using Amazon.SQS.Model;
using log4net;
using Newtonsoft.Json;

namespace Albumprinter.CorrelationTracking.Tracing.AmazonSqs
{
    public static class Log4NetExtensions
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings();

        public static void Log(this SendMessageRequest request)
        {
            var snapshot = JsonConvert.SerializeObject(request, JsonSerializerSettings);
            Logger.Debug(@"PostPublish: " + snapshot);
        }

        public static void Log(this SendMessageBatchRequestEntry request)
        {
            var snapshot = JsonConvert.SerializeObject(request, JsonSerializerSettings);
            Logger.Debug(@"PostPublish: " + snapshot);
        }

        public static void Log(this SendMessageBatchRequest request)
        {
            Log(request.Entries);
        }

        public static void Log(this IEnumerable<SendMessageBatchRequestEntry> entries)
        {
            foreach (var entry in entries)
            {
                Log(entry);
            }
        }

        public static void Log(this ReceiveMessageResponse response)
        {
            var snapshot = JsonConvert.SerializeObject(response, JsonSerializerSettings);
            Logger.Debug(@"PreConsume: " + snapshot);
        }

        public static Message Log(this Message message)
        {
            Logger.Debug(@"PreConsume: " + message.Body);
            return message;
        }
    }
}