using MassTransit;

namespace Albumprinter.CorrelationTracking.Correlation.MassTransit
{
    public static class CorrelationObserverExtensions
    {
        public static void UseCorrelationObserver(this IBusControl bus)
        {
            bus.ConnectPublishObserver(CorrelationObserver.Instance);
            bus.ConnectConsumeObserver(CorrelationObserver.Instance);
        }
    }
}