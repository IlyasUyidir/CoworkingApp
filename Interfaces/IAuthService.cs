using System;
using System.Threading.Tasks;
using CoworkingApp.API.DTOs;

namespace CoworkingApp.API.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<UserProfileResponse> GetUserProfileAsync(Guid userId);
        Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    }
}