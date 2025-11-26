using System.Collections.Generic;
using System.Threading.Tasks;
using EncoraOne.Grievance.API.DTOs;

namespace EncoraOne.Grievance.API.Services.Interfaces
{
    public interface IComplaintService
    {
        // Create a new complaint
        Task<ComplaintResponseDto> CreateComplaintAsync(CreateComplaintDto createDto, int employeeId);

        // Get complaints for a specific employee
        Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByEmployeeIdAsync(int employeeId);

        // Get complaints for a specific department (For Managers)
        Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByDepartmentIdAsync(int departmentId);

        // NEW: Get ALL complaints (For Super Admin)
        Task<IEnumerable<ComplaintResponseDto>> GetAllComplaintsAsync();

        // Update status (Manager/Admin)
        Task<bool> UpdateComplaintStatusAsync(UpdateComplaintStatusDto updateDto, int managerId);
    }
}