using System.ComponentModel.DataAnnotations;

namespace EncoraOne.Grievance.API.DTOs
{
    public class CreateDepartmentDto
    {
        [Required]
        public string Name { get; set; }
    }

    public class DepartmentResponseDto
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
    }
}