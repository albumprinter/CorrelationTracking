using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Albumprinter.CorrelationTracking
{
    [PublicAPI]
    public sealed class CorrelationLoggerProvider : ILoggerProvider
    {
        private readonly ILoggerProvider _loggerProvider;

        public CorrelationLoggerProvider([NotNull] ILoggerProvider loggerProvider)
        {
            _loggerProvider = loggerProvider ?? throw new ArgumentNullException(nameof(loggerProvider));
        }

        public void Dispose()
        {
            _loggerProvider.Dispose();
        }

        public ILogger CreateLogger(string categoryName)
        {
            var logger = _loggerProvider.CreateLogger(categoryName);
            logger.BeginScope()
        }
    }
}
