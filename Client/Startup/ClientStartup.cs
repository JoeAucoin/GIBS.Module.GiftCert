using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Oqtane.Services;
using GIBS.Module.GiftCert.Services;

namespace GIBS.Module.GiftCert.Startup
{
    public class ClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            if (!services.Any(s => s.ServiceType == typeof(IGiftCertService)))
            {
                services.AddScoped<IGiftCertService, GiftCertService>();
            }
        }
    }
}
