namespace Albumprinter.CorrelationTracking.Correlation.Core
{
    public static class CorrelationKeys
    {
        public static readonly string Namespace = @"http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics";
        public static readonly string ProcessId = @"X-ProcessId";
        public static readonly string CorrelationId = @"X-CorrelationId";
        public static readonly string RequestId = @"X-RequestId";
        public static readonly string ActivityId = @"X-ActivityId";
    }
}