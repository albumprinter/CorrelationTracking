using System;
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
        private const string HttpRequestInStart = "Microsoft.AspNetCore.Hosting.HttpRequestIn.Start";
        private const string HttpRequestInStop = "Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop";
        private const string ContextCorrelationScopeDisposables = nameof(ContextCorrelationScopeDisposables);
        private readonly ActivitySpanId EMPTY_SPAN = ActivitySpanId.CreateFromString("ffffffffffffffff".AsSpan());

        public CorrelationDiagnosticListenerSubscriber()
        {
            // We want to use the W3C format so we can be compatible with the standard as much as possible.
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        private void Start(HttpContext ctx)
        {
            if (ctx == null) return;

            Guid correlationId = default;

            var currentActivity = Activity.Current;
            // We can only manipulate the ids if they are in the W3C format
            if (currentActivity != null)
            {
                var resolvedBackwardsCompatibleId = TryResolveCorrelationId(ctx, out var guidFromOldSystem);
                if (currentActivity.IdFormat == ActivityIdFormat.W3C)
                {
                    // We want to set the current Activity's parent if:
                    // 1) We don't have an existing parent already -- this means a request came in with no correlation tracking in the new format
                    // 2) If we have a correlation id in the old format that we can set as the current request's parent
                    if (currentActivity.Parent == null && resolvedBackwardsCompatibleId)
                    {
                        currentActivity.SetParentId(
                            ActivityTraceId.CreateFromString(guidFromOldSystem.ToString("N").AsSpan()), EMPTY_SPAN);
                        correlationId = guidFromOldSystem;
                    }
                    else
                    {
                        // If the new request does have a parent, we should not override it.
                        // Instead we take the current trace-id, which happens to be something similar to our correlation id
                        // and use it in our old X-CorrelationId's place
                        var currentTraceId = currentActivity.TraceId.ToHexString();
                        if (Guid.TryParse(currentTraceId, out var w3CTraceIdAsGuid) && w3CTraceIdAsGuid != Guid.Empty)
                        {
                            correlationId = w3CTraceIdAsGuid;
                        }
                    }
                }
                else if (currentActivity.IdFormat == ActivityIdFormat.Hierarchical)
                {
                    correlationId = resolvedBackwardsCompatibleId ? guidFromOldSystem : Guid.NewGuid();
                    var parentTrace = ActivityTraceId.CreateFromString(correlationId.ToString("N").AsSpan());
                    // For whatever reason we received an id that we can't work with,
                    // we will try to override the parent anyway
                    try
                    {
                        currentActivity.SetParentId(parentTrace, EMPTY_SPAN);
                    }
                    catch
                    {
                        // If it fails, ignore it: there's no way we can restore the correlation
                    }
                }
            }

            if (correlationId == default)
                correlationId = Guid.NewGuid();
            ctx.Items[ContextCorrelationScopeDisposables] = CorrelationManager.Instance.UseScope(correlationId);
        }

        private void Stop(HttpContext ctx)
        {
            if (ctx == null) return;
            if (ctx.Items.TryGetValue(ContextCorrelationScopeDisposables, out var disposeScope))
            {
                var disposable = disposeScope as IDisposable;
                disposable?.Dispose();
                ctx.Items.Remove(ContextCorrelationScopeDisposables);
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

            id = default;
            return false;
        }
    }
}
