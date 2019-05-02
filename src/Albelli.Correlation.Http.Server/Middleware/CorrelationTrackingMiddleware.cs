using Albumprinter.CorrelationTracking.Correlation.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Albelli.Correlation.Http.Server.Middleware
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class CorrelationTrackingMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationTrackingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            Guid correlationId = ResolveCorrelationId(context);

            using (CorrelationManager.Instance.UseScope(correlationId, Guid.NewGuid()))
            {
                await _next(context);
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
