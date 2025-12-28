using System;
using System.Collections.Generic;

namespace CoworkingApp.API.Models
{
    public class Invoice
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }

        public Guid PaymentId { get; set; }
        public Payment Payment { get; set; }

        public ICollection<InvoiceItem> Items { get; set; }
    }
}