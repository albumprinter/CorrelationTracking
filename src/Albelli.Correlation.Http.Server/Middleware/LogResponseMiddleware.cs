using Albumprinter.CorrelationTracking.Correlation.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Albelli.Correlation.Http.Server.Middleware
{
    [PublicAPI]
    public class LogResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Action<HttpResponseDto, HttpContext> _logAction;
        private readonly Func<HttpContext, bool> _logBody;
        private readonly HashSet<string> _loggedHeaders;

        public LogResponseMiddleware(RequestDelegate next, [NotNull] ILogger<LogResponseMiddleware> logger)
            : this(next, new LoggingOptions<HttpResponseDto>(), logger)
        {
        }

        public LogResponseMiddleware(RequestDelegate next, LoggingOptions<HttpResponseDto> options,
            [NotNull] ILogger<LogResponseMiddleware> logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _next = next;
            _logAction = options.LogAction ?? new DefaultResponseLogger(logger).Log;
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

    internal sealed class DefaultResponseLogger
    {
        private readonly ILogger _logger;

        public DefaultResponseLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void Log(HttpResponseDto dto, HttpContext context)
        {
            var contextProperties = new Dictionary<string, object>
            {
                [CorrelationKeys.OperationId] = dto.OperationId,
                [ContextKeys.Url] = dto.Url,
                [ContextKeys.StatusCode] = dto.StatusCode,
                [ContextKeys.Duration] = (int)Math.Ceiling(dto.Duration.TotalMilliseconds),
                [CorrelationKeys.OperationId] = dto.OperationId,
                [ContextKeys.Url] = dto.Url
            };

            using (_logger.BeginScope(contextProperties))
            {
                _logger.LogInformation($"StatusCode: {dto.StatusCode}\nfor: {dto.Method} {dto.Url}\nHeaders:\n{InternalHttpHelper.FormatHeaders(dto.Headers)}\nContent:\n{dto.Body}");
            }
        }
    }
}