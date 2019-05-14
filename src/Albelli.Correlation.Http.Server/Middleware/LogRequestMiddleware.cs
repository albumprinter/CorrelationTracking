using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Albelli.Correlation.Http.Server.Logging;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Microsoft.AspNetCore.Http;

namespace Albelli.Correlation.Http.Server.Middleware
{
    public class LogRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Action<HttpDto> _logAction;
        private readonly HashSet<string> _loggedHeaders;

        public LogRequestMiddleware(RequestDelegate next) : this(next, new LoggingOptions<HttpDto>())
        {
        }

        public LogRequestMiddleware(RequestDelegate next, LoggingOptions<HttpDto> options)
        {
            _next = next;
            _logAction = options.LogAction ?? DefaultLogger.LogWithLibLog;
            _loggedHeaders = new HashSet<string>(options.LoggedHeaders ?? new[] {CorrelationKeys.CorrelationId});
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue && context.Request.Path.Value.IndexOf("swagger", StringComparison.OrdinalIgnoreCase) == -1 &&
                context.Request.Path.Value.IndexOf("health", StringComparison.OrdinalIgnoreCase) == -1)
            {
                var requestBodyStream = new MemoryStream();
                var originalRequestBody = context.Request.Body;

                await context.Request.Body.CopyToAsync(requestBodyStream);
                requestBodyStream.Seek(0, SeekOrigin.Begin);

                var requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();
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
                _logAction(requestDto);

                requestBodyStream.Seek(0, SeekOrigin.Begin);
                context.Request.Body = requestBodyStream;

                await _next(context);
                context.Request.Body = originalRequestBody;
            }
            else
            {
                await _next(context);
            }
        }
    }

    public class LoggingOptions<TLogDto> where TLogDto: HttpDto
    {
        public Action<TLogDto> LogAction { get; set; }
        public IReadOnlyCollection<string> LoggedHeaders { get; set; }
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
            if (context.Items.TryGetValue(ContextKeys.OperationId, out var operationIdObj))
                return (Guid)operationIdObj;
            var operationId = Guid.NewGuid();
            context.Items[ContextKeys.OperationId] = operationId;
            return operationId;

        }

        public static void SetStartTime(HttpContext context, DateTime time)
        {
            context.Items[ContextKeys.RequestTime] = time;
        }

        public static DateTime GetStartTime(HttpContext context)
        {
            return (DateTime)context.Items[ContextKeys.RequestTime];
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
    }

    public static class ContextKeys
    {
        public const string OperationId = "Albelli.Correlation.OperationId"; // TODO: move me in base project
        public const string Url = "Albelli.Correlation.Http.Url";
        public const string StatusCode = "Albelli.Correlation.Http.StatusCode";
        public const string RequestTime = "Albelli.Correlation.Http.RequestTime";
        public const string Duration = "Albelli.Correlation.Http.Duration";
    }

    public static class DefaultLogger
    {
        private static readonly ILog _log = LogProvider.GetCurrentClassLogger();
        public static void LogWithLibLog(HttpDto dto)
        {
            var currentLogProvider = LogProvider.CurrentLogProvider;
            using (currentLogProvider?.OpenMappedContext(ContextKeys.OperationId, Guid.NewGuid()))
            using (currentLogProvider?.OpenMappedContext(ContextKeys.Url, dto.Url))
            {
                _log.Info(() => $"{dto.Method} {dto.Url}\nHeaders:\n{dto.Headers}\nContent:\n{dto.Body}");
            }
        }
    }
}