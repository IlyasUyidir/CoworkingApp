using System;
using System.Threading.Tasks;
using CoworkingApp.API.Data;
using CoworkingApp.API.Enums;
using CoworkingApp.API.Interfaces;
using CoworkingApp.API.Models;

namespace CoworkingApp.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly CoworkingContext _context;

        public NotificationService(CoworkingContext context)
        {
            _context = context;
        }

        public async Task SendBookingConfirmationAsync(User recipient, Reservation reservation)
        {
            await CreateNotificationAsync(
                recipient.UserId,
                NotificationType.BOOKING_CONFIRMATION,
                "Booking Confirmed",
                $"Your reservation for {reservation.Space.Name} on {reservation.StartDateTime:d} is confirmed."
            );
            
            // Mock Email Sending
            Console.WriteLine($"[EMAIL SENT] To: {recipient.Email} | Subject: Booking Confirmed");
        }

        public async Task SendPaymentSuccessAsync(User recipient, Payment payment)
        {
            await CreateNotificationAsync(
                recipient.UserId,
                NotificationType.PAYMENT_SUCCESS,
                "Payment Successful",
                $"We received your payment of {payment.Amount} {payment.Currency}."
            );
        }

        public async Task SendCancellationNoticeAsync(User recipient, Reservation reservation)
        {
            await CreateNotificationAsync(
                recipient.UserId,
                NotificationType.CANCELLATION,
                "Reservation Cancelled",
                $"Your reservation for {reservation.Space.Name} has been cancelled."
            );
        }

        private async Task CreateNotificationAsync(Guid userId, NotificationType type, string title, string message)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                SentAt = DateTime.UtcNow,
                Channel = NotificationChannel.IN_APP,
                ReadAt = null // Unread by default
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}