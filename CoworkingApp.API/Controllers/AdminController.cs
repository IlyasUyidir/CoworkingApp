using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoworkingApp.API.Data;
using CoworkingApp.API.DTOs;
using CoworkingApp.API.Enums;
using CoworkingApp.API.Interfaces;
using CoworkingApp.API.Models;

namespace CoworkingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class AdminController : ControllerBase
    {
        private readonly CoworkingContext _context;
        private readonly IAnalyticsService _analyticsService;

        public AdminController(CoworkingContext context, IAnalyticsService analyticsService)
        {
            _context = context;
            _analyticsService = analyticsService;
        }

        // GET: api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Status = u.Status,
                    LastLogin = u.LastLogin,
                    Role = u is Administrator ? "Admin" : (u is Manager ? "Manager" : "Member")
                })
                .ToListAsync();

            return Ok(users);
        }

        // PUT: api/admin/users/{id}/status
        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UserStatus newStatus)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found.");

            if (user is Administrator) return BadRequest("Cannot change status of an Administrator.");

            user.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User status updated to {newStatus}" });
        }

        // GET: api/admin/analytics
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            var metrics = await _analyticsService.GetDashboardMetricsAsync();
            return Ok(metrics);
        }
    }
}