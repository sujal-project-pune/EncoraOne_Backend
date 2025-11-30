using System.Threading.Tasks;
using EncoraOne.Grievance.API.DTOs;

namespace EncoraOne.Grievance.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto);
        Task<bool> UserExistsAsync(string email);
        string HashPassword(string password);

        // NEW Methods
        Task<string> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetDto);
    }
}