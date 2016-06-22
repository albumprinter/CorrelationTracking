using System.Web;
using Albumprinter.CorrelationTracking;

namespace WebApp1
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            CorrelationTrackingConfiguration.Initialize();
            WebApiConfig.Register();
            MvcConfig.Register();
        }
    }
}
