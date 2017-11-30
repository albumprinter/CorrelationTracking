using System;
using System.Web;
using Albumprinter.CorrelationTracking.Correlation.Core;

namespace Albumprinter.CorrelationTracking.Correlation.IIS
{
    public sealed class CorrelationHttpModuleClassic : IHttpModule
    {
        public void Init(HttpApplication application)
        {
            application.BeginRequest += Application_BeginRequest;
            application.EndRequest += Application_EndRequest;
        }

        private void Application_BeginRequest(object sender, EventArgs args)
        {
            var application = (HttpApplication)sender;
            var context = application.Context;
            var request = context.Request;
            var response = context.Response;

            // NOTE: restore or create the correlation id
            var correlationId = GetCorrelationId(request);
            var requestId = GetRequestId(context);

            // AddHeader is used as an alternative to Headers.Set,
            // which doesn't work in iis classic mode
            response.AddHeader(CorrelationKeys.CorrelationId, correlationId.ToString());
            response.AddHeader(CorrelationKeys.RequestId, requestId.ToString());

            context.Items[typeof(CorrelationHttpModuleClassic).Name] = CorrelationManager.Instance.UseScope(correlationId, requestId);
            context.Items[typeof(CorrelationScope).Name] = CorrelationScope.Current;
        }

        private static bool GuidTryParse(string strGuid, out Guid guid)
        {
            try
            {
                guid = new Guid(strGuid);
                return true;
            }
            catch (Exception e)
            {
                guid = default(Guid);
                return false;
            }
        }

        private static Guid GetCorrelationId(HttpRequest request)
        {
            Guid correlationId;
            GuidTryParse(request.Headers[CorrelationKeys.CorrelationId], out correlationId);
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
            var application = (HttpApplication)sender;
            var context = application.Context;

            var disposable = context.Items[typeof(CorrelationHttpModuleClassic).Name] as IDisposable;
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
