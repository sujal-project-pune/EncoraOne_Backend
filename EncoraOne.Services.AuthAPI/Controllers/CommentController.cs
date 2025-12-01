using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EncoraOne.Grievance.API.Data;
using EncoraOne.Grievance.API.DTOs;
using EncoraOne.Grievance.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using EncoraOne.Grievance.API.Hubs;

namespace EncoraOne.Grievance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public CommentController(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet("complaint/{id}")]
        public async Task<IActionResult> GetComments(int id)
        {
            var comments = await _context.ComplaintComments
                .Where(c => c.ComplaintId == id)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentResponseDto
                {
                    CommentId = c.CommentId,
                    ComplaintId = c.ComplaintId,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt,
                    UserId = c.UserId,
                    UserName = c.User.FullName,
                    UserRole = c.User.Role.ToString()
                })
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPost]
        public async Task<IActionResult> PostComment([FromBody] CreateCommentDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var comment = new ComplaintComment
                {
                    ComplaintId = dto.ComplaintId,
                    UserId = userId,
                    Text = dto.Text,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ComplaintComments.Add(comment);
                await _context.SaveChangesAsync();

                var user = await _context.Users.FindAsync(userId);

                // REVERTED: Send only the message string, removed ComplaintId
                await _hubContext.Clients.All.SendAsync("ReceiveNotification",
                    $"New Comment on Complaint #{dto.ComplaintId} by {user.FullName}: {dto.Text.Substring(0, Math.Min(20, dto.Text.Length))}...");

                return Ok(new CommentResponseDto
                {
                    CommentId = comment.CommentId,
                    ComplaintId = comment.ComplaintId,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt,
                    UserId = userId,
                    UserName = user.FullName,
                    UserRole = user.Role.ToString()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to post comment." });
            }
        }
    }
}