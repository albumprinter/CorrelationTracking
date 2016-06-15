using System;
using System.Web;

namespace Albumprinter.CorrelationTracking.Correlation.IIS
{
    public sealed class CorrelationHttpModule : IHttpModule
    {
        public void Init(HttpApplication application)
        {
            application.BeginRequest += Application_BeginRequest;
            application.EndRequest += Application_EndRequest;
        }

        private void Application_BeginRequest(object sender, EventArgs args)
        {
            var application = (HttpApplication) sender;
            var context = application.Context;
            var request = context.Request;
            var response = context.Response;

            // NOTE: restore or create the correlation id
            var correlationId = GetCorrelationId(request);
            var requestId = GetRequestId(context);

            response.Headers.Set(CorrelationKeys.CorrelationId, correlationId.ToString());
            response.Headers.Set(CorrelationKeys.RequestId, requestId.ToString());

            context.Items[typeof(CorrelationHttpModule).Name] = CorrelationManager.Instance.UseScope(correlationId, requestId);
            context.Items[typeof(CorrelationScope).Name] = CorrelationScope.Current;
        }

        private static Guid GetCorrelationId(HttpRequest request)
        {
            Guid correlationId;
            Guid.TryParse(request.Headers[CorrelationKeys.CorrelationId], out correlationId);
            if (correlationId == Guid.Empty)
            {
                correlationId = Guid.NewGuid();
            }
            return correlationId;
        }

        private static Guid GetRequestId(IServiceProvider serviceProvider)
        {
            var workerRequest = serviceProvider.GetService(typeof(HttpWorkerRequest)) as HttpWorkerRequest;
            var requestId = workerRequest != null ? workerRequest.RequestTraceIdentifier : Guid.NewGuid();
            if (requestId == Guid.Empty)
            {
                requestId = Guid.NewGuid();
            }
            return requestId;
        }

        private void Application_EndRequest(object sender, EventArgs eventArgs)
        {
            var application = (HttpApplication) sender;
            var context = application.Context;

            var disposable = context.Items[typeof(CorrelationHttpModule).Name] as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        public void Dispose()
        {
        }
    }
}
