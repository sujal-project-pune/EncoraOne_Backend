namespace EncoraOne.Grievance.API.DTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }

        // Employee Specific
        public string? JobTitle { get; set; }

        // Manager Specific
        public string? DepartmentName { get; set; }
    }
}