using System.Web;
using Albumprinter.CorrelationTracking;
using Albumprinter.CorrelationTracking.Correlation.Log4net;
using log4net.Config;

namespace WebApp1
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            XmlConfigurator.Configure();
            CorrelationManager.Instance.ScopeInterceptors.Add(new Log4NetCorrelationScopeInterceptor());
            WebApiConfig.Register();
            MvcConfig.Register();
        }
    }
}
