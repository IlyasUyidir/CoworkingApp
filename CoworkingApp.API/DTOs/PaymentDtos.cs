using System;
using System.ComponentModel.DataAnnotations;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.DTOs
{
    public class ProcessPaymentRequest
    {
        [Required]
        public Guid ReservationId { get; set; }
        
        [Required]
        public PaymentMethod Method { get; set; }
        
        public decimal Amount { get; set; }
    }

    public class PaymentResponse
    {
        public Guid PaymentId { get; set; }
        public PaymentStatus Status { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaidAt { get; set; }
    }
}