using System.Collections.Generic; // Required for IEnumerable
using System.Threading.Tasks;
using EncoraOne.Grievance.API.DTOs;

namespace EncoraOne.Grievance.API.Services.Interfaces
{
    public interface IAdminService
    {
        Task<object?> GetUserByEmailAsync(string email);

        // NEW: Get all users for the list view
        Task<IEnumerable<object>> GetAllUsersAsync();

        Task UpdateUserAsync(UpdateUserDto updateDto);
        Task<bool> DeleteUserAsync(int id);
    }
}