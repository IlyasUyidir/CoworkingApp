using System;
using System.ComponentModel.DataAnnotations;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.DTOs
{
    public class CreateReservationRequest
    {
        [Required]
        public Guid SpaceId { get; set; }
        
        [Required]
        public DateTime StartDateTime { get; set; }
        
        [Required]
        public DateTime EndDateTime { get; set; }
    }

    public class ReservationResponse
    {
        public Guid ReservationId { get; set; }
        public Guid SpaceId { get; set; }
        public string SpaceName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public ReservationStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}