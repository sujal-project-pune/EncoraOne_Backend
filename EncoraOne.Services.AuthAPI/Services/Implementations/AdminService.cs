using EncoraOne.Grievance.API.DTOs;
using EncoraOne.Grievance.API.Models;
using EncoraOne.Grievance.API.Data;
using EncoraOne.Grievance.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grievance.API.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public AdminService(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<object?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;
            return await MapUserDetails(user);
        }

        // NEW: Get All Users
        public async Task<IEnumerable<object>> GetAllUsersAsync()
        {
            var users = await _context.Users.AsNoTracking().ToListAsync();
            var result = new List<object>();

            foreach (var user in users)
            {
                result.Add(await MapUserDetails(user));
            }
            return result;
        }

        private async Task<object> MapUserDetails(User user)
        {
            var manager = await _context.Managers.AsNoTracking().FirstOrDefaultAsync(m => m.Id == user.Id);
            var employee = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == user.Id);

            return new
            {
                user.Id,
                user.FullName,
                user.Email,
                Role = (int)user.Role,
                user.IsActive,
                JobTitle = manager?.JobTitle ?? employee?.JobTitle,
                DepartmentId = manager?.DepartmentId
            };
        }

        public async Task UpdateUserAsync(UpdateUserDto updateDto)
        {
            var userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.Id == updateDto.Id);
            if (userToUpdate == null) throw new ArgumentException("User not found");

            if (!string.IsNullOrWhiteSpace(updateDto.FullName)) userToUpdate.FullName = updateDto.FullName;
            if (!string.IsNullOrWhiteSpace(updateDto.Email)) userToUpdate.Email = updateDto.Email;
            if (updateDto.IsActive.HasValue) userToUpdate.IsActive = updateDto.IsActive.Value;

            if (!string.IsNullOrWhiteSpace(updateDto.Password))
            {
                userToUpdate.PasswordHash = _authService.HashPassword(updateDto.Password);
            }

            if (updateDto.Role.HasValue) userToUpdate.Role = (UserRole)updateDto.Role.Value;

            if (userToUpdate is Manager manager)
            {
                if (updateDto.DepartmentId.HasValue) manager.DepartmentId = updateDto.DepartmentId.Value;
                if (!string.IsNullOrWhiteSpace(updateDto.JobTitle)) manager.JobTitle = updateDto.JobTitle;
            }
            else if (userToUpdate is Employee employee)
            {
                if (!string.IsNullOrWhiteSpace(updateDto.JobTitle)) employee.JobTitle = updateDto.JobTitle;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}