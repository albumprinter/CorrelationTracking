using System;
using Albumprinter.CorrelationTracking.Correlation.AmazonSqs.Lambda.Logging;
using Amazon.Lambda.SQSEvents;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSqs.Lambda
{
    public class CorrelationTrackingHandler : ICorrelationTrackingHandler
    {
        private static readonly ILog _logger = LogProvider.GetLogger(typeof(CorrelationTrackingHandler));
        private readonly IServiceProvider _serviceProvider;

        public CorrelationTrackingHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void Handle(SQSEvent sqsEvent)
        {
            foreach (var sqsRecord in sqsEvent.Records)
            {
                using (sqsRecord.TrackCorrelationId())
                {
                    _logger.Info("Getting message handler");
                    var handler = (ISqsRecordHandler)_serviceProvider.GetService(typeof(ISqsRecordHandler));
                    if (handler == null)
                    {
                        _logger.Fatal($"No ISqsRecordHandler<{typeof(ISqsRecordHandler).Name}> could be found.");
                        throw new InvalidOperationException($"No INotificationHandler<{typeof(ISqsRecordHandler).Name}> could be found.");
                    }
                    _logger.Info("Invoking message handler");
                    handler.Handle(sqsRecord).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
        }
    }
}
