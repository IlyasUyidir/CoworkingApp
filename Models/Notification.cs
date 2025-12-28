using System;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.Models
{
    public class Notification
    {
        public Guid NotificationId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public NotificationChannel Channel { get; set; }

        public Guid UserId { get; set; }
        public User Recipient { get; set; }
    }
}