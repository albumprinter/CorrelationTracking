using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Albelli.CorrelationTracking.Correlation.Logging
{
    [PublicAPI]
    public sealed class ActivityBagTagDecoratingLogger<T> : ILogger<T>
    {
        [NotNull] private readonly ActivityBagTagDecoratingLoggerSettings _settings;
        [NotNull] private readonly ILogger _originalLogger;

        public ActivityBagTagDecoratingLogger([NotNull] ILoggerFactory factory, [NotNull] ActivityBagTagDecoratingLoggerSettings activityBagTagDecoratingLoggerSettings)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _settings = activityBagTagDecoratingLoggerSettings ?? throw new ArgumentNullException(nameof(activityBagTagDecoratingLoggerSettings));
            _originalLogger = factory.CreateLogger(nameof(T));
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

            if (_settings.ShouldLogBag != null)
            {
                var tagValues = currentActivity.Baggage
                    .Where(x => _settings.ShouldLogBag(x.Key))
                    .GroupBy(x => x.Key)
                    .Select(group => group.First());
                foreach (var keyValuePair in tagValues)
                {
                    correlationProperties[keyValuePair.Key] = keyValuePair.Value;
                }
            }

            if (_settings.ShouldLogTag != null)
            {
                var tagValues = currentActivity.Tags
                    .Where(x => _settings.ShouldLogTag(x.Key))
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

    public sealed class ActivityBagTagDecoratingLoggerSettings
    {
        public Predicate<string> ShouldLogBag { get; }
        public Predicate<string> ShouldLogTag { get; }

        public ActivityBagTagDecoratingLoggerSettings(Predicate<string> shouldLogBag, Predicate<string> shouldLogTag)
        {
            if (shouldLogBag == null && shouldLogTag == null)
                throw new ArgumentException("At least one predicate should be specified");

            ShouldLogBag = shouldLogBag;
            ShouldLogTag = shouldLogTag;
        }
    }
}