namespace Albumprinter.CorrelationTracking
{
    public static class CorrelationKeys
    {
        public static readonly string ProcessId = @"X-ProcessId";
        public static readonly string CorrelationId = @"X-CorrelationId";
        public static readonly string RequestId = @"X-RequestId";
        public static readonly string ActivityId = @"X-ActivityId";
    }
}