using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoworkingApp.API.Data;
using CoworkingApp.API.DTOs;
using CoworkingApp.API.Enums;
using CoworkingApp.API.Models;

namespace CoworkingApp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly CoworkingContext _context;

        public ReviewController(CoworkingContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitReview([FromBody] CreateReviewRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // 1. Verify User has booked this space before and it is COMPLETED
            var hasBooking = await _context.Reservations
                .AnyAsync(r => r.MemberId == userId 
                            && r.SpaceId == request.SpaceId 
                            && r.Status == ReservationStatus.COMPLETED);

            if (!hasBooking)
            {
                return BadRequest("You can only review spaces you have used (Reservation must be COMPLETED).");
            }

            var review = new Review
            {
                ReviewId = Guid.NewGuid(),
                SpaceId = request.SpaceId,
                MemberId = userId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
                Helpful = 0
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Review submitted successfully." });
        }

        [HttpGet("space/{spaceId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSpaceReviews(Guid spaceId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Member)
                .Where(r => r.SpaceId == spaceId)
                .Select(r => new ReviewResponse
                {
                    ReviewId = r.ReviewId,
                    MemberName = $"{r.Member.FirstName} {r.Member.LastName}",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }
    }
}