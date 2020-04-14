using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Albelli.CorrelationTracking.Correlation.Logging
{
    [PublicAPI]
    public sealed class ActivityBagTagLoggerFactory : ILoggerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly Predicate<string> _shouldLogTag;
        private readonly Predicate<string> _shouldLogBag;

        public ActivityBagTagLoggerFactory([NotNull] ILoggerFactory loggerFactory, [CanBeNull] Predicate<string> shouldLogBag = null, [CanBeNull] Predicate<string> shouldLogTag = null)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _shouldLogTag = shouldLogTag;
            _shouldLogBag = shouldLogBag;
        }

        public void Dispose()
        {
            _loggerFactory.Dispose();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DecorationLogger(_loggerFactory.CreateLogger(categoryName), _shouldLogBag, _shouldLogTag);
        }

        public void AddProvider(ILoggerProvider provider)
        {
            _loggerFactory.AddProvider(provider);
        }

        private sealed class DecorationLogger : ILogger
        {
            private readonly ILogger _originalLogger;
            private readonly Predicate<string> _shouldLogTag;
            private readonly Predicate<string> _shouldLogBag;

            public DecorationLogger([NotNull] ILogger originalLogger, [CanBeNull] Predicate<string> shouldLogBag, [CanBeNull] Predicate<string> shouldLogTag)
            {
                _originalLogger = originalLogger ?? throw new ArgumentNullException(nameof(originalLogger));
                _shouldLogTag = shouldLogTag;
                _shouldLogBag = shouldLogBag;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var currentActivity = Activity.Current;
                if (currentActivity == null)
                {
                    _originalLogger.Log(logLevel, eventId, state, exception, formatter);
                    return;
                }

                Dictionary<string, object> correlationProperties = new Dictionary<string, object>();

                if (_shouldLogBag != null)
                {
                    var tagValues = currentActivity.Baggage
                        .Where(x => _shouldLogBag(x.Key))
                        .GroupBy(x => x.Key)
                        .Select(group => group.First());
                    foreach (var keyValuePair in tagValues)
                    {
                        correlationProperties[keyValuePair.Key] = keyValuePair.Value;
                    }
                }

                if (_shouldLogTag != null)
                {
                    var tagValues = currentActivity.Tags
                        .Where(x => _shouldLogTag(x.Key))
                        .GroupBy(x => x.Key)
                        .Select(group => group.First());
                    foreach (var keyValuePair in tagValues)
                    {
                        correlationProperties[keyValuePair.Key] = keyValuePair.Value;
                    }
                }

                if (correlationProperties.Any())
                {
                    using (_originalLogger.BeginScope(correlationProperties))
                    {
                        _originalLogger.Log(logLevel, eventId, state, exception, formatter);
                    }
                }
                else
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
