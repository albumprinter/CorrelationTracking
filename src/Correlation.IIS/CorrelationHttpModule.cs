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
            Guid correlationId;
            if (!Guid.TryParse(request.Headers[@"X-CorrelationId"], out correlationId))
            {
                correlationId = Guid.NewGuid();
            }
            response.Headers.Set(@"X-CorrelationId", correlationId.ToString());

            context.Items[typeof(CorrelationHttpModule).Name] = CorrelationManager.Instance.UseScope(correlationId);
            context.Items[typeof(CorrelationScope).Name] = CorrelationScope.Current;
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
