using Amazon.Runtime.Internal;

namespace Albelli.CorrelationTracing.Amazon
{
    public static class LoggingExtensions
    {
        public static void ConfigureHandlers(LoggingOptions options = null)
        {
            RuntimePipelineCustomizerRegistry.Instance.Register(new LoggingPipelineCustomizer(options));
        }
    }
}
