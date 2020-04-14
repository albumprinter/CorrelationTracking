using System;
using Amazon.Runtime.Internal;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Albelli.CorrelationTracing.Amazon
{
    [PublicAPI]
    public static class LoggingExtensions
    {
        public static void ConfigureHandlers([NotNull] ILoggerFactory loggerFactory, LoggingOptions options = null)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            RuntimePipelineCustomizerRegistry.Instance.Register(new LoggingPipelineCustomizer(loggerFactory, options));
        }
    }
}
