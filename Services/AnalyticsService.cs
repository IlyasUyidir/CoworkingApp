using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoworkingApp.API.Data;
using CoworkingApp.API.Enums;
using CoworkingApp.API.Interfaces;
using CoworkingApp.API.Models;

namespace CoworkingApp.API.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly CoworkingContext _context;

        public AnalyticsService(CoworkingContext context)
        {
            _context = context;
        }

        public async Task<Analytics> GetDashboardMetricsAsync()
        {
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.COMPLETED)
                .SumAsync(p => p.Amount);

            var totalBookings = await _context.Reservations
                .CountAsync(r => r.Status != ReservationStatus.CANCELLED);

            // Calculate basic occupancy (Active reservations / Total Spaces)
            // This is a simplified metric for the dashboard
            var totalSpaces = await _context.Spaces.CountAsync();
            var activeReservations = await _context.Reservations
                .CountAsync(r => r.Status == ReservationStatus.CONFIRMED || r.Status == ReservationStatus.CHECKED_IN);
            
            float occupancyRate = totalSpaces > 0 ? (float)activeReservations / totalSpaces * 100 : 0;

            // Average Duration in minutes
            var durations = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.COMPLETED)
                .Select(r => EF.Functions.DateDiffMinute(r.StartDateTime, r.EndDateTime))
                .ToListAsync();
                
            var avgDuration = durations.Any() ? (int)durations.Average() : 0;

            return new Analytics
            {
                AnalyticsId = Guid.NewGuid(), // Generated on fly for report
                TotalRevenue = totalRevenue,
                TotalBookings = totalBookings,
                OccupancyRate = occupancyRate,
                AverageBookingDuration = avgDuration
            };
        }
    }
}