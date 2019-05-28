using Amazon.Runtime.Internal;

namespace Albelli.Correlation.AmazonSns
{
    public static class SnsCorrelationExtensions
    {
        public static void ConfigureAwsHandlers()
        {
            RuntimePipelineCustomizerRegistry.Instance.Register(new SnsCorrelationPipelineCustomizer());
        }
    }
}
