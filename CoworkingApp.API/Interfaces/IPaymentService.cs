using System;
using System.Threading.Tasks;
using CoworkingApp.API.DTOs;

namespace CoworkingApp.API.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request);
    }
}