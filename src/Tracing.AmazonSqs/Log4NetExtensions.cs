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

        public static void Log(this SendMessageRequest request, int maxMessageSize)
        {
            var snapshot = JsonConvert.SerializeObject(request, JsonSerializerSettings);
            Logger.Debug(TruncateMessage(@"PostPublish: " + snapshot, maxMessageSize));
        }

        public static void Log(this SendMessageBatchRequestEntry request, int maxMessageSize)
        {
            var snapshot = JsonConvert.SerializeObject(request, JsonSerializerSettings);
            Logger.Debug(TruncateMessage(@"PostPublish: " + snapshot, maxMessageSize));
        }

        public static void Log(this SendMessageBatchRequest request, int maxMessageSize)
        {
            Log(request.Entries, maxMessageSize);
        }

        public static void Log(this IEnumerable<SendMessageBatchRequestEntry> entries, int maxMessageSize)
        {
            foreach (var entry in entries)
            {
                Log(entry, maxMessageSize);
            }
        }

        public static void Log(this ReceiveMessageResponse response, int maxMessageSize)
        {
            var snapshot = JsonConvert.SerializeObject(response, JsonSerializerSettings);
            Logger.Debug(TruncateMessage(@"PreConsume: " + snapshot, maxMessageSize));
        }

        public static Message Log(this Message message, int maxMessageSize)
        {
            Logger.Debug(TruncateMessage(@"PreConsume: " + message.Body, maxMessageSize));
            return message;
        }

        private static string TruncateMessage(string original, int maxMessageSize)
        {
            if (maxMessageSize > 0 && maxMessageSize < original.Length)
                return original.Remove(maxMessageSize) + $" //LOG TRUNCATED from {original.Length} to {maxMessageSize} characters";
            return original;
        }
    }
}