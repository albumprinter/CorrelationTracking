﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly ConcurrentDictionary<HttpContext, IDisposable> _ctxToDispose = new ConcurrentDictionary<HttpContext, IDisposable>();

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        private void Start(HttpContext ctx)
        {
            if (ctx == null) return;
            var id = ResolveCorrelationId(ctx);
            _ctxToDispose[ctx] = CorrelationManager.Instance.UseScope(id);
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

        private Guid ResolveCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationKeys.CorrelationId, out var headers)
                && headers.Count >= 1
                && Guid.TryParse(headers[0], out var correlationId))
            {
                return correlationId;
            }

            return Guid.NewGuid();
        }
    }
}