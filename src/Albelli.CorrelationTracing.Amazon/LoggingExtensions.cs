using Amazon.Runtime.Internal;

namespace Albelli.CorrelationTracing.Amazon
{
    public static class LoggingExtensions
    {
        public static void ConfigureHandlers()
        {
            RuntimePipelineCustomizerRegistry.Instance.Register(new LoggingPipelineCustomizer());
        }
    }
}
