using System;

namespace Albelli.CorrelationTracing.Amazon
{
    public class LoggingOptions
    {
        public Action<LoggingEventArg> LogRequest { get; set; }
        public Action<LoggingEventArg> LogResponse { get; set; }
        public Action<WarningLoggingEventArg> LogWarning { get; set; }
        public Action<ErrorLoggingEventArg> LogError { get; set; }
        public bool LogRequestBodyEnabled { get; set; } = true;
        public bool LogResponseBodyEnabled { get; set; } = true;
    }
}
