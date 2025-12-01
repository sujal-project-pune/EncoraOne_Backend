using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EncoraOne.Grievance.API.Models
{
    public class ComplaintComment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public int ComplaintId { get; set; }
        [ForeignKey("ComplaintId")]
        public Complaint Complaint { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}