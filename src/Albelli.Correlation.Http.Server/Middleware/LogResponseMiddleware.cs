using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Albelli.Correlation.Http.Server.Logging;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Microsoft.AspNetCore.Http;

namespace Albelli.Correlation.Http.Server.Middleware
{
    public class LogResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Action<HttpResponseDto> _logAction;
        private readonly HashSet<string> _loggedHeaders;

        public LogResponseMiddleware(RequestDelegate next) : this(next, new LoggingOptions<HttpResponseDto>())
        {
        }

        public LogResponseMiddleware(RequestDelegate next, LoggingOptions<HttpResponseDto> options)
        {
            _next = next;
            _logAction = options.LogAction ?? DefaultResponseLogger.LogWithLibLog;
            _loggedHeaders = new HashSet<string>(options.LoggedHeaders ?? new[]
            {
                CorrelationKeys.CorrelationId, "Content-Type", "Accept"
            });
        }

        public async Task Invoke(HttpContext context)
        {
            
            if (context.Request.Path.HasValue && context.Request.Path.Value.IndexOf("swagger", StringComparison.OrdinalIgnoreCase) == -1 &&
                context.Request.Path.Value.IndexOf("health", StringComparison.OrdinalIgnoreCase) == -1)
            {
                var bodyStream = context.Response.Body;

                var responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;

                InternalHttpHelper.SetStartTime(context, DateTime.UtcNow);
                await _next(context);
                var endTime = InternalHttpHelper.GetStartTime(context);

                responseBodyStream.Seek(0, SeekOrigin.Begin);
                var responseBody = new StreamReader(responseBodyStream).ReadToEnd();

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
                        Duration = DateTime.UtcNow - endTime,
                    };
                _logAction(responseDto);

                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(bodyStream);
            }
            else
            {
                await _next(context);
            }
        }
    }

    public static class DefaultResponseLogger
    {
        private static readonly ILog _log = LogProvider.GetCurrentClassLogger();
        public static void LogWithLibLog(HttpResponseDto dto)
        {
            var currentLogProvider = LogProvider.CurrentLogProvider;
            using (currentLogProvider?.OpenMappedContext(ContextKeys.StatusCode, dto.StatusCode))
            using (currentLogProvider?.OpenMappedContext(ContextKeys.Duration, (int)Math.Ceiling(dto.Duration.TotalMilliseconds)))
            using (currentLogProvider?.OpenMappedContext(ContextKeys.OperationId, Guid.NewGuid()))
            using (currentLogProvider?.OpenMappedContext(ContextKeys.Url, dto.Url))
            {
                _log.Info(() => $"StatusCode: {dto.StatusCode}\nfor: {dto.Method} {dto.Url}\nHeaders:\n{dto.Headers}\nContent:\n{dto.Body}");
            }
        }
    }
}