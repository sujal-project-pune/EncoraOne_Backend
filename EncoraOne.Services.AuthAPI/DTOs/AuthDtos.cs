using System.ComponentModel.DataAnnotations;
using EncoraOne.Grievance.API.Models;

namespace EncoraOne.Grievance.API.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class RegisterUserDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

        // Optional: Only required if creating a Manager
        public int? DepartmentId { get; set; }

        // Optional: Only required if creating an Employee
        public string? JobTitle { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public int UserId { get; set; }
        public int? DepartmentId { get; set; } // Added this field
    }
}