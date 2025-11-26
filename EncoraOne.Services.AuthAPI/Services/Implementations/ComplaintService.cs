using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EncoraOne.Grievance.API.DTOs;
using EncoraOne.Grievance.API.Models;
using EncoraOne.Grievance.API.Repositories.Interfaces;
using EncoraOne.Grievance.API.Services.Interfaces;

namespace EncoraOne.Grievance.API.Services.Implementations
{
    public class ComplaintService : IComplaintService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ComplaintService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ComplaintResponseDto> CreateComplaintAsync(CreateComplaintDto createDto, int employeeId)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(createDto.DepartmentId);
            if (department == null) throw new Exception("Department not found.");

            var complaint = new Complaint
            {
                Title = createDto.Title,
                Description = createDto.Description,
                AttachmentUrl = createDto.AttachmentUrl,
                DepartmentId = createDto.DepartmentId,
                EmployeeId = employeeId,
                Status = ComplaintStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Complaints.AddAsync(complaint);
            await _unitOfWork.CompleteAsync();

            var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);

            return MapToResponse(complaint, department.Name, employee.FullName);
        }

        public async Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByEmployeeIdAsync(int employeeId)
        {
            var complaints = await _unitOfWork.Complaints.FindAsync(c => c.EmployeeId == employeeId);
            return await MapList(complaints);
        }

        public async Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByDepartmentIdAsync(int departmentId)
        {
            var complaints = await _unitOfWork.Complaints.FindAsync(c => c.DepartmentId == departmentId);
            return await MapList(complaints);
        }

        // NEW: Implementation for Admin to see everything
        public async Task<IEnumerable<ComplaintResponseDto>> GetAllComplaintsAsync()
        {
            var complaints = await _unitOfWork.Complaints.GetAllAsync();
            return await MapList(complaints);
        }

        public async Task<bool> UpdateComplaintStatusAsync(UpdateComplaintStatusDto updateDto, int managerId)
        {
            var complaint = await _unitOfWork.Complaints.GetByIdAsync(updateDto.ComplaintId);
            if (complaint == null) throw new Exception("Complaint not found");

            var manager = await _unitOfWork.Managers.GetByIdAsync(managerId);
            if (manager == null) throw new Exception("Manager not found");

            // Allow Admin (Role 1) OR Matching Department Manager
            // Note: Assuming Admin Role Enum value is 1
            if (manager.Role != UserRole.Admin && manager.DepartmentId != complaint.DepartmentId)
            {
                throw new Exception("You are not authorized to manage complaints for this department.");
            }

            complaint.Status = updateDto.Status;
            complaint.ManagerRemarks = updateDto.Remarks;

            if (updateDto.Status == ComplaintStatus.Resolved)
                complaint.ResolvedAt = DateTime.UtcNow;

            _unitOfWork.Complaints.Update(complaint);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        private async Task<List<ComplaintResponseDto>> MapList(IEnumerable<Complaint> complaints)
        {
            var responseList = new List<ComplaintResponseDto>();
            foreach (var c in complaints)
            {
                var dept = await _unitOfWork.Departments.GetByIdAsync(c.DepartmentId);
                var emp = await _unitOfWork.Employees.GetByIdAsync(c.EmployeeId);
                responseList.Add(MapToResponse(c, dept?.Name, emp?.FullName));
            }
            return responseList;
        }

        private ComplaintResponseDto MapToResponse(Complaint c, string deptName, string empName)
        {
            return new ComplaintResponseDto
            {
                ComplaintId = c.ComplaintId,
                Title = c.Title,
                Description = c.Description,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt,
                ResolvedAt = c.ResolvedAt,
                ManagerRemarks = c.ManagerRemarks,
                DepartmentName = deptName ?? "Unknown",
                EmployeeName = empName ?? "Unknown"
            };
        }
    }
}