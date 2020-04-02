using System;

namespace Albelli.CorrelationTracing.Amazon
{
    public sealed class ErrorLoggingEventArg : LoggingEventArg
    {
        public Exception Exception { get; set; }
    }
}
