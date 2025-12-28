using System;
using System.ComponentModel.DataAnnotations;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.Models
{
    public abstract class User
    {
        [Key]
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public UserStatus Status { get; set; }
        
        // Refresh Token Fields
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}