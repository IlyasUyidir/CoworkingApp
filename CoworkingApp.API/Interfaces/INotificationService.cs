using System;
using System.Threading.Tasks;
using CoworkingApp.API.Models;

namespace CoworkingApp.API.Interfaces
{
    public interface INotificationService
    {
        Task SendBookingConfirmationAsync(User recipient, Reservation reservation);
        Task SendPaymentSuccessAsync(User recipient, Payment payment);
        Task SendCancellationNoticeAsync(User recipient, Reservation reservation);
    }
}