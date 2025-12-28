using System;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.Models
{
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime PaidAt { get; set; }

        public Guid ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        
        public Invoice Invoice { get; set; }
    }
}