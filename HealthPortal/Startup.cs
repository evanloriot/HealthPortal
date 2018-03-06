using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HealthPortal.Startup))]
namespace HealthPortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
