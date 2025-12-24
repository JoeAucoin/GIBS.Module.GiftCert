using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Infrastructure;
using Oqtane.Repository.Databases.Interfaces;

namespace GIBS.Module.GiftCert.Repository
{
    public class GiftCertContext : DBContextBase, ITransientService, IMultiDatabase
    {
        public virtual DbSet<Models.GiftCert> GiftCert { get; set; }

        public GiftCertContext(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies)
        {
            // ContextBase handles multi-tenant database connections
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Models.GiftCert>().ToTable(ActiveDatabase.RewriteName("GIBSGiftCert"));
        }
    }
}
