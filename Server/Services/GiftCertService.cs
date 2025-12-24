using GIBS.Module.GiftCert.Models;
using GIBS.Module.GiftCert.Repository;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;
using PaypalServerSdk.Standard;
using PaypalServerSdk.Standard.Authentication;
using PaypalServerSdk.Standard.Controllers;
using PaypalServerSdk.Standard.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Oqtane.Repository;

namespace GIBS.Module.GiftCert.Services
{
    public class ServerGiftCertService : IGiftCertService
    {
        private readonly IGiftCertRepository _GiftCertRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;
        private readonly ISqlRepository _sql;
        private readonly ISettingRepository _settings;

        public ServerGiftCertService(IGiftCertRepository GiftCertRepository, IUserPermissions userPermissions, ITenantManager tenantManager, ILogManager logger, IHttpContextAccessor accessor, ISqlRepository sql, ISettingRepository settings)
        {
            _GiftCertRepository = GiftCertRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
            _sql = sql;
            _settings = settings;
        }

        private (string PayPalPayee,string ClientId, string ClientSecret, PaypalServerSdk.Standard.Environment Environment) GetPayPalCredentials(int moduleId)
        {
            var settings = _settings.GetSettings("Module", moduleId).ToDictionary(s => s.SettingName, s => s.SettingValue);
            var isSandbox = bool.Parse(settings.GetValueOrDefault("PayPalSandboxMode", "true"));

            if (isSandbox)
            {
                return (
                    settings.GetValueOrDefault("PayPalSandboxPayee"),
                    settings.GetValueOrDefault("PayPalSandboxClientId"),
                    settings.GetValueOrDefault("PayPalSandboxClientSecret"),
                    PaypalServerSdk.Standard.Environment.Sandbox
                );
            }
            else
            {
                return (
                    settings.GetValueOrDefault("PayPalPayee"),
                    settings.GetValueOrDefault("OAuthClientId"),
                    settings.GetValueOrDefault("OAuthClientSecret"),
                    PaypalServerSdk.Standard.Environment.Production
                );
            }
        }

        public Task<List<Models.GiftCert>> GetGiftCertsAsync(int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_GiftCertRepository.GetGiftCerts(ModuleId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Get Attempt {ModuleId}", ModuleId);
                return null;
            }
        }

        public Task<Models.GiftCert> GetGiftCertAsync(int GiftCertId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_GiftCertRepository.GetGiftCert(GiftCertId));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Get Attempt {GiftCertId} {ModuleId}", GiftCertId, ModuleId);
                return null;
            }
        }

        public Task<Models.GiftCert> AddGiftCertAsync(Models.GiftCert GiftCert)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, GiftCert.ModuleId, PermissionNames.View))
            {
                GiftCert = _GiftCertRepository.AddGiftCert(GiftCert);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "GiftCert Added {GiftCert}", GiftCert);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Add Attempt {GiftCert}", GiftCert);
                GiftCert = null;
            }
            return Task.FromResult(GiftCert);
        }

        public Task<Models.GiftCert> UpdateGiftCertAsync(Models.GiftCert GiftCert)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, GiftCert.ModuleId, PermissionNames.View))
            {
                GiftCert = _GiftCertRepository.UpdateGiftCert(GiftCert);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "GiftCert Updated {GiftCert}", GiftCert);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Update Attempt {GiftCert}", GiftCert);
                GiftCert = null;
            }
            return Task.FromResult(GiftCert);
        }

        public Task DeleteGiftCertAsync(int GiftCertId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                _GiftCertRepository.DeleteGiftCert(GiftCertId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "GiftCert Deleted {GiftCertId}", GiftCertId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Delete Attempt {GiftCertId} {ModuleId}", GiftCertId, ModuleId);
            }
            return Task.CompletedTask;
        }

        public async Task<PayPalOrderResponseDto> CreatePayPalOrderAsync(Models.GiftCert giftCert)
        {
            var (payPalPayee, clientId, clientSecret, environment) = GetPayPalCredentials(giftCert.ModuleId);

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, "PayPal client ID or secret is not configured for module {ModuleId}", giftCert.ModuleId);
                throw new System.Exception("PayPal is not configured.");
            }

            var payPalClient = new PaypalServerSdkClient.Builder()
                .ClientCredentialsAuth(new ClientCredentialsAuthModel.Builder(clientId, clientSecret).Build())
                .Environment(environment)
                .Build();

            var ordersController = payPalClient.OrdersController;

            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, giftCert.ModuleId, PermissionNames.View))
            {
                try
                {
                    var orderRequest = new OrderRequest
                    {
                        Intent = CheckoutPaymentIntent.Capture,
                        PurchaseUnits = new List<PurchaseUnitRequest>
                        {
                            new PurchaseUnitRequest
                            {
                                Description = $"Gift certificate for {giftCert.ToName ?? "Gift"}",
                                InvoiceId = giftCert.GiftCertId.ToString(),
                              //  CustomId = giftCert.GiftCertId.ToString(),
                                Amount = new AmountWithBreakdown
                                {
                                    CurrencyCode = "USD",
                                    MValue = giftCert.CertAmount.ToString("F2")
                                },
                                Payee = new PayeeBase
                                {
                                    EmailAddress = payPalPayee.ToString() 
                                  //  EmailAddress = "joe-facilitator@gibs.com"
                                }
                            }
                        },
                        
                        Payer = new PaypalServerSdk.Standard.Models.Payer
                        {
                            Name = new Name
                            {
                                GivenName = giftCert.FromName ?? "Customer"
                            },
                            EmailAddress = giftCert.FromEmail
                        }
                    };

                    // Log the request payload
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "PayPal Order Request: {PayPalRequest}", JsonSerializer.Serialize(orderRequest));

                    var createOrderInput = new CreateOrderInput { Body = orderRequest };
                    var response = await ordersController.CreateOrderAsync(createOrderInput);

                    var fullOrder = response.Data;
                    return new PayPalOrderResponseDto
                    {
                        OrderId = fullOrder.Id,
                        RawOrderJson = JsonSerializer.Serialize(fullOrder)
                    };
                }
                catch (System.Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error Creating PayPal Order {Error}", ex.Message);
                    throw;
                }
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized PayPal Order Create Attempt {GiftCert}", giftCert);
            return null;
        }

        public async Task<string> CapturePayPalOrderAsync(string orderId, int moduleId)
        {
            var (payPalPayee, clientId, clientSecret, environment) = GetPayPalCredentials(moduleId);

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, "PayPal client ID or secret is not configured for module {ModuleId}", moduleId);
                throw new System.Exception("PayPal is not configured.");
            }

            var payPalClient = new PaypalServerSdkClient.Builder()
                .ClientCredentialsAuth(new ClientCredentialsAuthModel.Builder(clientId, clientSecret).Build())
                .Environment(environment)
                .Build();

            var ordersController = payPalClient.OrdersController;

            // Authorization is already handled by the controller, but we accept moduleId to match the interface
            try
            {
                var captureOrderInput = new CaptureOrderInput { Id = orderId };
                var response = await ordersController.CaptureOrderAsync(captureOrderInput);
                var capturedOrder = response.Data;

                // Serialize the entire captured order object to a JSON string
                return JsonSerializer.Serialize(capturedOrder);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error Capturing PayPal Order {Error}", ex.Message);
                throw;
            }
        }
    }
}