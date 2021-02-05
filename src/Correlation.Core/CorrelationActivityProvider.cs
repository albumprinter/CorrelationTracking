using System.Diagnostics;

namespace Albumprinter.CorrelationTracking.Correlation.Core
{
    public class CorrelationActivityProvider
    {
        public static readonly ActivitySource Source = new ActivitySource("Albumprinter.CorrelationTracking");
    }
}