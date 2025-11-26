using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EncoraOne.Grievance.API.Models
{
    [Table("Employees")]
    public class Employee : User
    {
        [MaxLength(50)]
        public string JobTitle { get; set; }

        // Navigation Property: One Employee has many complaints
        public ICollection<Complaint> Complaints { get; set; }
    }
}