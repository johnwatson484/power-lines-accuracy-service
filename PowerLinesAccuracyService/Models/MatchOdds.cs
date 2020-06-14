using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerLinesAccuracyService.Models
{
    [Table("match_odds")]
    public class MatchOdds
    {
        [Column("matchOddsId")]
        public int MatchOddsId { get; set; }

        [Column("resultId")]
        public int ResultId { get; set; }

        [NotMapped]
        public int FixtureId { get; set; }

        [Column("home")]
        public decimal Home { get; set; }

        [Column("draw")]
        public decimal Draw { get; set; }

        [Column("away")]
        public decimal Away { get; set; }

        [Column("expectedHomeGoals")]
        public int HomeGoals { get; set; }

        [Column("expectedAwayGoals")]
        public int AwayGoals { get; set; }

        [Column("expectedGoals")]
        public decimal ExpectedGoals { get; set; }

        [Column("recommended")]
        public string Recommended { get; set; }

        [Column("lowerRecommended")]
        public string LowerRecommended { get; set; }

        [Column("calculated")]
        public DateTime Calculated { get; set; }

        public bool IsRecommended
        {
            get
            {
                return Recommended != "X";
            }
        }

        public bool IsLowerRecommended
        {
            get
            {
                return LowerRecommended != "X";
            }
        }

        public MatchOdds()
        {
            ResultId = FixtureId;
        }
    }
}
