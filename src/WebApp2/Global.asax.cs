namespace WebApp2
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            WebApiConfig.Register();
            MvcConfig.Register();
        }
    }
}
