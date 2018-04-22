using System;
using Albumprinter.CorrelationTracking.Correlation.AmazonSns.Lambda.Logging;
using Amazon.Lambda.SNSEvents;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSns.Lambda
{
    public class CorrelationTrackingHandler : ICorrelationTrackingHandler
    {
        private static readonly ILog _logger = LogProvider.GetLogger(typeof(CorrelationTrackingHandler));
        private readonly IServiceProvider _serviceProvider;

        public CorrelationTrackingHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void Handle(SNSEvent snsEvent)
        {
            var requestId = Guid.NewGuid();

            foreach (var snsRecord in snsEvent.Records)
            {
                using (CorrelationManager.Instance.UseScope(snsRecord.Sns.ExtractCorrelationId() ?? Guid.NewGuid(), requestId))
                {
                    _logger.Info("Getting message handler");
                    var handler = (ISnsRecordHandler)_serviceProvider.GetService(typeof(ISnsRecordHandler));
                    if (handler == null)
                    {
                        _logger.Fatal($"No ISnsRecordHandler<{typeof(ISnsRecordHandler).Name}> could be found.");
                        throw new InvalidOperationException($"No INotificationHandler<{typeof(ISnsRecordHandler).Name}> could be found.");
                    }
                    _logger.Info("Invoking message handler");
                    handler.Handle(snsRecord).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
        }
    }
}
