using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace GIBS.Module.GiftCert.Models
{
    public class OrderDetails
    {
        // Core Transactional Information
        public string OrderId { get; set; }
        public string Status { get; set; } // e.g., "CREATED", "COMPLETED"
        public string Intent { get; set; } // e.g., "CAPTURE", "AUTHORIZE"
        public DateTime DateCreated { get; set; }

        // Financial Information
        public decimal TotalAmount { get; set; }
        public string CurrencyCode { get; set; } // e.g., "USD"

        // Payer and Shipping Details
        public Payer PayerInfo { get; set; }
        public Address ShippingAddress { get; set; }

        // Line Items (The actual products being purchased)
        public List<LineItem> Items { get; set; }

        public OrderDetails()
        {
            Items = new List<LineItem>();
        }
    }
    public class Payer
    {
        public string PayerId { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
    }

    // Helper class to structure Address information
    public class Address
    {
        public string RecipientName { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; } // Optional street address line 2
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; } // e.g., "US"
    }

    // Helper class to structure individual Line Items (products)
    public class LineItem
    {
        public string Name { get; set; }
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public decimal UnitAmount { get; set; }
        public string Description { get; set; }
        // Optional field for type of good (e.g., 'PHYSICAL_GOODS', 'DIGITAL_GOODS')
        public string Type { get; set; }
    }

    public class PayPalOrderResponseDto
    {
        public string OrderId { get; set; }
        public string RawOrderJson { get; set; }
    }
}
