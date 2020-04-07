using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Albumprinter.CorrelationTracking.Correlation.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Albumprinter.CorrelationTracking.Correlation.Logging
{
    [PublicAPI]
    public sealed class ActivityBagLoggerFactory : ILoggerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly string _activityPrefix;

        public ActivityBagLoggerFactory([NotNull] ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _activityPrefix = CorrelationKeys.ActivityPrefix;
        }

        public void Dispose()
        {
            _loggerFactory.Dispose();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DecorationLogger(_loggerFactory.CreateLogger(categoryName), _activityPrefix);
        }

        public void AddProvider(ILoggerProvider provider)
        {
            _loggerFactory.AddProvider(provider);
        }

        private sealed class DecorationLogger : ILogger
        {
            [NotNull] private readonly ILogger _originalLogger;
            [NotNull] private readonly string _activityPrefix;

            public DecorationLogger([NotNull] ILogger originalLogger, [NotNull] string activityPrefix)
            {
                _originalLogger = originalLogger ?? throw new ArgumentNullException(nameof(originalLogger));
                _activityPrefix = activityPrefix ?? throw new ArgumentNullException(nameof(activityPrefix));
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var currentActivity = Activity.Current;
                if (currentActivity == null)
                {
                    _originalLogger.Log(logLevel, eventId, state, exception, formatter);
                    return;
                }

                var correlationProperties = Activity.Current.Baggage
                    .Where(x => x.Key.StartsWith(_activityPrefix, StringComparison.InvariantCultureIgnoreCase))
                    .GroupBy(x => x.Key)
                    .Select(group => group.First())
                    .ToDictionary(x => x.Key, x => (object)x.Value);

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
