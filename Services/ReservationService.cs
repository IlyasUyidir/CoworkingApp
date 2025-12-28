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

        public ReservationService(CoworkingContext context)
        {
            _context = context;
        }

        // ... Keep existing Create, Get, Cancel methods here ...

        public async Task<ReservationResponse> CreateReservationAsync(Guid memberId, CreateReservationRequest request)
        {
            // (Paste previous Create implementation here)
            var space = await _context.Spaces.FindAsync(request.SpaceId);
            if (space == null) throw new Exception("Space not found.");

            var hasConflict = await _context.Reservations
                .AnyAsync(r => r.SpaceId == request.SpaceId 
                            && r.Status != ReservationStatus.CANCELLED
                            && r.StartDateTime < request.EndDateTime 
                            && r.EndDateTime > request.StartDateTime);

            if (hasConflict) throw new Exception("Space is not available.");

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

            return MapToResponse(reservation, space.Name);
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
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null) throw new Exception("Reservation not found.");
            if (reservation.MemberId != memberId) throw new Exception("Unauthorized.");
            
            reservation.Status = ReservationStatus.CANCELLED;
            await _context.SaveChangesAsync();
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

        // --- NEW CHECK-IN / CHECK-OUT LOGIC ---

        public async Task<bool> CheckInAsync(Guid reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Space)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null) throw new Exception("Reservation not found.");

            // Validation: Must be CONFIRMED (paid) and within valid time window
            if (reservation.Status != ReservationStatus.CONFIRMED)
                throw new Exception("Reservation must be confirmed (paid) before check-in.");

            var now = DateTime.UtcNow;
            if (now < reservation.StartDateTime.AddMinutes(-15))
                throw new Exception("Too early to check in.");
            
            if (now > reservation.EndDateTime)
                throw new Exception("Reservation has expired.");

            // Update Logic
            reservation.Status = ReservationStatus.CHECKED_IN;
            reservation.CheckInTime = now;
            
            // Mark Space as Occupied
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

            // Update Logic
            reservation.Status = ReservationStatus.COMPLETED;
            reservation.CheckOutTime = DateTime.UtcNow;

            // Free up the Space
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