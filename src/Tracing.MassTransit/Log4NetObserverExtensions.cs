using MassTransit;

namespace Albumprinter.CorrelationTracking.Tracing.MassTransit
{
    public static class Log4NetObserverExtensions
    {
        public static IBusControl UseLog4NetObserver(this IBusControl bus)
        {
            bus.ConnectSendObserver(Log4NetObserver.Instance);
            bus.ConnectPublishObserver(Log4NetObserver.Instance);
            bus.ConnectReceiveObserver(Log4NetObserver.Instance);
            bus.ConnectConsumeObserver(Log4NetObserver.Instance);
            return bus;
        }
    }
}