using System.ComponentModel.DataAnnotations;

namespace EncoraOne.Grievance.API.DTOs
{
    public class CreateDepartmentDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }

    public class UpdateDepartmentDto
    {
        [Required]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }

    public class DepartmentResponseDto
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public int ManagerCount { get; set; } // Useful for UI
        public int ComplaintCount { get; set; } // Useful for UI
    }
}