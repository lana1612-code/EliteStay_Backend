using Hotel_Backend_API.DTO.Notification;
using Hotel_Backend_API.Models;
using Hotel_Backend_API.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hotel_Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationsController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }
       
        [HttpGet("user")]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            string userId = userIdClaim;
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);

            if (!notifications.Any())
            {
                return NotFound("No Notification found");
            }

            var notificationDTOs = new List<NotificationDTO>();

            foreach (var userNotification in notifications)
            {
                var notiDTO = new NotificationDTO
                {
                    Message = userNotification.Notification.Message,
                    sentAt = userNotification.SentAt.ToString("yyyy-MM-dd"),
                    IsRead = userNotification.IsRead
                };

                notificationDTOs.Add(notiDTO);
            }

            return Ok(notificationDTOs);
        }


        [HttpPut("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok("Notification marked as read.");
        }
    
    }
}
