using Hotel_Backend_API.Data;
using Hotel_Backend_API.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<Hotel>> GetHotelRecommendationsByTagsAsync(string tagsSearchString, int numOfRecommendations = 5)
        {
            var allHotels = await _dbContext.Hotels.ToListAsync();

            var searchTags = tagsSearchString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var filteredHotels = allHotels
                .Where(hotel => hotel.Tags != null &&
                                searchTags.Any(tag => hotel.Tags.Contains(tag, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (!filteredHotels.Any())
            {
                return new List<Hotel>();
            }

            return filteredHotels.Take(numOfRecommendations).ToList();
        }
    }
}
