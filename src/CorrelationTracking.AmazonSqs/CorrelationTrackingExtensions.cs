using System;
using Albumprinter.CorrelationTracking.Correlation.AmazonSqs;
using Albumprinter.CorrelationTracking.Tracing.AmazonSqs;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace Albumprinter.CorrelationTracking.AmazonSqs
{
    public static class CorrelationTrackingExtensions
    {
        public static IAmazonSQS UseCorrelationTracking(this IAmazonSQS client, bool logReceiveMessageResponse = false)
        {
            return new Log4NetDecorator(new CorrelationDecorator(client), logReceiveMessageResponse);
        }

        public static IDisposable SetCorrelationScopeAndLog(this Message message)
        {
            var disposable = message.SetCorrelationScopeFromMessage();
            message.Log();
            return disposable;
        }
    }
}