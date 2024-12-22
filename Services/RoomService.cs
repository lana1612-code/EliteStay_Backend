using Hotel_Backend_API.Data;
using Hotel_Backend_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Services
{
    public class RoomService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Dictionary<string, double> _idfScores;
        private readonly int _totalDocuments;

        public RoomService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

            var roomDescriptions = _dbContext.RoomTypes.Select(rt => rt.Description).ToList();
            _idfScores = CalculateIdf(roomDescriptions);
            _totalDocuments = roomDescriptions.Count;
        }


        public async Task<List<RoomType>> GetRoomTypeRecommendationsByDescriptionAsync(string descriptionSearchString, int numOfRecommendations = 100)
        {
            var allRoomTypes = await _dbContext.RoomTypes.ToListAsync();

            var filteredRoomTypes = allRoomTypes
                .Where(rt => rt.Description.Contains(descriptionSearchString, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!filteredRoomTypes.Any())
            {
                return new List<RoomType>();
            }

            var similarityScores = filteredRoomTypes.Select(roomType =>
            {
                var roomTypeVector = CreateFeatureVector(roomType);
                var searchVector = CreateFeatureVector(new RoomType { Description = descriptionSearchString });
                return new
                {
                    RoomType = roomType,
                    SimilarityScore = SimilarityHelper.CosineSimilarity(searchVector, roomTypeVector)
                };
            })

         .OrderByDescending(x => x.SimilarityScore)
            .Take(numOfRecommendations)
            .Select(x => x.RoomType)
            .ToList();

            return similarityScores;
        }

        private Dictionary<string, double> CreateFeatureVector(RoomType roomType)
        {
            var descriptionVector = Transform(roomType.Description);

            return new Dictionary<string, double>
            {
                {"Name", CalculateLocationScore(roomType.Name)},
                {"PricePerNight", (double)roomType.PricePerNight},
                {"Capacity", roomType.Capacity},
                {"Description", descriptionVector.Sum()}

            };
        }

        private double CalculateLocationScore(string location)
        {
            return 1.0;
        }

        private double[] Transform(string text)
        {
            var tfScores = CalculateTf(text);
            return tfScores.Select(term => tfScores[term.Key] * _idfScores.GetValueOrDefault(term.Key, 0)).ToArray();
        }


        private Dictionary<string, double> CalculateTf(string text)
        {
            var terms = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var termCount = terms.Length;
            var termFrequency = terms.GroupBy(t => t).ToDictionary(g => g.Key, g => (double)g.Count() / termCount);
            return termFrequency;
        }

        private Dictionary<string, double> CalculateIdf(IEnumerable<string> documents)
        {
            var termDocumentCount = new Dictionary<string, int>();
            var totalDocuments = documents.Count();

            foreach (var document in documents)
            {
                var uniqueTerms = new HashSet<string>(document.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                foreach (var term in uniqueTerms)
                {
                    if (termDocumentCount.ContainsKey(term))
                    {
                        termDocumentCount[term]++;
                    }
                    else
                    {
                        termDocumentCount[term] = 1;
                    }
                }
            }
            return termDocumentCount.ToDictionary(
                pair => pair.Key,
                pair => Math.Log((double)totalDocuments / (1 + pair.Value))
            );
        }
 
    }
}
