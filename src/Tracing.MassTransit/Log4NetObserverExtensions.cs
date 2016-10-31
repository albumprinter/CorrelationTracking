using MassTransit;

namespace Albumprinter.CorrelationTracking.Tracing.MassTransit
{
    public static class Log4NetObserverExtensions
    {
        public static IBusControl UseLog4NetObserver(this IBusControl bus)
        {
            return bus.UseLog4NetObserver(Log4NetObserver.Instance);
        }

        public static IBusControl UseLog4NetObserver(this IBusControl bus, Log4NetObserver observer)
        {
            bus.ConnectSendObserver(observer);
            bus.ConnectPublishObserver(observer);
            bus.ConnectReceiveObserver(observer);
            bus.ConnectConsumeObserver(observer);
            return bus;
        }
    }
}