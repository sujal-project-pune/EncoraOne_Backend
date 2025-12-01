using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EncoraOne.Grievance.API.Data;
using EncoraOne.Grievance.API.DTOs;
using EncoraOne.Grievance.API.Models;
using EncoraOne.Grievance.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EncoraOne.Grievance.API.Services.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private readonly AppDbContext _context;

        public DepartmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DepartmentResponseDto>> GetAllDepartmentsAsync()
        {
            return await _context.Departments
                .AsNoTracking()
                .Select(d => new DepartmentResponseDto
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name,
                    ManagerCount = d.Managers.Count(),
                    ComplaintCount = d.Complaints.Count()
                })
                .ToListAsync();
        }

        public async Task<DepartmentResponseDto> GetDepartmentByIdAsync(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return null;

            return new DepartmentResponseDto
            {
                DepartmentId = dept.DepartmentId,
                Name = dept.Name
            };
        }

        public async Task<DepartmentResponseDto> CreateDepartmentAsync(CreateDepartmentDto createDto)
        {
            var dept = new Department { Name = createDto.Name };
            await _context.Departments.AddAsync(dept);
            await _context.SaveChangesAsync();

            return new DepartmentResponseDto
            {
                DepartmentId = dept.DepartmentId,
                Name = dept.Name
            };
        }

        public async Task UpdateDepartmentAsync(UpdateDepartmentDto updateDto)
        {
            var dept = await _context.Departments.FindAsync(updateDto.DepartmentId);
            if (dept == null) throw new Exception("Department not found");

            dept.Name = updateDto.Name;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            var dept = await _context.Departments
                .Include(d => d.Managers)
                .Include(d => d.Complaints)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (dept == null) return false;

            // Integrity Check: Don't delete if it has dependencies
            if (dept.Managers.Any() || dept.Complaints.Any())
            {
                throw new Exception($"Cannot delete '{dept.Name}'. It has {dept.Managers.Count} Managers and {dept.Complaints.Count} Complaints linked to it.");
            }

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}