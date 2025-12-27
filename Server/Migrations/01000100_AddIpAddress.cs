using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.GiftCert.Repository;

namespace GIBS.Module.GiftCert.Migrations
{
    [DbContext(typeof(GiftCertContext))]
    [Migration("GIBS.Module.GiftCert.01.00.01.00")]
    public class AddUserId : MultiDatabaseMigration
    {
        public AddUserId(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
               name: "IP_Address",
               table: "GIBSGiftCert",
               nullable: true,
               maxLength: 45);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IP_Address",
               table: "GIBSGiftCert");
        }
    }
}