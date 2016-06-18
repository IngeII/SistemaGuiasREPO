using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GuiasOET.Startup))]
namespace GuiasOET
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
