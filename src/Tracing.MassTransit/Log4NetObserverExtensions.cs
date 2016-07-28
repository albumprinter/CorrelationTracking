﻿using MassTransit;

namespace Albumprinter.CorrelationTracking.Tracing.MassTransit
{
    public static class Log4NetObserverExtensions
    {
        public static IBusControl UseLog4NetObserver(this IBusControl bus)
        {
            bus.ConnectPublishObserver(Log4NetObserver.Instance);
            bus.ConnectConsumeObserver(Log4NetObserver.Instance);
            return bus;
        }
    }
}