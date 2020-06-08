using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerLinesAccuracyService.Models
{
    [Table("accuracy")]
    public class Accuracy
    {
        [Column("accuracyId")]
        public int AccuracyId { get; set; }

        [Column("division")]
        public string Division { get; set; }

        [Column("matches")]
        public int Matches { get; set; }

        [Column("recommended")]
        public int Recommended { get; set; }

        [Column("recommendedAccuracy")]
        public decimal RecommendedAccuracy { get; set; }

        [Column("lowerRecommended")]
        public int LowerRecommended { get; set; }

        [Column("lowerRecommendedAccuracy")]
        public decimal LowerRecommendedAccuracy { get; set; }

        [Column("calculated")]
        public DateTime Calculated { get; set; }

        public Accuracy()
        {
            Calculated = DateTime.UtcNow;
        }
    }
}
