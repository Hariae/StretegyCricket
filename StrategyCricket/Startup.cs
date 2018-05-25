using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(StrategyCricket.Startup))]
namespace StrategyCricket
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
