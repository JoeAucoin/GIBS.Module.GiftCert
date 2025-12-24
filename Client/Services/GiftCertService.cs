using GIBS.Module.GiftCert.Models;
using Oqtane.Services;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
//using static System.Net.WebRequestMethods;


namespace GIBS.Module.GiftCert.Services
{
    public interface IGiftCertService
    {
        Task<List<Models.GiftCert>> GetGiftCertsAsync(int ModuleId);

        Task<Models.GiftCert> GetGiftCertAsync(int GiftCertId, int ModuleId);

        Task<Models.GiftCert> AddGiftCertAsync(Models.GiftCert GiftCert);

        Task<Models.GiftCert> UpdateGiftCertAsync(Models.GiftCert GiftCert);

        Task DeleteGiftCertAsync(int GiftCertId, int ModuleId);

      //  Task<string> CreatePayPalOrderAsync(Models.GiftCert giftCert);

        // Return the DTO with both OrderId and raw PayPal JSON
        Task<PayPalOrderResponseDto> CreatePayPalOrderAsync(Models.GiftCert giftCert);

        // Update the return type to string
        Task<string> CapturePayPalOrderAsync(string orderId, int moduleId);
    }

    public class GiftCertService : ServiceBase, IGiftCertService
    {
        public GiftCertService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("GiftCert");
        private string PayPalApiUrl => CreateApiUrl("PayPal");

       

        public async Task<List<Models.GiftCert>> GetGiftCertsAsync(int ModuleId)
        {
            List<Models.GiftCert> GiftCerts = await GetJsonAsync<List<Models.GiftCert>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), Enumerable.Empty<Models.GiftCert>().ToList());
            return GiftCerts.OrderBy(item => item.ToName).ToList();
        }

        public async Task<Models.GiftCert> GetGiftCertAsync(int GiftCertId, int ModuleId)
        {
            return await GetJsonAsync<Models.GiftCert>(CreateAuthorizationPolicyUrl($"{Apiurl}/{GiftCertId}/{ModuleId}", EntityNames.Module, ModuleId));
        }

        public async Task<Models.GiftCert> AddGiftCertAsync(Models.GiftCert GiftCert)
        {
            return await PostJsonAsync<Models.GiftCert>(CreateAuthorizationPolicyUrl($"{Apiurl}", EntityNames.Module, GiftCert.ModuleId), GiftCert);
        }

        public async Task<Models.GiftCert> UpdateGiftCertAsync(Models.GiftCert GiftCert)
        {
            return await PutJsonAsync<Models.GiftCert>(CreateAuthorizationPolicyUrl($"{Apiurl}/{GiftCert.GiftCertId}", EntityNames.Module, GiftCert.ModuleId), GiftCert);
        }

        public async Task DeleteGiftCertAsync(int GiftCertId, int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{GiftCertId}/{ModuleId}", EntityNames.Module, ModuleId));
        }

        public async Task<PayPalOrderResponseDto> CreatePayPalOrderAsync(Models.GiftCert giftCert)
        {
            // Calls PayPalController.CreateOrder and deserializes into PayPalOrderResponseDto
            return await PostJsonAsync<PayPalOrderResponseDto>(
                CreateAuthorizationPolicyUrl(PayPalApiUrl, EntityNames.Module, giftCert.ModuleId),
                giftCert);
        }

        private Task<T> PostJsonAsync<T>(string v, Models.GiftCert giftCert)
        {
            throw new NotImplementedException();
        }

        // Modify this implementation

        public async Task<string> CapturePayPalOrderAsync(string orderId, int moduleId)
        {
            var url = CreateAuthorizationPolicyUrl($"{PayPalApiUrl}/CaptureOrder/{orderId}", EntityNames.Module, moduleId);
            //  ServiceBase.HttpClient HttpClient => base.HttpClient;
            // Change 2: Corrected to use the HttpClient instance property from ServiceBase
            using var client = new HttpClient();
            var response = await client.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

    }
}