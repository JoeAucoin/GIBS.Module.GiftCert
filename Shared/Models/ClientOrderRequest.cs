using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIBS.Module.GiftCert.Models
{
    public class ClientOrderRequest
    {
        public decimal? Amount { get; set; }
        public string ToName { get; set; }
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public int? GiftCertId { get; set; }
    }


}
