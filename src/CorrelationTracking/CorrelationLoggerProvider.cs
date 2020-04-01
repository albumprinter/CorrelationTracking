using System;
using System.Collections.Generic;
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
            CorrelationProperties = new Dictionary<string, object>();
        }
        public Dictionary<string, object> CorrelationProperties { get; }

        public void Dispose()
        {
            _loggerProvider.Dispose();
        }

        public ILogger CreateLogger(string categoryName)
        {
            var logger = _loggerProvider.CreateLogger(categoryName);
            logger.BeginScope(this.CorrelationProperties);
            return logger;
        }
    }
}
