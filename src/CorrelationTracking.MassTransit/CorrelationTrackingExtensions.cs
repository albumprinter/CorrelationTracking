using Albumprinter.CorrelationTracking.Correlation.MassTransit;
using Albumprinter.CorrelationTracking.Tracing.MassTransit;
using MassTransit;

namespace Albumprinter.CorrelationTracking.MassTransit
{
    public static class CorrelationTrackingExtensions
    {
        public static IBusControl UseCorrelationTracking(this IBusControl bus)
        {
            return bus.UseCorrelationObserver().UseLog4NetObserver();
        }
    }
}