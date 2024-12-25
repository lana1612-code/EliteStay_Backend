using Hotel_Backend_API.Data;
using Hotel_Backend_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Services
{
    public class LikeRecommendedService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly RoomService roomService;

        public LikeRecommendedService(ApplicationDbContext dbContext, RoomService roomService)
        {
            this.dbContext = dbContext;
            this.roomService = roomService;
        }

        public async Task<List<Room>> GetLikedRoomsAsync(string userId)
        {
            return await dbContext.Likes
                .Where(like => like.UserId == userId)
                .Select(like => like.Room)
                .Distinct()
                .ToListAsync();
        }

    }
}
