using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WebApp1.Startup))]

namespace WebApp1
{
    public sealed class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
