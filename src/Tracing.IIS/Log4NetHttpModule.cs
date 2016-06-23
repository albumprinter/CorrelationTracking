using System;
using System.Reflection;
using System.Web;
using log4net;

namespace Albumprinter.CorrelationTracking.Tracing.IIS
{
    public sealed class Log4NetHttpModule : IHttpModule
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly TrackingHttpModuleConfiguration configuration;

        public Log4NetHttpModule()
        {
            configuration = TrackingHttpModuleConfiguration.FromConfig("Log4NetHttpModule");
        }

        public void Init(HttpApplication application)
        {
            application.BeginRequest += Application_BeginRequest;
            application.EndRequest += Application_EndRequest;
            application.Error += Application_Error;
        }

        public void Dispose()
        {
        }

        private void Application_BeginRequest(object sender, EventArgs args)
        {
            var application = (HttpApplication) sender;
            var context = application.Context;
            var request = context.Request;

            if (configuration.AllowedUrls.IsMatch(request.Url.OriginalString))
            {
                var state = TrackingHttpModuleState.AttachTo(context, configuration);
                var message = $"{request.HttpMethod} {request.Url.OriginalString}{Environment.NewLine}{state.GetInputHeaders()}{Environment.NewLine}{state.GetInputContent()}";
                Log.Debug(message);
            }
        }

        private void Application_EndRequest(object sender, EventArgs args)
        {
            var application = (HttpApplication) sender;
            var context = application.Context;
            var request = context.Request;
            var response = context.Response;

            var state = TrackingHttpModuleState.DetachFrom(context);
            if (state != null)
            {
                var message = $"{response.Status} {request.Url.OriginalString} {state.Stopwatch.Elapsed.TotalMilliseconds:N1}ms{Environment.NewLine}{state.GetOutputHeaders()}{Environment.NewLine}{state.GetOutputContent()}";
                Log.Debug(message);
            }
        }

        private void Application_Error(object sender, EventArgs args)
        {
            var application = (HttpApplication) sender;
            var context = application.Context;
            var request = context.Request;
            var response = context.Response;

            var exception = context.Server.GetLastError();
            Log.Error($"{response.Status} {request.Url.OriginalString}", exception);
        }
    }
}