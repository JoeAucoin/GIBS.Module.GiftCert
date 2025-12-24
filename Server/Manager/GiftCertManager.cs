using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Enums;
using Oqtane.Repository;
using GIBS.Module.GiftCert.Repository;
using System.Threading.Tasks;

namespace GIBS.Module.GiftCert.Manager
{
    public class GiftCertManager : MigratableModuleBase, IInstallable, IPortable, ISearchable
    {
        private readonly IGiftCertRepository _GiftCertRepository;
        private readonly IDBContextDependencies _DBContextDependencies;

        public GiftCertManager(IGiftCertRepository GiftCertRepository, IDBContextDependencies DBContextDependencies)
        {
            _GiftCertRepository = GiftCertRepository;
            _DBContextDependencies = DBContextDependencies;
        }

        public bool Install(Tenant tenant, string version)
        {
            return Migrate(new GiftCertContext(_DBContextDependencies), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            return Migrate(new GiftCertContext(_DBContextDependencies), tenant, MigrationType.Down);
        }

        public string ExportModule(Oqtane.Models.Module module)
        {
            string content = "";
            List<Models.GiftCert> GiftCerts = _GiftCertRepository.GetGiftCerts(module.ModuleId).ToList();
            if (GiftCerts != null)
            {
                content = JsonSerializer.Serialize(GiftCerts);
            }
            return content;
        }

        public void ImportModule(Oqtane.Models.Module module, string content, string version)
        {
            List<Models.GiftCert> GiftCerts = null;
            if (!string.IsNullOrEmpty(content))
            {
                GiftCerts = JsonSerializer.Deserialize<List<Models.GiftCert>>(content);
            }
            if (GiftCerts != null)
            {
                foreach(var GiftCert in GiftCerts)
                {
                    _GiftCertRepository.AddGiftCert(new Models.GiftCert { ModuleId = module.ModuleId, ToName = GiftCert.ToName });
                }
            }
        }

        public Task<List<SearchContent>> GetSearchContentsAsync(PageModule pageModule, DateTime lastIndexedOn)
        {
           var searchContentList = new List<SearchContent>();

           foreach (var GiftCert in _GiftCertRepository.GetGiftCerts(pageModule.ModuleId))
           {
               if (GiftCert.ModifiedOn >= lastIndexedOn)
               {
                   searchContentList.Add(new SearchContent
                   {
                       EntityName = "GIBSGiftCert",
                       EntityId = GiftCert.GiftCertId.ToString(),
                       Title = GiftCert.ToName,
                       Body = GiftCert.ToName,
                       ContentModifiedBy = GiftCert.ModifiedBy,
                       ContentModifiedOn = GiftCert.ModifiedOn
                   });
               }
           }

           return Task.FromResult(searchContentList);
        }
    }
}
