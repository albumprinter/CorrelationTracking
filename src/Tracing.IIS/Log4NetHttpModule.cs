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

        public Log4NetHttpModule() : this(TrackingHttpModuleConfiguration.FromConfig("Log4NetHttpModule"))
        {
        }

        internal Log4NetHttpModule(TrackingHttpModuleConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            this.configuration = configuration;
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

            OnBeginRequest(new HttpContextWrapper(application.Context));
        }

        private void Application_EndRequest(object sender, EventArgs args)
        {
            var application = (HttpApplication) sender;

            OnEndRequest(new HttpContextWrapper(application.Context));
        }

        private void Application_Error(object sender, EventArgs args)
        {
            var application = (HttpApplication) sender;

            OnError(new HttpContextWrapper(application.Context));
        }

        public void OnBeginRequest(HttpContextBase context)
        {
            var request = context.Request;

            if (TrackingHttpModuleState.IsTrackable(context, configuration))
            {
                var state = TrackingHttpModuleState.AttachTo(context, configuration);
                var message = $"{request.HttpMethod} {request.Url.OriginalString}{Environment.NewLine}{state.GetInputHeaders()}{Environment.NewLine}{state.GetInputContent()}";
                Log.Debug(TruncateMessage(message));
            }
        }

        public void OnEndRequest(HttpContextBase context)
        {
            var request = context.Request;
            var response = context.Response;

            var state = TrackingHttpModuleState.DetachFrom(context);
            if (state != null)
            {
                var message = $"{response.Status} {request.Url.OriginalString} {state.Stopwatch.Elapsed.TotalMilliseconds:N1}ms{Environment.NewLine}{state.GetOutputHeaders()}{Environment.NewLine}{state.GetOutputContent()}";
                Log.Debug(TruncateMessage(message));
            }
        }

        public void OnError(HttpContextBase context)
        {
            var request = context.Request;
            var response = context.Response;

            var exception = context.Server.GetLastError();
            Log.Error(TruncateMessage($"{response.Status} {request.Url.OriginalString}"), exception);
        }

        private string TruncateMessage(string original)
        {
            var maxMessageSize = configuration.MaxMessageSize;
            if (maxMessageSize > 0 && maxMessageSize < original.Length)
                return original.Remove(maxMessageSize) + $" //LOG TRUNCATED from {original.Length} to {maxMessageSize} characters";
            return original;
        }
    }
}