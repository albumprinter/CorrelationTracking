using System;
using Albumprinter.CorrelationTracking.Correlation.Core;

namespace Albelli.CorrelationTracing.Amazon
{
    public class LoggingEventArg
    {
        public CorrelationScope Scope { get; set; }
        public Guid OperationId { get; set; }
        public string RequestName { get; set; }
        public string Body { get; set; }
    }
}
