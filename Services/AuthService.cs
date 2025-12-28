using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using CoworkingApp.API.Data;
using CoworkingApp.API.DTOs;
using CoworkingApp.API.Enums;
using CoworkingApp.API.Interfaces;
using CoworkingApp.API.Models;
using CoworkingApp.API.Exceptions;

namespace CoworkingApp.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly CoworkingContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(CoworkingContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new BadRequestException("Email already exists.");
            }

            var member = new Member
            {
                UserId = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Status = UserStatus.ACTIVE,
                MembershipType = MembershipType.BASIC,
                MembershipStartDate = DateTime.UtcNow,
                TotalBookings = 0
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var refreshToken = GenerateRefreshToken();
            member.RefreshToken = refreshToken;
            member.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(14);
            await _context.SaveChangesAsync();

            return GenerateAuthResponse(member, "Member", refreshToken);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                throw new BadRequestException("Invalid email or password.");
            }

            var role = "Member";
            if (user is Administrator) role = "Admin";
            else if (user is Manager) role = "Manager";

            // Generate and save refresh token
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(14);
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return GenerateAuthResponse(user, role, refreshToken);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new BadRequestException("Invalid or expired refresh token.");
            }

            var role = "Member";
            if (user is Administrator) role = "Admin";
            else if (user is Manager) role = "Manager";

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(14);
            await _context.SaveChangesAsync();

            return GenerateAuthResponse(user, role, newRefreshToken);
        }

        public async Task<UserProfileResponse> GetUserProfileAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            return new UserProfileResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new NotFoundException("User not found.");

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.FirstName)) user.FirstName = request.FirstName;
            if (!string.IsNullOrEmpty(request.LastName)) user.LastName = request.LastName;
            if (!string.IsNullOrEmpty(request.PhoneNumber)) user.PhoneNumber = request.PhoneNumber;

            // Handle Password Change if requested
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                if (string.IsNullOrEmpty(request.CurrentPassword))
                    throw new BadRequestException("Current password is required to set a new password.");

                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
                    throw new BadRequestException("Current password is incorrect.");

                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private AuthResponse GenerateAuthResponse(User user, string role, string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(15), // Shorter lifespan for JWT
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken,
                Role = role,
                FullName = $"{user.FirstName} {user.LastName}"
            };
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}