using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CoworkingApp.API.Data;
using CoworkingApp.API.DTOs;
using CoworkingApp.API.Enums;
using CoworkingApp.API.Interfaces;

namespace CoworkingApp.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly CoworkingContext _context;
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(CoworkingContext context, IAnalyticsService analyticsService, ILogger<AdminController> logger)
        {
            _context = context;
            _analyticsService = analyticsService;
            _logger = logger;
        }

        // GET: api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var usersQuery = _context.Users
                .AsNoTracking()
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Status = u.Status,
                    LastLogin = u.LastLogin,
                    Role = u is CoworkingApp.API.Models.Administrator ? "Admin"
                         : u is CoworkingApp.API.Models.Manager ? "Manager"
                         : "Member"
                })
                .OrderBy(u => u.FullName);

            var users = await usersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(users);
        }

        // PUT: api/admin/users/{id}/status
        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required." });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            // Prevent changing administrator accounts' status via this endpoint
            if (user is CoworkingApp.API.Models.Administrator)
            {
                return BadRequest(new { message = "Cannot change status of an Administrator." });
            }

            // Validate supplied status
            if (!Enum.IsDefined(typeof(UserStatus), request.Status))
            {
                return BadRequest(new { message = "Invalid status value provided." });
            }

            user.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User status updated to {request.Status}" });
        }

        // GET: api/admin/analytics
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            try
            {
                var metrics = await _analyticsService.GetDashboardMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get analytics metrics.");
                return StatusCode(500, new { message = "Failed to retrieve analytics." });
            }
        }
    }
}
