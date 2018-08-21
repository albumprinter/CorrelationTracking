using Amazon.Lambda.SQSEvents;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSqs.Lambda
{
    public interface ICorrelationTrackingHandler
    {
        void Handle(SQSEvent sqsEvent);
    }
}