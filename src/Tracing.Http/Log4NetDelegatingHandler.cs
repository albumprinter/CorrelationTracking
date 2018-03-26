using System;

namespace Albumprinter.CorrelationTracking.Tracing.Http
{
    [Obsolete("Please use LoggingDelegatingHandler, this class will be removed in future versions")]
    public sealed class Log4NetDelegatingHandler : LoggingDelegatingHandler
    {
        public Log4NetDelegatingHandler(bool logAll = true)
            : base(logAll)
        {

        }
    }
}
