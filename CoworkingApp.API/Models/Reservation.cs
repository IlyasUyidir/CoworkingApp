using System;
using System.Collections.Generic;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.Models
{
    public class Reservation
    {
        public Guid ReservationId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public ReservationStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        public Guid MemberId { get; set; }
        public Member Member { get; set; }

        public Guid SpaceId { get; set; }
        public Space Space { get; set; }
        
        // One-to-One Payment relationship
        public Payment Payment { get; set; }
    }
}