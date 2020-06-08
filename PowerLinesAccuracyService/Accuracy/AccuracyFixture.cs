using PowerLinesAccuracyService.Models;

namespace PowerLinesAccuracyService.Accuracy
{
    public class AccuracyFixture
    {
        public Fixture Fixture { get; set; }
        MatchOdds matchOdds;
        string actual;

        public AccuracyFixture(Fixture fixture, MatchOdds matchOdds, string actual)
        {
            Fixture = fixture;
            this.matchOdds = matchOdds;
            this.actual = actual;
        }

        public bool IsRecommended
        {
            get
            {
                return matchOdds.Recommended != "X";
            }
        }

        public bool IsRecommendedCorrect
        {
            get
            {
                return matchOdds.Recommended == actual;
            }
        }

        public bool IsLowerRecommended
        {
            get
            {
                return matchOdds.LowerRecommended != "X";
            }
        }

        public bool IsLowerRecommendedCorrect
        {
            get
            {
                return matchOdds.LowerRecommended == actual;
            }
        }
    }
}
