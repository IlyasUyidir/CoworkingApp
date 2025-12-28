using System;

namespace CoworkingApp.API.Models
{
    public class Analytics
    {
        public Guid AnalyticsId { get; set; } // Added ID for PK
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public float OccupancyRate { get; set; }
        public int AverageBookingDuration { get; set; }
        // Trends typically computed, not stored, but added here per diagram
    }
}