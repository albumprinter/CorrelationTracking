using Albumprinter.CorrelationTracking.Correlation.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Albelli.Correlation.Http.Server.Middleware
{
    [PublicAPI]
    public sealed class LogRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Action<HttpDto, HttpContext> _logAction;
        private readonly Func<HttpContext, bool> _logBody;
        private readonly HashSet<string> _loggedHeaders;

        public LogRequestMiddleware(RequestDelegate next, [NotNull] ILogger<LogRequestMiddleware> logger)
            : this(next, new LoggingOptions<HttpDto>(), logger)
        {
        }

        public LogRequestMiddleware(RequestDelegate next, LoggingOptions<HttpDto> options, [NotNull] ILogger<LogRequestMiddleware> logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _next = next;
            _logAction = options.LogAction ?? new DefaultLogger(logger).Log;
            _logBody = options.LogBody ?? (_ => true);
            _loggedHeaders = new HashSet<string>(options.LoggedHeaders ?? new[] { CorrelationKeys.CorrelationId });
        }

        public async Task Invoke(HttpContext context)
        {
            var shouldLogBody = _logBody(context);
            string requestBodyText = null;
            MemoryStream requestBodyStream = null;
            Stream originalRequestBody = null;
            if (shouldLogBody)
            {
                requestBodyStream = new MemoryStream();
                originalRequestBody = context.Request.Body;

                await context.Request.Body.CopyToAsync(requestBodyStream);
                requestBodyStream.Seek(0, SeekOrigin.Begin);

                requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();
            }

            var url = InternalHttpHelper.GetDisplayUrl(context.Request);
            var operationId = InternalHttpHelper.GetOrCreateOperationId(context);
            var requestDto =
                new HttpDto
                {
                    Scope = CorrelationScope.Current,
                    Method = context.Request.Method,
                    Url = url,
                    Headers = InternalHttpHelper.GetHeaders(context.Request.Headers, _loggedHeaders),
                    Body = requestBodyText,
                    OperationId = operationId,
                };
            _logAction(requestDto, context);

            if (shouldLogBody)
            {
                requestBodyStream.Seek(0, SeekOrigin.Begin);
                context.Request.Body = requestBodyStream;
            }

            await _next(context);
            if (shouldLogBody)
            {
                context.Request.Body = originalRequestBody;
            }
        }
    }

    public class LoggingOptions<TLogDto> where TLogDto : HttpDto
    {
        public Action<TLogDto, HttpContext> LogAction { get; set; }
        public IReadOnlyCollection<string> LoggedHeaders { get; set; }
        public Func<HttpContext, bool> LogBody { get; set; }
    }

    public class HttpDto
    {
        public CorrelationScope Scope { get; set; }
        public Guid OperationId { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public IHeaderDictionary Headers { get; set; }
        public string Body { get; set; }
    }

    public class HttpResponseDto : HttpDto
    {
        public int StatusCode { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public static class InternalHttpHelper
    {
        public static Guid GetOrCreateOperationId(HttpContext context)
        {
            if (context.Items.TryGetValue(CorrelationKeys.OperationId, out var operationIdObj))
            {
                return (Guid)operationIdObj;
            }

            var operationId = Guid.NewGuid();
            context.Items[CorrelationKeys.OperationId] = operationId;
            return operationId;
        }

        public static string GetDisplayUrl(HttpRequest request)
        {
            return $"{request.Scheme}://{request.Host.Value}{request.PathBase.Value}{request.Path.Value}{request.QueryString.Value}";
        }

        public static IHeaderDictionary GetHeaders(IHeaderDictionary headers, HashSet<string> allowedHeaders)
        {
            var result = new HeaderDictionary();
            var allowed = headers.Where(h => allowedHeaders.Contains(h.Key));
            foreach (var pair in allowed)
            {
                result.Add(pair.Key, pair.Value);
            }

            return result;
        }

        public static string FormatHeaders(IHeaderDictionary headers)
        {
            var stringBuilder = new StringBuilder();

            foreach (var kvp in headers)
            {
                stringBuilder.AppendLine($"- {kvp.Key}: {kvp.Value}");
            }

            return stringBuilder.ToString();
        }
    }

    public static class ContextKeys
    {
        public const string Url = "Albelli.Correlation.Http.Server.Url";
        public const string StatusCode = "Albelli.Correlation.Http.Server.StatusCode";
        public const string Duration = "Albelli.Correlation.Http.Server.Duration";
    }

    internal sealed class DefaultLogger
    {
        private readonly ILogger _logger;

        public DefaultLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void Log(HttpDto dto, HttpContext context)
        {
            var contextProperties = new Dictionary<string, object>
            {
                [CorrelationKeys.OperationId] = dto.OperationId,
                [ContextKeys.Url] = dto.Url
            };
            using (_logger.BeginScope(contextProperties))
            {
                _logger.LogInformation($"{dto.Method} {dto.Url}\nHeaders:\n{InternalHttpHelper.FormatHeaders(dto.Headers)}\nContent:\n{dto.Body}");
            }
        }
    }
}