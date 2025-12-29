using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoworkingApp.API.Data;
using CoworkingApp.API.DTOs;
using CoworkingApp.API.Enums;
using CoworkingApp.API.Interfaces;
using CoworkingApp.API.Models;

namespace CoworkingApp.API.Services
{
    public class ReservationService : IReservationService
    {
        private readonly CoworkingContext _context;
        private readonly INotificationService _notificationService; //

        public ReservationService(CoworkingContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<ReservationResponse> CreateReservationAsync(Guid memberId, CreateReservationRequest request)
        {
            // Fix: Use Transaction to prevent Race Conditions (Double Booking)
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var space = await _context.Spaces.FindAsync(request.SpaceId);
                if (space == null) throw new Exception("Space not found.");

                // Concurrency Check
                var hasConflict = await _context.Reservations
                    .AnyAsync(r => r.SpaceId == request.SpaceId 
                                && r.Status != ReservationStatus.CANCELLED
                                && r.StartDateTime < request.EndDateTime 
                                && r.EndDateTime > request.StartDateTime);

                if (hasConflict) throw new Exception("Space is not available for the selected dates.");

                var duration = request.EndDateTime - request.StartDateTime;
                var hours = (decimal)duration.TotalHours;
                var totalPrice = Math.Round(space.PricePerHour * hours, 2);

                var reservation = new Reservation
                {
                    ReservationId = Guid.NewGuid(),
                    SpaceId = request.SpaceId,
                    MemberId = memberId,
                    StartDateTime = request.StartDateTime,
                    EndDateTime = request.EndDateTime,
                    Status = ReservationStatus.PENDING,
                    TotalPrice = totalPrice,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();

                return MapToResponse(reservation, space.Name);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ReservationResponse>> GetMemberReservationsAsync(Guid memberId)
        {
            var reservations = await _context.Reservations
                .Include(r => r.Space)
                .Include(r => r.Payment)
                .Where(r => r.MemberId == memberId)
                .OrderByDescending(r => r.StartDateTime)
                .ToListAsync();

            return reservations.Select(r => MapToResponse(r, r.Space.Name)).ToList();
        }

        public async Task<bool> CancelReservationAsync(Guid reservationId, Guid memberId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Space)
                .Include(r => r.Member)
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null) throw new Exception("Reservation not found.");
            if (reservation.MemberId != memberId) throw new Exception("Unauthorized.");
            if (reservation.Status == ReservationStatus.CANCELLED) throw new Exception("Already cancelled.");
            
            // Logic: Handle Refund if previously CONFIRMED (Paid)
            if (reservation.Status == ReservationStatus.CONFIRMED && reservation.Payment != null)
            {
                // In a real app, call PaymentGateway.Refund(payment.TransactionId)
                reservation.Payment.Status = PaymentStatus.REFUNDED;
                Console.WriteLine($"[REFUND INITIATED] Refund of {reservation.Payment.Amount} for Transaction {reservation.Payment.TransactionId}");
            }

            reservation.Status = ReservationStatus.CANCELLED;
            await _context.SaveChangesAsync();

            // Connected Logic: Send Notification
            await _notificationService.SendCancellationNoticeAsync(reservation.Member, reservation);

            return true;
        }

        public async Task<ReservationResponse> GetReservationByIdAsync(Guid reservationId)
        {
             var reservation = await _context.Reservations
                .Include(r => r.Space)
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
                
             if (reservation == null) return null;
             return MapToResponse(reservation, reservation.Space.Name);
        }

        public async Task<bool> CheckInAsync(Guid reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Space)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null) throw new Exception("Reservation not found.");

            if (reservation.Status != ReservationStatus.CONFIRMED)
                throw new Exception("Reservation must be confirmed (paid) before check-in.");

            var now = DateTime.UtcNow;
            // Relaxed check-in window for testing (remove +/- 15 min constraints if needed)
            if (now < reservation.StartDateTime.AddMinutes(-15))
                throw new Exception("Too early to check in.");
            
            if (now > reservation.EndDateTime)
                throw new Exception("Reservation has expired.");

            reservation.Status = ReservationStatus.CHECKED_IN;
            reservation.CheckInTime = now;
            reservation.Space.Status = SpaceStatus.OCCUPIED;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckOutAsync(Guid reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Space)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null) throw new Exception("Reservation not found.");
            if (reservation.Status != ReservationStatus.CHECKED_IN)
                throw new Exception("Reservation is not currently checked in.");

            reservation.Status = ReservationStatus.COMPLETED;
            reservation.CheckOutTime = DateTime.UtcNow;
            reservation.Space.Status = SpaceStatus.AVAILABLE;

            await _context.SaveChangesAsync();
            return true;
        }

        private ReservationResponse MapToResponse(Reservation r, string spaceName)
        {
            return new ReservationResponse
            {
                ReservationId = r.ReservationId,
                SpaceId = r.SpaceId,
                SpaceName = spaceName,
                StartDateTime = r.StartDateTime,
                EndDateTime = r.EndDateTime,
                Status = r.Status,
                TotalPrice = r.TotalPrice,
                PaymentStatus = r.Payment != null ? r.Payment.Status : PaymentStatus.PENDING
            };
        }
    }
}