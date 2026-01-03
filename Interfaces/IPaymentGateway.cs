using System.Threading.Tasks;

namespace CoworkingApp.API.Interfaces
{
    public class PaymentGatewayRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Method { get; set; }
        public object Metadata { get; set; }
    }

    public class PaymentGatewayResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; }
        public string Currency { get; set; }
    }

    public interface IPaymentGateway
n    {
        Task<PaymentGatewayResult> ChargeAsync(PaymentGatewayRequest request);
    }
}
