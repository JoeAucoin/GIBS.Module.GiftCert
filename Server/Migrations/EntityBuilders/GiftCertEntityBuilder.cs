using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;


namespace GIBS.Module.GiftCert.Migrations.EntityBuilders
{
    public class GiftCertEntityBuilder : AuditableBaseEntityBuilder<GiftCertEntityBuilder>
    {
        private const string _entityTableName = "GIBSGiftCert";
        private readonly PrimaryKey<GiftCertEntityBuilder> _primaryKey = new("PK_GIBSGiftCert", x => x.GiftCertId);
        private readonly ForeignKey<GiftCertEntityBuilder> _moduleForeignKey = new("FK_GIBSGiftCert_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

        public GiftCertEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
        }

        protected override GiftCertEntityBuilder BuildTable(ColumnsBuilder table)
        {
            // The Create() method in the base class will call the public column methods.
            // This method can be used for other purposes if needed, but for simple column definition, it can be left empty.
            
            GiftCertId = AddAutoIncrementColumn(table, "GiftCertId");
            ModuleId = AddIntegerColumn(table, "ModuleId");
            CertAmount = AddDecimalColumn(table, "CertAmount", 18, 2);
            ToName = AddStringColumn(table, "ToName", 100, false);
            MailTo = AddStringColumn(table, "MailTo", 100, true);
            MailToAddress = AddStringColumn(table, "MailToAddress", 254, true);
            MailToAddress1 = AddStringColumn(table, "MailToAddress1", 254, true);
            MailToCity = AddStringColumn(table, "MailToCity", 100, true);
            MailToState = AddStringColumn(table, "MailToState", 100, true);
            MailToZip = AddStringColumn(table, "MailToZip", 20, true);
            FromUserID = AddIntegerColumn(table, "FromUserID", true);
            FromName = AddStringColumn(table, "FromName", 100, true);
            FromPhone = AddStringColumn(table, "FromPhone", 50, true);
            FromEmail = AddStringColumn(table, "FromEmail", 100, true);
            Notes = AddMaxStringColumn(table, "Notes", true);
            isProcessed = AddBooleanColumn(table, "isProcessed", false);
            PP_PaymentId = AddStringColumn(table, "PP_PaymentId", 50, true);
            PP_Response = AddMaxStringColumn(table, "PP_Response", true);
            PaypalPaymentState = AddStringColumn(table, "PaypalPaymentState", 50, true);
            AddAuditableColumns(table);

            return this;
        }

       

        public OperationBuilder<AddColumnOperation> GiftCertId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> CertAmount { get; set; }
        public OperationBuilder<AddColumnOperation> ToName { get; set; }
        public OperationBuilder<AddColumnOperation> MailTo { get; set; }
        public OperationBuilder<AddColumnOperation> MailToAddress { get; set; }
        public OperationBuilder<AddColumnOperation> MailToAddress1 { get; set; }
        public OperationBuilder<AddColumnOperation> MailToCity { get; set; }
        public OperationBuilder<AddColumnOperation> MailToState { get; set; }
        public OperationBuilder<AddColumnOperation> MailToZip { get; set; }
        public OperationBuilder<AddColumnOperation> FromUserID { get; set; }
        public OperationBuilder<AddColumnOperation> FromName { get; set; }
        public OperationBuilder<AddColumnOperation> FromPhone { get; set; }
        public OperationBuilder<AddColumnOperation> FromEmail { get; set; }
        public OperationBuilder<AddColumnOperation> Notes { get; set; }
        public OperationBuilder<AddColumnOperation> isProcessed { get; set; }
        public OperationBuilder<AddColumnOperation> PP_PaymentId { get; set; }
        public OperationBuilder<AddColumnOperation> PP_Response { get; set; }
        public OperationBuilder<AddColumnOperation> PaypalPaymentState { get; set; }
    }
}