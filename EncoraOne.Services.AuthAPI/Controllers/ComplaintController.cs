using System;
using System.Security.Claims;
using System.Threading.Tasks;
using EncoraOne.Grievance.API.DTOs;
using EncoraOne.Grievance.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EncoraOne.Grievance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _complaintService;

        public ComplaintController(IComplaintService complaintService)
        {
            _complaintService = complaintService;
        }

        [HttpPost]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> CreateComplaint([FromBody] CreateComplaintDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _complaintService.CreateComplaintAsync(createDto, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-complaints")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetMyComplaints()
        {
            var userId = GetCurrentUserId();
            var result = await _complaintService.GetComplaintsByEmployeeIdAsync(userId);
            return Ok(result);
        }

        [HttpGet("department/{deptId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetDepartmentComplaints(int deptId)
        {
            var result = await _complaintService.GetComplaintsByDepartmentIdAsync(deptId);
            return Ok(result);
        }

        // NEW: Admin only endpoint to see everything
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllComplaints()
        {
            var result = await _complaintService.GetAllComplaintsAsync();
            return Ok(result);
        }

        [HttpPut("update-status")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateComplaintStatusDto updateDto)
        {
            try
            {
                var managerId = GetCurrentUserId();
                var result = await _complaintService.UpdateComplaintStatusAsync(updateDto, managerId);
                return Ok(new { message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (userClaim != null) return int.Parse(userClaim.Value);
            }
            throw new Exception("User ID not found in token");
        }
    }
}