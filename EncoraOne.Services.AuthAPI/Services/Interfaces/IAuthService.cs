using System.Threading.Tasks;
using EncoraOne.Grievance.API.DTOs;
using EncoraOne.Grievance.API.Models;

namespace EncoraOne.Grievance.API.Services.Interfaces
{
    public interface IAuthService
    {
        // Register a new user (Admin/Manager/Employee)
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto);

        // Login an existing user
        Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto);

        // Helper to check if user exists
        Task<bool> UserExistsAsync(string email);
    }
}