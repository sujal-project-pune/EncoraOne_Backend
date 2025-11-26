using System.ComponentModel.DataAnnotations.Schema;

namespace EncoraOne.Grievance.API.Models
{
    [Table("Managers")]
    public class Manager : User
    {
        // Foreign Key
        public int DepartmentId { get; set; }

        // Navigation Property
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
    }
}