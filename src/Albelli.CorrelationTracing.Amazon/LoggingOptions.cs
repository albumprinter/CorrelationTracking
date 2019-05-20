using Albumprinter.CorrelationTracking.Correlation.Core;
using System;

namespace Albelli.CorrelationTracing.Amazon
{
    public class LoggingOptions
    {
        public Action<AmazonDto> LogRequest { get; set; }
        public Action<AmazonDto> LogResponse { get; set; }
        public Action<AmazonErrorDto> LogError { get; set; }
        public bool LogRequestBodyEnabled { get; set; } = true;
        public bool LogResponseBodyEnabled { get; set; } = true;
    }

    public class AmazonDto
    {
        public CorrelationScope Scope { get; set; }
        public Guid OperationId { get; set; }
        public string RequestName { get; set; }
        public string Body { get; set; }
    }

    public class AmazonErrorDto : AmazonDto
    {
        public Exception Exception { get; set; }
    }
}
