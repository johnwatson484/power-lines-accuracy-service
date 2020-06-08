
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PowerLinesAccuracyService.Analysis;
using PowerLinesAccuracyService.Data;
using PowerLinesAccuracyService.Models;
using PowerLinesAccuracyService.Extensions;

namespace PowerLinesAccuracyService.Accuracy
{
    public class AccuracyCalculator : IAccuracyCalculator
    {
        ApplicationDbContext dbContext;
        IAnalysisService analysisService;

        public AccuracyCalculator(ApplicationDbContext dbContext, IAnalysisService analysisService)
        {
            this.dbContext = dbContext;
            this.analysisService = analysisService;
        }

        public void CalculateAccuracy()
        {

            Console.WriteLine("Started {0}", DateTime.UtcNow);
            // TODO refactor into smaller methods
            DateTime startDate = new DateTime(DateTime.UtcNow.Year - 1, 9, 1);
            DateTime startDateResults = new DateTime(DateTime.UtcNow.Year - 6, 9, 1);
            var results = dbContext.Results.AsNoTracking().Where(x => x.Date >= startDateResults).ToList();

            List<AccuracyFixture> accuracyFixtures = new List<AccuracyFixture>();

            foreach (var result in results.Where(x => x.Date >= startDate))
            {
                var fixture = new Fixture
                {
                    Date = result.Date,
                    Division = result.Division,
                    HomeTeam = result.HomeTeam,
                    AwayTeam = result.AwayTeam
                };

                var matchOdds = analysisService.GetMatchOdds(fixture, results);
                accuracyFixtures.Add(new AccuracyFixture(fixture, matchOdds, result.FullTimeResult));
            }

            List<Models.Accuracy> accuracies = new List<Models.Accuracy>();

            foreach (var accuracyFixtureGroup in accuracyFixtures.GroupBy(x => x.Fixture.Division))
            {
                var accuracy = new Models.Accuracy();
                accuracy.Division = accuracyFixtureGroup.Key;
                accuracy.Matches = accuracyFixtureGroup.Count();
                accuracy.Recommended = accuracyFixtureGroup.Where(x => x.IsRecommended).Count();
                accuracy.LowerRecommended = accuracyFixtureGroup.Where(x => x.IsLowerRecommended).Count();
                accuracy.RecommendedAccuracy = DecimalExtensions.SafeDivide(accuracyFixtureGroup.Where(x => x.IsRecommended && x.IsRecommendedCorrect).Count(), accuracy.Recommended);
                accuracy.LowerRecommendedAccuracy = DecimalExtensions.SafeDivide(accuracyFixtureGroup.Where(x => x.IsLowerRecommended && x.IsLowerRecommendedCorrect).Count(), accuracy.LowerRecommended);
                accuracies.Add(accuracy);
            }

            dbContext.Database.ExecuteSqlRaw("TRUNCATE table accuracy;");
            dbContext.Accuracy.AddRange(accuracies);
            dbContext.SaveChanges();


            Console.WriteLine("Ended {0}", DateTime.UtcNow);
        }
    }
}
