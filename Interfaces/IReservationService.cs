using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoworkingApp.API.DTOs;

namespace CoworkingApp.API.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationResponse> CreateReservationAsync(Guid memberId, CreateReservationRequest request);
        Task<List<ReservationResponse>> GetMemberReservationsAsync(Guid memberId);
        Task<bool> CancelReservationAsync(Guid reservationId, Guid memberId);
        Task<ReservationResponse> GetReservationByIdAsync(Guid reservationId);
        
        // --- NEW METHODS ---
        Task<bool> CheckInAsync(Guid reservationId);
        Task<bool> CheckOutAsync(Guid reservationId);
    }
}