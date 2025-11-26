using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EncoraOne.Grievance.API.Models
{
    // Abstract class so we cannot create just a "User" - it must be an Employee or Manager
    public abstract class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [JsonIgnore] // Security: Never send password hash to the frontend
        public string PasswordHash { get; set; }

        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}