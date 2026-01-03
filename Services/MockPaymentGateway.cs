using System;
using System.Threading.Tasks;
using CoworkingApp.API.Interfaces;

namespace CoworkingApp.API.Services
{
    // Simple mock/local payment gateway for development. Returns success for all charges.
    public class MockPaymentGateway : IPaymentGateway
    {
        public Task<PaymentGatewayResult> ChargeAsync(PaymentGatewayRequest request)
        {
            var result = new PaymentGatewayResult
            {
                Success = true,
                TransactionId = "MOCK-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                Currency = request.Currency
            };

            return Task.FromResult(result);
        }
    }
}
