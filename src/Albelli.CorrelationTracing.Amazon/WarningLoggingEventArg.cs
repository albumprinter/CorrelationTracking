using System;

namespace Albelli.CorrelationTracing.Amazon
{
    public class WarningLoggingEventArg : LoggingEventArg
    {
        public Exception Exception { get; set; }
    }
}