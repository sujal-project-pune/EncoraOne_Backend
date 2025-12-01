using System;
using System.ComponentModel.DataAnnotations;

namespace EncoraOne.Grievance.API.DTOs
{
    public class CreateCommentDto
    {
        [Required]
        public int ComplaintId { get; set; }
        [Required]
        public string Text { get; set; }
    }

    public class CommentResponseDto
    {
        public int CommentId { get; set; }
        public int ComplaintId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }

        // User Details for UI Display
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; } // To style bubbles (Manager vs Employee)
    }
}