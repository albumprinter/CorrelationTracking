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
        private static readonly LoggingOptions DefaultOptions = new LoggingOptions
        {
            LogRequestBodyEnabled = true,
            LogResponseBodyEnabled = true,
            LogRequest = LogRequest,
            LogResponse = LogResponse,
            LogError = LogError
        };
        private readonly LoggingOptions _options;
        private static readonly ILog _log = LogProvider.GetCurrentClassLogger();

        public LoggingPipelineHandler(LoggingOptions options = null)
        {
            _options = options ?? DefaultOptions;
        }

        public override void InvokeSync(IExecutionContext executionContext)
        {
            var currentLogProvider = LogProvider.CurrentLogProvider;
            var operationId = Guid.NewGuid();
            using (currentLogProvider?.OpenMappedContext(CorrelationKeys.OperationId, operationId))
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
            var currentLogProvider = LogProvider.CurrentLogProvider;
            var operationId = Guid.NewGuid();
            using (currentLogProvider?.OpenMappedContext(CorrelationKeys.OperationId, operationId))
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

        private static void LogHttpException(IExecutionContext executionContext, HttpErrorResponseException amEx, Guid operationId)
        {
            string content;
            using (var openResponse = amEx.Response.ResponseBody.OpenResponse())
            using (var streamReader = new System.IO.StreamReader(openResponse))
            {
                content = streamReader.ReadToEnd();
                Console.WriteLine(content);
            }

            var headers = (amEx.Response.GetHeaderNames() ?? new string[0]).ToDictionary(k => k, s => amEx.Response.GetHeaderValue(s));
            var errorResponse = new {amEx.Response.StatusCode, amEx.Response.ContentType, amEx.Response.ContentLength, Headers = headers, Content = content};
            var errorEvent = new ErrorLoggingEventArg
            {
                Body = JsonConvert.SerializeObject(errorResponse),
                OperationId = operationId,
                RequestName = executionContext.RequestContext.RequestName,
                Scope = CorrelationScope.Current,
                Exception = amEx
            };
            LogError(errorEvent);
        }

        private static void LogGenericException(IExecutionContext executionContext, Guid operationId, Exception ex)
        {
            var errorEvent = new ErrorLoggingEventArg
            {
                Body = JsonConvert.SerializeObject(executionContext.ResponseContext.HttpResponse),
                OperationId = operationId,
                RequestName = executionContext.RequestContext.RequestName,
                Scope = CorrelationScope.Current,
                Exception = ex
            };
            LogError(errorEvent);
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
            LogResponse(responseEvent);
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
            LogRequest(requestEvent);
        }

        private static void LogRequest(LoggingEventArg loggingEventArg)
        {
            _log.Info($"Pre - {loggingEventArg.RequestName}: {loggingEventArg.Body}");
        }

        private static void LogResponse(LoggingEventArg loggingEventArg)
        {
            _log.Info($"Post - {loggingEventArg.RequestName}: {loggingEventArg.Body}");
        }

        private static void LogError(ErrorLoggingEventArg loggingEventArg)
        {
            _log.Error($"Post - {loggingEventArg.RequestName}: {loggingEventArg.Body}", loggingEventArg.Exception);
        }
    }
}