using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Newtonsoft.Json;
using Albelli.CorrelationTracing.Amazon.Logging;

namespace Albelli.CorrelationTracing.Amazon
{
    public class LoggingPipelineHandler : PipelineHandler
    {
        private static readonly ILog _log = LogProvider.GetCurrentClassLogger();

        public override void InvokeSync(IExecutionContext executionContext)
        {
            if (_log.IsDebugEnabled())
            {
                _log.Debug(
                    $"Pre - {executionContext.RequestContext.RequestName}: {JsonConvert.SerializeObject(executionContext.RequestContext.OriginalRequest)}");
            }

            try
            {
                base.InvokeSync(executionContext);
                if (_log.IsDebugEnabled())
                {
                    _log.Debug(
                        $"Post - {executionContext.RequestContext.RequestName}: {JsonConvert.SerializeObject(executionContext.ResponseContext.HttpResponse)}");
                }
            }
            catch (HttpErrorResponseException amEx)
            {
                LogError(executionContext, amEx);
                throw;
            }
            catch (Exception ex)
            {
                _log.Error(
                    $"Post - {executionContext.RequestContext.RequestName}: {JsonConvert.SerializeObject(executionContext.ResponseContext.HttpResponse)}",
                    ex);
                throw;
            }
        }

        private static void LogError(IExecutionContext executionContext, HttpErrorResponseException amEx)
        {
            string content;
            using (var openResponse = amEx.Response.ResponseBody.OpenResponse())
            using (var streamReader = new System.IO.StreamReader(openResponse))
            {
                content = streamReader.ReadToEnd();
                Console.WriteLine(content);
            }

            var headers = (amEx.Response.GetHeaderNames() ?? new string[0]).ToDictionary(k => k, s => amEx.Response.GetHeaderValue(s));
            var errorResponse = new { amEx.Response.StatusCode, amEx.Response.ContentType, amEx.Response.ContentLength, Headers = headers, Content = content };
            _log.Error($"Post - {executionContext.RequestContext.RequestName}: {JsonConvert.SerializeObject(errorResponse)}", amEx);
        }

        public override async Task<T> InvokeAsync<T>(IExecutionContext executionContext)
        {
            if (_log.IsDebugEnabled())
            {
                _log.Debug(
                    $"Pre - {executionContext.RequestContext.RequestName}: {JsonConvert.SerializeObject(executionContext.RequestContext.OriginalRequest)}");
            }

            try
            {
                var result = await base.InvokeAsync<T>(executionContext);
                if (_log.IsDebugEnabled())
                {
                    _log.Debug(
                        $"Post - {executionContext.RequestContext.RequestName}: {JsonConvert.SerializeObject(executionContext.ResponseContext.HttpResponse)}");
                }

                return result;
            }
            catch (HttpErrorResponseException amEx)
            {
                LogError(executionContext, amEx);
                throw;
            }
            catch (Exception ex)
            {
                _log.Error(
                    $"Post - {executionContext.RequestContext.RequestName}: {JsonConvert.SerializeObject(executionContext.ResponseContext.HttpResponse)}",
                    ex);
                throw;
            }
        }
    }
}