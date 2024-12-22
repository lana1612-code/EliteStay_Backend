namespace Hotel_Backend_API.Services
{
    public class SimilarityHelper
    {
        public static double CosineSimilarity(Dictionary<string, double> vectorA, Dictionary<string, double> vectorB)
        {
            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            foreach (var key in vectorA.Keys)
            {
                dotProduct += vectorA[key] * vectorB[key];
                magnitudeA += Math.Pow(vectorA[key], 2);
                magnitudeB += Math.Pow(vectorB[key], 2);
            }

            magnitudeA = Math.Sqrt(magnitudeA);
            magnitudeB = Math.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0)
            {
                return 0;
            }

            return dotProduct / (magnitudeA * magnitudeB);
        }
    }
}
