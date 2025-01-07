using Hotel_Backend_API.Data;
using Hotel_Backend_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Services
{
    public class SaveRecommendedService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly RoomService roomService;

        public SaveRecommendedService(ApplicationDbContext dbContext, RoomService roomService)
        {
            this.dbContext = dbContext;
            this.roomService = roomService;
        }

        public async Task<List<Room>> GetSaveRoomsAsync(string userId)
        {
            return await dbContext.SavedRooms
                .Where(save => save.UserId == userId)
                .Select(save => save.Room)
                .Distinct()
                .ToListAsync();
        }
    }
}
