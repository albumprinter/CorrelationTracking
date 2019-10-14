using Albelli.Correlation.Http.Server.Logging;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace Albelli.Correlation.Http.Server.Middleware
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class LogResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Action<HttpResponseDto, HttpContext> _logAction;
        private readonly Func<HttpContext, bool> _logBody;
        private readonly HashSet<string> _loggedHeaders;

        public LogResponseMiddleware(RequestDelegate next) : this(next, new LoggingOptions<HttpResponseDto>())
        {
        }

        public LogResponseMiddleware(RequestDelegate next, LoggingOptions<HttpResponseDto> options)
        {
            _next = next;
            _logAction = options.LogAction ?? DefaultResponseLogger.LogWithLibLog;
            _logBody = options.LogBody ?? (_ => true);
            _loggedHeaders = new HashSet<string>(options.LoggedHeaders ?? new[]
            {
                CorrelationKeys.CorrelationId, "Content-Type", "Accept"
            });
        }

        public async Task Invoke(HttpContext context)
        {
            var shouldLogBody = _logBody(context);
            Stream bodyStream = null;
            MemoryStream responseBodyStream = null;
            if (shouldLogBody)
            {
                bodyStream = context.Response.Body;

                responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;
            }

            var stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();

            string responseBody = null;
            if (shouldLogBody)
            {
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                responseBody = new StreamReader(responseBodyStream).ReadToEnd();
            }

            var url = InternalHttpHelper.GetDisplayUrl(context.Request);
            var operationId = InternalHttpHelper.GetOrCreateOperationId(context);
            var responseDto =
                new HttpResponseDto
                {
                    Scope = CorrelationScope.Current,
                    Method = context.Request.Method,
                    Url = url,
                    Headers = InternalHttpHelper.GetHeaders(context.Response.Headers, _loggedHeaders),
                    Body = responseBody,
                    OperationId = operationId,
                    StatusCode = context.Response.StatusCode,
                    Duration = stopwatch.Elapsed,
                };
            _logAction(responseDto, context);

            if (shouldLogBody)
            {
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(bodyStream);
            }
        }
    }

    public static class DefaultResponseLogger
    {
        private static readonly ILog _log = LogProvider.GetCurrentClassLogger();
        public static void LogWithLibLog(HttpResponseDto dto, HttpContext context)
        {
            using (LogProvider.OpenMappedContext(ContextKeys.StatusCode, dto.StatusCode))
            using (LogProvider.OpenMappedContext(ContextKeys.Duration, (int)Math.Ceiling(dto.Duration.TotalMilliseconds)))
            using (LogProvider.OpenMappedContext(CorrelationKeys.OperationId, dto.OperationId))
            using (LogProvider.OpenMappedContext(ContextKeys.Url, dto.Url))
            {
                _log.Info(() => $"StatusCode: {dto.StatusCode}\nfor: {dto.Method} {dto.Url}\nHeaders:\n{InternalHttpHelper.FormatHeaders(dto.Headers)}\nContent:\n{dto.Body}");
            }
        }
    }
}