using System;
using System.Collections.Generic;
using System.Diagnostics;
using Albumprinter.CorrelationTracking.Correlation.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DiagnosticAdapter;

namespace Albelli.Correlation.Http.Client
{
    /// <summary>
    /// Diagnostic listener for backward compatibility with the old correlation id format.
    /// Usage:
    /// DiagnosticListener.AllListeners.Subscribe(new HttpRequestDiagnosticsListener());
    /// </summary>
    [PublicAPI]
    public class CompatibleCorrelationHttpRequestDiagnosticListener : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object>>
    {
        private const string HttpRequestOutStartEvent = "System.Net.Http.HttpRequestOut.Start";
        private const string DiagnosticListenerName = "HttpHandlerDiagnosticListener";
        private readonly List<IDisposable> _subscriptions;

        public CompatibleCorrelationHttpRequestDiagnosticListener()
        {
            _subscriptions = new List<IDisposable>();
        }

        void IObserver<DiagnosticListener>.OnNext(DiagnosticListener diagnosticListener)
        {
            if (diagnosticListener.Name.Equals(DiagnosticListenerName, StringComparison.OrdinalIgnoreCase))
            {
                var subscription = diagnosticListener.SubscribeWithAdapter(this);
                _subscriptions.Add(subscription);
            }
        }

        void IObserver<DiagnosticListener>.OnError(Exception error) { }

        void IObserver<DiagnosticListener>.OnCompleted()
        {
            _subscriptions.ForEach(x => x.Dispose());
            _subscriptions.Clear();
        }

        void IObserver<KeyValuePair<string, object>>.OnNext(KeyValuePair<string, object> pair) { }

        void IObserver<KeyValuePair<string, object>>.OnError(Exception error) { }

        void IObserver<KeyValuePair<string, object>>.OnCompleted() { }

        [UsedImplicitly]
        [DiagnosticName(HttpRequestOutStartEvent)]
        public virtual void OnHttpRequestOutStart(System.Net.Http.HttpRequestMessage request)
        {
            var currentActivity = Activity.Current;
            if (currentActivity != null && currentActivity.IdFormat == ActivityIdFormat.W3C)
            {
                if (Guid.TryParse(currentActivity.TraceId.ToHexString(), out var guidTraceId))
                    request.Headers.Add(CorrelationKeys.CorrelationId, guidTraceId.ToString());
            }
        }
    }
}
