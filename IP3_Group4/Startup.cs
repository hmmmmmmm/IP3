using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(IP3_Group4.Startup))]
namespace IP3_Group4
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
