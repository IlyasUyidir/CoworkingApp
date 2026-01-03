using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using CoworkingApp.API.Data;
using CoworkingApp.API.Interfaces;
using CoworkingApp.API.Models;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly CoworkingContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AnalyticsService> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);

        public AnalyticsService(CoworkingContext context, IMemoryCache cache, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Analytics> GetDashboardMetricsAsync()
        {
            const string cacheKey = "analytics:dashboard_metrics";

            if (_cache.TryGetValue(cacheKey, out Analytics cached))
            {
                return cached;
            }

            try
            {
                // Total revenue: sum of completed payments
                var totalRevenue = await _context.Payments
                    .Where(p => p.Status == PaymentStatus.COMPLETED)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                // Total bookings: count of reservations excluding cancelled
                var totalBookings = await _context.Reservations
                    .CountAsync(r => r.Status != ReservationStatus.CANCELLED);

                // Occupancy: (confirmed + checked-in) / total spaces * 100
                var totalSpaces = await _context.Spaces.CountAsync();
                var activeReservations = await _context.Reservations
                    .CountAsync(r => r.Status == ReservationStatus.CONFIRMED || r.Status == ReservationStatus.CHECKED_IN);

                float occupancyRate = totalSpaces > 0
                    ? (float)activeReservations / totalSpaces * 100f
                    : 0f;

                // Average booking duration in minutes for completed reservations.
                // Try to compute server-side using EF.Functions.DateDiffMinute for relational providers.
                int avgDurationMinutes = 0;

                var providerName = _context.Database.ProviderName ?? string.Empty;
                var isInMemory = providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase);

                if (!isInMemory)
                {
                    try
                    {
                        // Assume StartDateTime and EndDateTime are non-nullable DateTime in your model.
                        var durations = await _context.Reservations
                            .Where(r => r.Status == ReservationStatus.COMPLETED)
                            .Select(r => EF.Functions.DateDiffMinute(r.StartDateTime, r.EndDateTime))
                            .ToListAsync();

                        avgDurationMinutes = durations.Any() ? (int)Math.Round(durations.Average()) : 0;
                    }
                    catch (Exception ex)
                    {
                        // If server-side computation isn't supported, fall back to client-side
                        _logger.LogWarning(ex, "Server-side DateDiffMinute failed; falling back to client-side duration calculation.");
                        isInMemory = true;
                    }
                }

                if (isInMemory)
                {
                    // Client-side fallback: calculate durations from start/end DateTime (non-nullable)
                    var completedReservations = await _context.Reservations
                        .Where(r => r.Status == ReservationStatus.COMPLETED)
                        .Select(r => new { r.StartDateTime, r.EndDateTime })
                        .ToListAsync();

                    var durations = completedReservations
                        .Select(r => (int)Math.Round((r.EndDateTime - r.StartDateTime).TotalMinutes))
                        .Where(d => d >= 0)
                        .ToList();

                    avgDurationMinutes = durations.Any() ? (int)Math.Round(durations.Average()) : 0;
                }

                var result = new Analytics
                {
                    AnalyticsId = Guid.NewGuid(),
                    TotalRevenue = totalRevenue,
                    TotalBookings = totalBookings,
                    OccupancyRate = occupancyRate,
                    AverageBookingDuration = avgDurationMinutes
                };

                // Cache the computed result for a short period
                _cache.Set(cacheKey, result, _cacheDuration);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to compute analytics metrics.");
                // Return neutral metrics on failure
                return new Analytics
                {
                    AnalyticsId = Guid.NewGuid(),
                    TotalRevenue = 0m,
                    TotalBookings = 0,
                    OccupancyRate = 0f,
                    AverageBookingDuration = 0
                };
            }
        }
    }
}
