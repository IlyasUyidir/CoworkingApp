using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoworkingApp.API.DTOs;
using CoworkingApp.API.Interfaces;

namespace CoworkingApp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var result = await _reservationService.CreateReservationAsync(userId, request);
                return CreatedAtAction(nameof(GetMyReservations), new { id = result.ReservationId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-reservations")]
        public async Task<IActionResult> GetMyReservations()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _reservationService.GetMemberReservationsAsync(userId);
            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelReservation(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                await _reservationService.CancelReservationAsync(id, userId);
                return Ok(new { message = "Reservation cancelled successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/check-in")]
        public async Task<IActionResult> CheckIn(Guid id)
        {
            try
            {
                await _reservationService.CheckInAsync(id);
                return Ok(new { message = "Checked in successfully. Welcome!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/check-out")]
        public async Task<IActionResult> CheckOut(Guid id)
        {
            try
            {
                await _reservationService.CheckOutAsync(id);
                return Ok(new { message = "Checked out successfully. Thank you!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}