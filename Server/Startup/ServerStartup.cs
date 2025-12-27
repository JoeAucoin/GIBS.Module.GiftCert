using Microsoft.AspNetCore.Builder; 
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using GIBS.Module.GiftCert.Repository;
using GIBS.Module.GiftCert.Services;

namespace GIBS.Module.GiftCert.Startup
{
    public class ServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // not implemented
        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            // not implemented
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IGiftCertService, ServerGiftCertService>();
            services.AddDbContextFactory<GiftCertContext>(opt => { }, ServiceLifetime.Transient);
            
        }
    }
}
