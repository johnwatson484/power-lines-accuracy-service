using System;

namespace PowerLinesAccuracyService.Accuracy
{
    public class AccuracyResponse
    {
        public int AccuracyId { get; set; }
        public string Country { get; set; }
        public int CountryRank { get; set; }
        public string Division { get; set; }
        public int Tier { get; set; }
        public int Matches { get; set; }
        public int Recommended { get; set; }
        public decimal RecommendedAccuracy { get; set; }
        public int LowerRecommended { get; set; }
        public decimal LowerRecommendedAccuracy { get; set; }
        public DateTime Calculated { get; set; }

        public AccuracyResponse(Models.Accuracy accuracy)
        {
            var division = new Division(accuracy.Division);

            AccuracyId = accuracy.AccuracyId;
            Country = division.Country;
            CountryRank = division.CountryRank;
            Division = division.Name;
            Tier = division.Tier;
            Matches = accuracy.Matches;
            Recommended = accuracy.Recommended;
            RecommendedAccuracy = accuracy.RecommendedAccuracy;
            LowerRecommended = accuracy.LowerRecommended;
            LowerRecommendedAccuracy = accuracy.LowerRecommendedAccuracy;
            Calculated = accuracy.Calculated;
        }
    }
}
