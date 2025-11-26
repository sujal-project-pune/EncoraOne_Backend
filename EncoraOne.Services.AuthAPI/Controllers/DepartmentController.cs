using System.Linq;
using System.Threading.Tasks;
using EncoraOne.Grievance.API.DTOs;
using EncoraOne.Grievance.API.Models;
using EncoraOne.Grievance.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EncoraOne.Grievance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/Department
        // Publicly accessible or Authorized Users only
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _unitOfWork.Departments.GetAllAsync();

            var dtos = departments.Select(d => new DepartmentResponseDto
            {
                DepartmentId = d.DepartmentId,
                Name = d.Name
            });

            return Ok(dtos);
        }

        // POST: api/Department
        // Only Admins can add departments
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
                Name = dept.Name
            });
        }
    }
}