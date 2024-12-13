using Hotel_Backend_API.Data;
using Hotel_Backend_API.Models;

namespace Hotel_Backend_API.Services
{
    public class HotelService
    {
        private readonly ApplicationDbContext _dbContext;

        public HotelService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Hotel> GetHotelByIdAsync(int id)
        {
            return await _dbContext.Hotels.FindAsync(id);
        }
    }
}
