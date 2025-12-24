using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using GIBS.Module.GiftCert.Services;
using Oqtane.Controllers;
using System.Net;
using System.Threading.Tasks;

namespace GIBS.Module.GiftCert.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class GiftCertController : ModuleControllerBase
    {
        private readonly IGiftCertService _GiftCertService;

        public GiftCertController(IGiftCertService GiftCertService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _GiftCertService = GiftCertService;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<Models.GiftCert>> Get(string moduleid)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return await _GiftCertService.GetGiftCertsAsync(ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.GiftCert> Get(int id, int moduleid)
        {
            Models.GiftCert GiftCert = await _GiftCertService.GetGiftCertAsync(id, moduleid);
            if (GiftCert != null && IsAuthorizedEntityId(EntityNames.Module, GiftCert.ModuleId))
            {
                return GiftCert;
            }
            else
            { 
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Get Attempt {GiftCertId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.GiftCert> Post([FromBody] Models.GiftCert GiftCert)
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, GiftCert.ModuleId))
            {
                GiftCert = await _GiftCertService.AddGiftCertAsync(GiftCert);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Post Attempt {GiftCert}", GiftCert);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                GiftCert = null;
            }
            return GiftCert;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.GiftCert> Put(int id, [FromBody] Models.GiftCert GiftCert)
        {
            if (ModelState.IsValid && GiftCert.GiftCertId == id && IsAuthorizedEntityId(EntityNames.Module, GiftCert.ModuleId))
            {
                GiftCert = await _GiftCertService.UpdateGiftCertAsync(GiftCert);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Put Attempt {GiftCert}", GiftCert);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                GiftCert = null;
            }
            return GiftCert;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleid)
        {
            Models.GiftCert GiftCert = await _GiftCertService.GetGiftCertAsync(id, moduleid);
            if (GiftCert != null && IsAuthorizedEntityId(EntityNames.Module, GiftCert.ModuleId))
            {
                await _GiftCertService.DeleteGiftCertAsync(id, GiftCert.ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Delete Attempt {GiftCertId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        [HttpPost("SendEmail")]
        [Authorize(Policy = PolicyNames.ViewModule)] // Allows unauthenticated users (e.g. guest checkout) to trigger the email
        public async Task SendEmail([FromBody] Models.EmailRequest request)
        {
            await _GiftCertService.SendHtmlEmailAsync(
                request.RecipientName,
                request.RecipientEmail,
                request.BccName,
                request.BccEmail,
                request.Subject,
                request.HtmlMessage
            );
        }
    }
}
