using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;

namespace GIBS.Module.GiftCert.Repository
{
    public interface IGiftCertRepository
    {
        IEnumerable<Models.GiftCert> GetGiftCerts(int ModuleId);
        Models.GiftCert GetGiftCert(int GiftCertId);
        Models.GiftCert GetGiftCert(int GiftCertId, bool tracking);
        Models.GiftCert AddGiftCert(Models.GiftCert GiftCert);
        Models.GiftCert UpdateGiftCert(Models.GiftCert GiftCert);
        void DeleteGiftCert(int GiftCertId);
    }

    public class GiftCertRepository : IGiftCertRepository, ITransientService
    {
        private readonly IDbContextFactory<GiftCertContext> _factory;

        public GiftCertRepository(IDbContextFactory<GiftCertContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<Models.GiftCert> GetGiftCerts(int ModuleId)
        {
            using var db = _factory.CreateDbContext();
            return db.GiftCert.Where(item => item.ModuleId == ModuleId).ToList();
        }

        public Models.GiftCert GetGiftCert(int GiftCertId)
        {
            return GetGiftCert(GiftCertId, true);
        }

        public Models.GiftCert GetGiftCert(int GiftCertId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.GiftCert.Find(GiftCertId);
            }
            else
            {
                return db.GiftCert.AsNoTracking().FirstOrDefault(item => item.GiftCertId == GiftCertId);
            }
        }

        public Models.GiftCert AddGiftCert(Models.GiftCert GiftCert)
        {
            using var db = _factory.CreateDbContext();
            db.GiftCert.Add(GiftCert);
            db.SaveChanges();
            return GiftCert;
        }

        public Models.GiftCert UpdateGiftCert(Models.GiftCert GiftCert)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(GiftCert).State = EntityState.Modified;
            db.SaveChanges();
            return GiftCert;
        }

        public void DeleteGiftCert(int GiftCertId)
        {
            using var db = _factory.CreateDbContext();
            Models.GiftCert GiftCert = db.GiftCert.Find(GiftCertId);
            db.GiftCert.Remove(GiftCert);
            db.SaveChanges();
        }
    }
}
