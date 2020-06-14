using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PowerLinesAccuracyService.Data;
using PowerLinesAccuracyService.Models;
using System.Collections.Generic;
using PowerLinesAccuracyService.Messaging;
using Microsoft.EntityFrameworkCore;
using PowerLinesAccuracyService.Analysis;
using PowerLinesAccuracyService.Extensions;

namespace PowerLinesAccuracyService.Accuracy
{
    public class AccuracyService : BackgroundService, IAccuracyService
    {
        private IServiceScopeFactory serviceScopeFactory;
        private Timer timer;
        private int frequencyInMinutes;

        public AccuracyService(IServiceScopeFactory serviceScopeFactory, int frequencyInMinutes = 10)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.frequencyInMinutes = frequencyInMinutes;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            timer = new Timer(CalculateAccuracy, null, TimeSpan.Zero, TimeSpan.FromMinutes(frequencyInMinutes));
            return Task.CompletedTask;
        }

        public void CalculateAccuracy(object state)
        {
            var lastOddsDate = GetLastOddsDate();

            if (lastOddsDate.HasValue)
            {
                CheckPendingAccuracy(lastOddsDate.Value);
            }
        }

        private DateTime? GetLastOddsDate()
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return dbContext.MatchOdds.AsNoTracking().OrderBy(x => x.Calculated).Select(x => x.Calculated).FirstOrDefault();
            }
        }

        public void CheckPendingAccuracy(DateTime lastResultDate)
        {
            var accuracyCalculatedDate = GetAccuracyCalculatedDate();
            CalculateAccuracyIfPending(lastResultDate, accuracyCalculatedDate);
        }

        private DateTime? GetAccuracyCalculatedDate()
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return dbContext.Accuracy.AsNoTracking().OrderBy(x => x.Calculated).Select(x => x.Calculated).FirstOrDefault();
            }
        }

        private void CalculateAccuracyIfPending(DateTime? lastOddsDate, DateTime? accuracyCalculatedDate)
        {
            if (!lastOddsDate.HasValue || (lastOddsDate.Value > DateTime.UtcNow.AddMinutes(-10)))
            {
                return;
            }
            if (!accuracyCalculatedDate.HasValue || (accuracyCalculatedDate.Value < lastOddsDate.Value))
            {
                Calculate();
            }
        }

        public void Calculate()
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var divisions = dbContext.Results.AsNoTracking().Select(x=>x.Division).Distinct().ToList();

                foreach (var division in divisions)
                {
                    var testResults = dbContext.Results.AsNoTracking().Where(x=>x.MatchOdds != null).ToList();
                    
                    var accuracy = new Models.Accuracy();
                    accuracy.Division = division;
                    accuracy.Matches = testResults.Count();
                    accuracy.Recommended = testResults.Where(x => x.MatchOdds.IsRecommended).Count();
                    accuracy.LowerRecommended = testResults.Where(x => x.MatchOdds.IsLowerRecommended).Count();
                    accuracy.RecommendedAccuracy = DecimalExtensions.SafeDivide(testResults.Where(x => x.MatchOdds.Recommended == x.FullTimeResult).Count(), accuracy.Recommended);
                    accuracy.LowerRecommendedAccuracy = DecimalExtensions.SafeDivide(testResults.Where(x => x.MatchOdds.LowerRecommended == x.FullTimeResult).Count(), accuracy.LowerRecommended);
                    accuracy.Calculated = DateTime.UtcNow;
                
                    dbContext.Accuracy.Upsert(accuracy)
                        .On(x => new { x.Division })
                        .Run();
                }
            }
        }        
    }
}
