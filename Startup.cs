using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SurveyForm.Startup))]
namespace SurveyForm
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
