using System;
using CoworkingApp.API.Enums;

namespace CoworkingApp.API.DTOs
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastLogin { get; set; }
        public string Role { get; set; }
    }
}
