using HumbleBundleBotRegistration.Services;
using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HumbleBundleBotRegistration
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddTransient<IWebhookService, WebhookService>();

        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
