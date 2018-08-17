using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSqs.Lambda
{
    public interface ISqsRecordHandler
    {
        Task Handle(SQSEvent.SQSMessage sqsMessage);
    }
}
