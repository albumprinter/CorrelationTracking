using System.Web;
using Albumprinter.CorrelationTracking;
using log4net.Config;

namespace WebApp1
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            XmlConfigurator.Configure();
            CorrelationTrackingConfiguration.Initialize();
            WebApiConfig.Register();
            MvcConfig.Register();
        }
    }
}
