using System;
using System.Collections.Generic;
using System.Linq;
using PowerLinesAccuracyService.Data;
using PowerLinesAccuracyService.Models;
using PowerLinesAccuracyService.Extensions;
using Microsoft.EntityFrameworkCore;

namespace PowerLinesAccuracyService.Analysis
{
    public class AnalysisService : IAnalysisService
    {
        ApplicationDbContext dbContext;
        Threshold threshold;
        const int yearsToAnalyse = 6;
        const int maxGoalsPerGame = 5;
        DateTime startDate;
        List<Result> matches;
        decimal homeExpectedGoals;
        decimal awayExpectedGoals;
        GoalDistribution goalDistribution;
        Poisson poisson;
        OddsCalculator oddsService;

        public AnalysisService(ApplicationDbContext dbContext, Threshold threshold)
        {
            this.dbContext = dbContext;
            this.threshold = threshold;
            goalDistribution = new GoalDistribution();
            poisson = new Poisson();
        }

        public MatchOdds GetMatchOdds(Fixture fixture, List<Result> historicResults = null)
        {
            SetStartDate(fixture.Date);
            SetAnalysisMatches(fixture.Division, historicResults);

            CalculateExpectedGoals(fixture);
            CalculateGoalDistribution();

            oddsService = new OddsCalculator(fixture.FixtureId, goalDistribution, threshold);
            return oddsService.GetMatchOdds();
        }

        private void SetStartDate(DateTime fixtureDate)
        {
            startDate = fixtureDate.AddYears(-6).Date;
        }

        private void SetAnalysisMatches(string division, List<Result> historicResults = null)
        {
            if (historicResults == null)
            {
                matches = dbContext.Results.AsNoTracking().Where(x => x.Division == division && x.Date >= startDate).ToList();
            }
            else
            {
                matches = historicResults.Where(x => x.Division == division && x.Date >= startDate).ToList();
            }
        }

        private void CalculateExpectedGoals(Fixture fixture)
        {
            var totalAverageHomeGoals = GetTotalAverageHomeGoals();
            var totalAverageAwayGoals = GetTotalAverageAwayGoals();
            var totalAverageHomeConceded = totalAverageAwayGoals;
            var totalAverageAwayConceded = totalAverageHomeGoals;

            var averageHomeGoals = GetAverageHomeGoals(fixture.HomeTeam);
            var homeAttackStrength = GetAttackStrength(averageHomeGoals, totalAverageHomeGoals);
            var averageAwayConceded = GetAverageAwayConceded(fixture.AwayTeam);
            var awayDefenceStrength = GetDefenceStrength(averageAwayConceded, totalAverageAwayConceded);
            homeExpectedGoals = GetExpectedGoals(homeAttackStrength, awayDefenceStrength, totalAverageHomeGoals);

            var averageAwayGoals = GetAverageAwayGoals(fixture.AwayTeam);
            var awayAttackStrength = GetAttackStrength(averageAwayGoals, totalAverageAwayGoals);
            var averageHomeConceded = GetAverageHomeConceded(fixture.HomeTeam);
            var homeDefenceStrength = GetDefenceStrength(averageHomeConceded, totalAverageHomeConceded);
            awayExpectedGoals = GetExpectedGoals(awayAttackStrength, homeDefenceStrength, totalAverageAwayGoals);
        }

        private decimal GetTotalAverageHomeGoals()
        {
            return DecimalExtensions.SafeDivide(matches.Sum(x => x.FullTimeHomeGoals), matches.Count);
        }

        private decimal GetTotalAverageAwayGoals()
        {
            return DecimalExtensions.SafeDivide(matches.Sum(x => x.FullTimeAwayGoals), matches.Count);
        }

        private decimal GetAverageHomeGoals(string homeTeam)
        {
            var homeMatches = matches.Where(x => x.HomeTeam == homeTeam).ToList();
            return DecimalExtensions.SafeDivide(homeMatches.Sum(x => x.FullTimeHomeGoals), homeMatches.Count);
        }

        private decimal GetAverageAwayGoals(string awayTeam)
        {
            var awayMatches = matches.Where(x => x.AwayTeam == awayTeam).ToList();
            return DecimalExtensions.SafeDivide(awayMatches.Sum(x => x.FullTimeAwayGoals), awayMatches.Count);
        }

        private decimal GetAverageHomeConceded(string homeTeam)
        {
            var homeMatches = matches.Where(x => x.HomeTeam == homeTeam).ToList();
            return DecimalExtensions.SafeDivide(homeMatches.Sum(x => x.FullTimeAwayGoals), homeMatches.Count);
        }

        private decimal GetAverageAwayConceded(string awayTeam)
        {
            var awayMatches = matches.Where(x => x.AwayTeam == awayTeam).ToList();
            return DecimalExtensions.SafeDivide(awayMatches.Sum(x => x.FullTimeHomeGoals), awayMatches.Count);
        }

        private decimal GetAttackStrength(decimal averageGoals, decimal totalAverageGoals)
        {
            return DecimalExtensions.SafeDivide(averageGoals, totalAverageGoals);
        }

        private decimal GetDefenceStrength(decimal averageConceded, decimal totalAverageConceded)
        {
            return DecimalExtensions.SafeDivide(averageConceded, totalAverageConceded);
        }

        private decimal GetExpectedGoals(decimal teamAttackStrength, decimal oppositionDefenceStrength, decimal totalAverageGoals)
        {
            return teamAttackStrength * oppositionDefenceStrength * totalAverageGoals;
        }

        private void CalculateGoalDistribution()
        {
            for (int goals = 0; goals <= maxGoalsPerGame; goals++)
            {
                goalDistribution.HomeGoalProbabilities.Add(GetGoalProbability(goals, homeExpectedGoals));
                goalDistribution.AwayGoalProbabilities.Add(GetGoalProbability(goals, awayExpectedGoals));
            }

            goalDistribution.CalculateDistribution();
        }

        private GoalProbability GetGoalProbability(int goals, decimal expectedGoals)
        {
            return new GoalProbability(goals, (decimal)poisson.GetProbability(goals, (double)expectedGoals));
        }
    }
}
