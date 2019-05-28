using System;

namespace Albelli.CorrelationTracing.Amazon
{
    public class ErrorLoggingEventArg : LoggingEventArg
    {
        public Exception Exception { get; set; }
    }
}