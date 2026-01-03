using System;

namespace CoworkingApp.API.DTOs
{
    public class ProcessPaymentRequest
    {
        public Guid ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Method { get; set; }
        public object Metadata { get; set; }
    }

    public class PaymentResponse
    {
        public Guid PaymentId { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
