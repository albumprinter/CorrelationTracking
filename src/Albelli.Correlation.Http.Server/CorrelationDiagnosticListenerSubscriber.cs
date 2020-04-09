using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Albumprinter.CorrelationTracking.Correlation.Core;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using CorrelationManager = Albumprinter.CorrelationTracking.Correlation.Core.CorrelationManager;

namespace Albelli.Correlation.Http.Server
{
    [PublicAPI]
    public sealed class CorrelationDiagnosticListenerSubscriber : IObserver<KeyValuePair<string, object>>
    {
        const string HttpRequestInStart = "Microsoft.AspNetCore.Hosting.HttpRequestIn.Start";
        const string HttpRequestInStop = "Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop";
        private readonly ActivitySpanId EMPTY_SPAN = ActivitySpanId.CreateFromString("ffffffffffffffff".AsSpan());
        private readonly ConcurrentDictionary<HttpContext, IDisposable> _ctxToDispose = new ConcurrentDictionary<HttpContext, IDisposable>();

        public CorrelationDiagnosticListenerSubscriber()
        {
            // We want to use the W3C format so we can be compatible with the standard as much as possible.
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        private void Start(HttpContext ctx)
        {
            if (ctx == null) return;

            var resolvedBackwardsCompatibleId = TryResolveCorrelationId(ctx, out var correlationId);

            // We are only going to set the Activity's correlation parent
            // if we don't have an existing parent already
            // and we have one that came from albelli's correlation.
            if (Activity.Current != null
                && Activity.Current.Parent == null
                && resolvedBackwardsCompatibleId)
            {
                Activity.Current.SetParentId(ActivityTraceId.CreateFromString(correlationId.ToString("N").AsSpan()), EMPTY_SPAN);
            }

            // We need to set some kind of id for the old system.
            // So, we will try to use the trace-id from the W3C standard if we can use it
            // Otherwise we will just generate a new guid instead
            if (!resolvedBackwardsCompatibleId)
            {
                if (Activity.Current != null && Activity.DefaultIdFormat == ActivityIdFormat.W3C)
                {
                    var currentTraceId = Activity.Current.TraceId.ToHexString();
                    if (Guid.TryParse(currentTraceId, out var w3CTraceIdAsGuid) && w3CTraceIdAsGuid != Guid.Empty)
                    {
                        correlationId = w3CTraceIdAsGuid;
                    }
                    else
                    {
                        correlationId = Guid.NewGuid();
                    }
                }
                else
                {
                    correlationId = Guid.NewGuid();
                }
            }

            _ctxToDispose[ctx] = CorrelationManager.Instance.UseScope(correlationId);
        }

        private void Stop(HttpContext ctx)
        {
            if (ctx == null) return;
            if (_ctxToDispose.TryRemove(ctx, out var disposable))
            {
                disposable.Dispose();
            }
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            if (string.Equals(value.Key, HttpRequestInStart, StringComparison.InvariantCultureIgnoreCase))
            {
                Start(value.Value as HttpContext);
            }
            else if (string.Equals(value.Key, HttpRequestInStop, StringComparison.InvariantCultureIgnoreCase))
            {
                Stop(value.Value as HttpContext);
            }
        }

        private bool TryResolveCorrelationId(HttpContext context, out Guid id)
        {
            if (context.Request.Headers.TryGetValue(CorrelationKeys.CorrelationId, out var headers)
                && headers.Count >= 1
                && Guid.TryParse(headers[0], out var correlationId))
            {
                id = correlationId;
                return true;
            }

            id = Guid.Empty;
            return false;
        }
    }
}
