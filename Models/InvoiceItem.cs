using System;

namespace CoworkingApp.API.Models
{
    public class InvoiceItem
    {
        public Guid InvoiceItemId { get; set; } // Added ID for PK
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }

        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
    }
}