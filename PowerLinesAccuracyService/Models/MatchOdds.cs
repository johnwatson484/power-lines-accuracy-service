using System;
using PowerLinesAccuracyService.Models;

public class MatchOdds
{
    public int FixtureId { get; set; }
    public decimal Home { get; set; }
    public decimal Draw { get; set; }
    public decimal Away { get; set; }
    public int HomeGoals { get; set; }
    public int AwayGoals { get; set; }
    public decimal ExpectedGoals { get; set; }
    public string Recommended { get; set; }
    public string LowerRecommended { get; set; }
    public DateTime Calculated { get; set; }

    public MatchOdds(int fixtureId)
    {
        FixtureId = fixtureId;
        Recommended = "X";
        LowerRecommended = "X";
        Calculated = DateTime.UtcNow;
    }
}
