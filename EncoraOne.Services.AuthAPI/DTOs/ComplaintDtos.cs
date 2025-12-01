using System;
using System.ComponentModel.DataAnnotations;
using EncoraOne.Grievance.API.Models;

namespace EncoraOne.Grievance.API.DTOs
{
    public class CreateComplaintDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        public string? AttachmentUrl { get; set; } // Kept as simple string
    }

    public class UpdateComplaintStatusDto
    {
        [Required]
        public int ComplaintId { get; set; }

        [Required]
        public ComplaintStatus Status { get; set; }

        public string? Remarks { get; set; }
    }

    public class ComplaintResponseDto
    {
        public int ComplaintId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string AttachmentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ManagerRemarks { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
    }
}