using System.Web;
using log4net.Config;

namespace WebApp1
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            XmlConfigurator.Configure();
            Correlation.CorrelationManager.Instance.ScopeInterceptors.Add(new Correlation.Log4net.Log4NetCorrelationScopeInterceptor());
            WebApiConfig.Register();
            MvcConfig.Register();
        }
    }
}
