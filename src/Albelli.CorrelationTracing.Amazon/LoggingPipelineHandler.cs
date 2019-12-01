using Albelli.CorrelationTracing.Amazon.Logging;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Albelli.CorrelationTracing.Amazon
{
    public class LoggingPipelineHandler : PipelineHandler
    {
        private const string SkippedValue = "[SKIPPED]";
        private readonly LoggingOptions _options;
        private readonly ILog _log = LogProvider.GetCurrentClassLogger();

        public LoggingPipelineHandler(LoggingOptions options = null)
        {
            _options = options ?? new LoggingOptions
            {
                LogRequestBodyEnabled = true,
                LogResponseBodyEnabled = true,
                LogRequest = LogRequest,
                LogResponse = LogResponse,
                LogWarning = LogWarning,
                LogError = LogError
            };

            _options.LogRequest = _options.LogRequest ?? LogRequest;
            _options.LogResponse = _options.LogResponse ?? LogResponse;
            _options.LogWarning = _options.LogWarning ?? LogWarning;
            _options.LogError = _options.LogError ?? LogError;
        }

        public override void InvokeSync(IExecutionContext executionContext)
        {
            var operationId = Guid.NewGuid();
            using (LogProvider.OpenMappedContext(CorrelationKeys.OperationId, operationId))
            {
                LogRequest(executionContext, operationId);

                try
                {
                    base.InvokeSync(executionContext);
                    LogResponse(executionContext, operationId);
                }
                catch (HttpErrorResponseException amEx)
                {
                    LogHttpException(executionContext, amEx, operationId);

                    throw;
                }
                catch (Exception ex)
                {
                    LogGenericException(executionContext, operationId, ex);

                    throw;
                }
            }
        }

        public override async Task<T> InvokeAsync<T>(IExecutionContext executionContext)
        {
            var operationId = Guid.NewGuid();
            using (LogProvider.OpenMappedContext(CorrelationKeys.OperationId, operationId))
            {
                LogRequest(executionContext, operationId);

                try
                {
                    var result = await base.InvokeAsync<T>(executionContext);
                    LogResponse(executionContext, operationId);
                    return result;
                }
                catch (HttpErrorResponseException amEx)
                {
                    LogHttpException(executionContext, amEx, operationId);

                    throw;
                }
                catch (Exception ex)
                {
                    LogGenericException(executionContext, operationId, ex);

                    throw;
                }
            }
        }

        private void LogHttpException(IExecutionContext executionContext, HttpErrorResponseException amEx, Guid operationId)
        {
            string content;
            using (var openResponse = amEx.Response.ResponseBody.OpenResponse())
            using (var streamReader = new System.IO.StreamReader(openResponse))
            {
                content = streamReader.ReadToEnd();
            }

            var headers = (amEx.Response.GetHeaderNames() ?? new string[0]).ToDictionary(k => k, s => amEx.Response.GetHeaderValue(s));
            var errorResponse = new {amEx.Response.StatusCode, amEx.Response.ContentType, amEx.Response.ContentLength, Headers = headers, Content = content};
            var errorBody = JsonConvert.SerializeObject(errorResponse);

            var body = $"{errorBody}{Environment.NewLine}{amEx}";
            var requestName = executionContext.RequestContext.RequestName;
            if (HasNotReachedMaxRetries(executionContext))
            {
                var warningLoggingEventArg = new WarningLoggingEventArg
                {
                    Body = body,
                    OperationId = operationId,
                    RequestName = requestName,
                    Scope = CorrelationScope.Current,
                    Exception = amEx
                };
                _options.LogWarning(warningLoggingEventArg);
            }
            else
            {
                var errorEvent = new ErrorLoggingEventArg
                {
                    Body = body,
                    OperationId = operationId,
                    RequestName = requestName,
                    Scope = CorrelationScope.Current,
                    Exception = amEx
                };
                _options.LogError(errorEvent);
            }
        }

        private void LogGenericException(IExecutionContext executionContext, Guid operationId, Exception ex)
        {
            var errorBody = executionContext.ResponseContext.HttpResponse == null
                ? JsonConvert.SerializeObject(executionContext.ResponseContext.Response)
                : JsonConvert.SerializeObject(executionContext.ResponseContext.HttpResponse);

            var body = $"Generic error of type {ex.GetType().Name} {errorBody}{Environment.NewLine}{ex}";
            var requestName = executionContext.RequestContext.RequestName;
            if (HasNotReachedMaxRetries(executionContext))
            {
                var warningLoggingEventArg = new WarningLoggingEventArg
                {
                    Body = body,
                    OperationId = operationId,
                    RequestName = requestName,
                    Scope = CorrelationScope.Current,
                    Exception = ex
                };
                _options.LogWarning(warningLoggingEventArg);
            }
            else
            {
                var errorEvent = new ErrorLoggingEventArg
                {
                    Body = body,
                    OperationId = operationId,
                    RequestName = requestName,
                    Scope = CorrelationScope.Current,
                    Exception = ex
                };
                _options.LogError(errorEvent);
            }
        }

        private static bool HasNotReachedMaxRetries(IExecutionContext executionContext)
        {
            return executionContext.RequestContext.Retries < executionContext.RequestContext.ClientConfig.MaxErrorRetry;
        }

        private void LogResponse(IExecutionContext executionContext, Guid operationId)
        {
            var responseEvent = new LoggingEventArg
            {
                Body = _options.LogResponseBodyEnabled ? JsonConvert.SerializeObject(executionContext.ResponseContext.HttpResponse) : null,
                OperationId = operationId,
                RequestName = executionContext.RequestContext.RequestName,
                Scope = CorrelationScope.Current
            };

            _options.LogResponse(responseEvent);
        }

        private void LogRequest(IExecutionContext executionContext, Guid operationId)
        {
            var requestEvent = new LoggingEventArg
            {
                Body = _options.LogRequestBodyEnabled ? JsonConvert.SerializeObject(executionContext.RequestContext.OriginalRequest) : null,
                OperationId = operationId,
                RequestName = executionContext.RequestContext.RequestName,
                Scope = CorrelationScope.Current
            };

            _options.LogRequest(requestEvent);
        }

        private void LogRequest(LoggingEventArg loggingEventArg)
        {
            _log.Info($"Pre - {loggingEventArg.RequestName}: {loggingEventArg.Body ?? SkippedValue}");
        }

        private void LogResponse(LoggingEventArg loggingEventArg)
        {
            _log.Info($"Post - {loggingEventArg.RequestName}: {loggingEventArg.Body ?? SkippedValue}");
        }

        private void LogWarning(WarningLoggingEventArg loggingEventArg)
        {
            _log.Warn($"Post - {loggingEventArg.RequestName}: {loggingEventArg.Body ?? SkippedValue}", loggingEventArg.Exception);
        }
        
        private void LogError(ErrorLoggingEventArg loggingEventArg)
        {
            _log.Error($"Post - {loggingEventArg.RequestName}: {loggingEventArg.Body ?? SkippedValue}", loggingEventArg.Exception);
        }
    }
}