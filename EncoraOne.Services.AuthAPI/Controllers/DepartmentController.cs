using System;
using System.Linq;
using System.Threading.Tasks;
using EncoraOne.Grievance.API.Data; // Required for AppDbContext
using EncoraOne.Grievance.API.DTOs;
using EncoraOne.Grievance.API.Models;
using EncoraOne.Grievance.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Required for EF Core Async methods

namespace EncoraOne.Grievance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context; // Inject Context for efficient projection queries

        public DepartmentController(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        // GET: api/Department
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            // FIX: Use .Select() to count Managers and Complaints in the database query
            var dtos = await _context.Departments
                .AsNoTracking()
                .Select(d => new DepartmentResponseDto
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name,
                    // These counts will now be populated correctly
                    ManagerCount = d.Managers.Count(),
                    ComplaintCount = d.Complaints.Count()
                })
                .ToListAsync();

            return Ok(dtos);
        }

        // GET: api/Department/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var dto = await _context.Departments
                .AsNoTracking()
                .Where(d => d.DepartmentId == id)
                .Select(d => new DepartmentResponseDto
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name,
                    ManagerCount = d.Managers.Count(),
                    ComplaintCount = d.Complaints.Count()
                })
                .FirstOrDefaultAsync();

            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // POST: api/Department
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto createDto)
        {
            var dept = new Department { Name = createDto.Name };

            await _unitOfWork.Departments.AddAsync(dept);
            await _unitOfWork.CompleteAsync();

            return Ok(new DepartmentResponseDto
            {
                DepartmentId = dept.DepartmentId,
                Name = dept.Name,
                ManagerCount = 0,
                ComplaintCount = 0
            });
        }

        // PUT: api/Department
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDepartment([FromBody] UpdateDepartmentDto updateDto)
        {
            var dept = await _unitOfWork.Departments.GetByIdAsync(updateDto.DepartmentId);
            if (dept == null) return NotFound(new { message = "Department not found" });

            dept.Name = updateDto.Name;

            _unitOfWork.Departments.Update(dept);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        // DELETE: api/Department/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var dept = await _unitOfWork.Departments.GetByIdAsync(id);
            if (dept == null) return NotFound(new { message = "Department not found" });

            // FIX: Use AppDbContext for efficient dependency check
            var hasManagers = await _context.Managers.AnyAsync(m => m.DepartmentId == id);
            var hasComplaints = await _context.Complaints.AnyAsync(c => c.DepartmentId == id);

            if (hasManagers || hasComplaints)
            {
                return BadRequest(new
                {
                    message = $"Cannot delete '{dept.Name}'. It has active Managers or Complaints linked to it. Please reassign or delete them first."
                });
            }

            _unitOfWork.Departments.Remove(dept);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
    }
}