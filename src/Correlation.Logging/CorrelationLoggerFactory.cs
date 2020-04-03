using System;
using System.Collections.Generic;
using System.Diagnostics;
using Albumprinter.CorrelationTracking.Correlation.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Albumprinter.CorrelationTracking.Correlation.Logging
{
    [PublicAPI]
    public sealed class CorrelationLoggerFactory : ILoggerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public CorrelationLoggerFactory([NotNull] ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public void Dispose()
        {
            _loggerFactory.Dispose();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DecorationLogger(_loggerFactory.CreateLogger(categoryName));
        }

        public void AddProvider(ILoggerProvider provider)
        {
            _loggerFactory.AddProvider(provider);
        }

        private sealed class DecorationLogger : ILogger
        {
            private readonly ILogger _originalLogger;

            public DecorationLogger([NotNull] ILogger originalLogger)
            {
                _originalLogger = originalLogger ?? throw new ArgumentNullException(nameof(originalLogger));
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var correlationProperties = new Dictionary<string, object>
                {
                    [CorrelationKeys.ProcessId] = CorrelationScope.Current.ProcessId,
                    [CorrelationKeys.CorrelationId] = CorrelationScope.Current.CorrelationId,
                    [CorrelationKeys.RequestId] = CorrelationScope.Current.RequestId,
                    [CorrelationKeys.ActivityId] = Activity.Current.Id
                };

                using (_originalLogger.BeginScope(correlationProperties))
                {
                    _originalLogger.Log(logLevel, eventId, state, exception, formatter);
                }
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return _originalLogger.IsEnabled(logLevel);
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return _originalLogger.BeginScope(state);
            }
        }
    }
}
