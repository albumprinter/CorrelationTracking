using Microsoft.Owin;
using Owin;
using WebApp2;

[assembly: OwinStartup(typeof(Startup))]

namespace WebApp2
{
    public sealed class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
