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

namespace PowerLinesAccuracyService.Accuracy
{
    public class AccuracyService : BackgroundService, IAccuracyService
    {
        private IServiceScopeFactory serviceScopeFactory;
        private Timer timer;
        private int frequencyInMinutes;

        public AccuracyService(IServiceScopeFactory serviceScopeFactory, int frequencyInMinutes = 20)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.frequencyInMinutes = frequencyInMinutes;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            timer = new Timer(CheckAccuracy, null, TimeSpan.Zero, TimeSpan.FromMinutes(frequencyInMinutes));
            return Task.CompletedTask;
        }

        public void CheckAccuracy(object state)
        {
            var lastResultDate = GetLastResultDate();
            var accuracyCalculatedDate = GetAccuracyCalculatedDate();
            CalculateAccuracyIfPending(lastResultDate, accuracyCalculatedDate);
            
        }

        private DateTime? GetLastResultDate()
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return dbContext.Results.AsNoTracking().OrderByDescending(x => x.Created).Select(x => x.Created).FirstOrDefault();
            }
        }

        private DateTime? GetAccuracyCalculatedDate()
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return dbContext.Accuracy.AsNoTracking().OrderBy(x => x.Calculated).Select(x => x.Calculated).FirstOrDefault();
            }
        }

        private void CalculateAccuracyIfPending(DateTime? lastResultDate, DateTime? accuracyCalculatedDate)
        {
            if(!lastResultDate.HasValue || (lastResultDate.Value > DateTime.UtcNow.AddMinutes(-5)))
            {
                return;
            }
            if(!accuracyCalculatedDate.HasValue || (accuracyCalculatedDate.Value < lastResultDate.Value))
            {
                CalculateAccuracy();
            }
        }

        private void CalculateAccuracy()
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var accuracyCalculator = scope.ServiceProvider.GetRequiredService<IAccuracyCalculator>();
                accuracyCalculator.CalculateAccuracy();
            }
        }
    }
}
