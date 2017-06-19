using System;
using Albumprinter.CorrelationTracking.Correlation.AmazonSqs;
using Albumprinter.CorrelationTracking.Tracing.AmazonSqs;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace Albumprinter.CorrelationTracking.AmazonSqs
{
    public static class CorrelationTrackingExtensions
    {
        public static IAmazonSQS UseCorrelationTracking(this IAmazonSQS client, bool logReceiveMessageResponse = false, int maxMessageSize = 0)
        {
            return new Log4NetDecorator(new CorrelationDecorator(client), logReceiveMessageResponse, maxMessageSize);
        }

        public static IDisposable SetCorrelationScopeAndLog(this Message message, int maxMessageSize = 0)
        {
            var disposable = message.SetCorrelationScopeFromMessage();
            message.Log(maxMessageSize);
            return disposable;
        }
    }
}