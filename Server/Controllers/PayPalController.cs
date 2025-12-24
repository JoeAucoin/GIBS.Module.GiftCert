using GIBS.Module.GiftCert.Models;
using GIBS.Module.GiftCert.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using PaypalServerSdk.Standard.Exceptions;
using System;
using System.Collections;
using System.Text.Json;
using System.Threading.Tasks;

namespace GIBS.Module.GiftCert.Controllers
{
    [Route("{alias}/api/[controller]")]
    public class PayPalController : ModuleControllerBase
    {
        private readonly IGiftCertService _giftCertService;

        public PayPalController(IGiftCertService giftCertService, ILogManager logger, IHttpContextAccessor accessor)
            : base(logger, accessor)
        {
            _giftCertService = giftCertService;
        }


        [HttpPost]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> CreateOrder([FromBody] GIBS.Module.GiftCert.Models.GiftCert giftCert)
        {
            try
            {
                // Step 1: Call the service to get the PayPal order response DTO
                var orderResponseDto = await _giftCertService.CreatePayPalOrderAsync(giftCert);

                // Step 2: Handle the case where the service returns a null DTO or an empty Order ID
                if (orderResponseDto == null || string.IsNullOrEmpty(orderResponseDto.OrderId))
                {
                    var errorResponse = new { message = "Failed to create PayPal order (empty response from service)." };
                    return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
                }

                // Step 3: Log the successful creation
                _logger.Log(LogLevel.Information, this, LogFunction.Security, "Created PayPal order with ID: {OrderId}", orderResponseDto.OrderId);

                // Step 4: Return the DTO as a JSON result
                return new JsonResult(orderResponseDto);
            }
            catch (ApiException e)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, e,
                    "PayPal API error creating order. Status: {StatusCode}", e.ResponseCode);

                return StatusCode(e.ResponseCode, new
                {
                    message = "PayPal API error creating order.",
                    statusCode = e.ResponseCode
                });
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, e, "Error creating PayPal order");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error creating PayPal order.",
                    error = e.Message
                });
            }
        }

        [HttpPost("CaptureOrder/{orderId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> CaptureOrder(string orderId)
        {
            try
            {
                // The module context is verified by the [Authorize] attribute.
                // The service layer on the server only needs the orderId.
                var resultJson = await _giftCertService.CapturePayPalOrderAsync(orderId, _entityId);

                // Return the raw JSON string with the correct content type
                return Content(resultJson, "application/json");
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, e, "Error capturing PayPal order {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error capturing PayPal order.",
                    error = e.Message
                });
            }
        }


    }
}