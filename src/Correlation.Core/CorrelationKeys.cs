namespace Albumprinter.CorrelationTracking.Correlation.Core
{
    public static class CorrelationKeys
    {
        public const string Namespace = @"http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics";
        public const string ProcessId = @"X-ProcessId";
        public const string CorrelationId = @"X-CorrelationId";
        public const string OperationId = @"X-OperationId";
        public const string RequestId = @"X-RequestId";
        public const string ActivityId = @"X-ActivityId";
        public const string ClientName = @"X-Client-Name";
        public const string TraceParent = @"traceparent";
        public const string TraceState = @"tracestate";
    }
}