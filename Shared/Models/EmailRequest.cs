using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace GIBS.Module.GiftCert.Models
{
    public class EmailRequest
    {
        public string RecipientName { get; set; }
        public string RecipientEmail { get; set; }
        public string BccName { get; set; }
        public string BccEmail { get; set; }
        public string ReplyToName { get; set; }
        public string ReplyToEmail { get; set; }
        public string Subject { get; set; }
        public string HtmlMessage { get; set; }
    }
}
