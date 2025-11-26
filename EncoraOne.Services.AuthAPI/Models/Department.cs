using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EncoraOne.Grievance.API.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // e.g., "IT", "HR"

        // Navigation Properties
        public ICollection<Manager> Managers { get; set; }
        public ICollection<Complaint> Complaints { get; set; }
    }
}