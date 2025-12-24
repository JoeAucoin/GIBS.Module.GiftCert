using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace GIBS.Module.GiftCert.Models
{
    [Table("GIBSGiftCert")]
    public class GiftCert : IAuditable
    {
        [Key]
        public int GiftCertId { get; set; }
        public int ModuleId { get; set; }
        public decimal CertAmount { get; set; }
        public string ToName { get; set; }
        public string MailTo { get; set; }
        public string MailToAddress { get; set; }
        public string MailToAddress1 { get; set; }
        public string MailToCity { get; set; }
        public string MailToState { get; set; }
        public string MailToZip { get; set; }
        public int? FromUserID { get; set; }
        public string FromName { get; set; }
        public string FromPhone { get; set; }
        public string FromEmail { get; set; }
        public string Notes { get; set; }
        public bool isProcessed { get; set; }
        public string PP_PaymentId { get; set; }
        public string PP_Response { get; set; }
        public string PaypalPaymentState { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
