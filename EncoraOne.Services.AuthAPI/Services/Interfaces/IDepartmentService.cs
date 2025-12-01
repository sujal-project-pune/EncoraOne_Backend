using System.Collections.Generic;
using System.Threading.Tasks;
using EncoraOne.Grievance.API.DTOs;

namespace EncoraOne.Grievance.API.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentResponseDto>> GetAllDepartmentsAsync();
        Task<DepartmentResponseDto> GetDepartmentByIdAsync(int id);
        Task<DepartmentResponseDto> CreateDepartmentAsync(CreateDepartmentDto createDto);
        Task UpdateDepartmentAsync(UpdateDepartmentDto updateDto);
        Task<bool> DeleteDepartmentAsync(int id);
    }
}