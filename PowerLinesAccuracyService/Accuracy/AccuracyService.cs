using PowerLinesAccuracyService.Data;
using Microsoft.EntityFrameworkCore;
using PowerLinesAccuracyService.Extensions;

namespace PowerLinesAccuracyService.Accuracy;

public class AccuracyService : BackgroundService
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private Timer timer;
    private readonly int frequencyInMinutes;

    public AccuracyService(IServiceScopeFactory serviceScopeFactory, int frequencyInMinutes = 5)
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
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return dbContext.MatchOdds.AsNoTracking().OrderByDescending(x => x.Calculated).Select(x => x.Calculated).FirstOrDefault();
    }

    public void CheckPendingAccuracy(DateTime lastResultDate)
    {
        var accuracyCalculatedDate = GetAccuracyCalculatedDate();
        CalculateAccuracyIfPending(lastResultDate, accuracyCalculatedDate);
    }

    private DateTime? GetAccuracyCalculatedDate()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return dbContext.Accuracy.AsNoTracking().OrderByDescending(x => x.Calculated).Select(x => x.Calculated).FirstOrDefault();
    }

    private void CalculateAccuracyIfPending(DateTime? lastOddsDate, DateTime? accuracyCalculatedDate)
    {
        if (!lastOddsDate.HasValue || (lastOddsDate.Value > DateTime.UtcNow.AddMinutes(-5)))
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
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var divisions = dbContext.Results.AsNoTracking().Select(x => x.Division).Distinct().ToList();

        foreach (var division in divisions)
        {
            var testResults = dbContext.Results.AsNoTracking().Include(x => x.MatchOdds).Where(x => x.Division == division && x.MatchOdds != null).ToList();

            var accuracy = new Models.Accuracy();
            accuracy.Division = division;
            accuracy.Matches = testResults.Count;
            accuracy.Recommended = testResults.Where(x => x.MatchOdds.IsRecommended).Count();
            accuracy.LowerRecommended = testResults.Where(x => x.MatchOdds.IsLowerRecommended).Count();
            accuracy.RecommendedAccuracy = Math.Round(DecimalExtensions.SafeDivide(testResults.Where(x => x.MatchOdds.Recommended == x.FullTimeResult).Count(), accuracy.Recommended), 2);
            accuracy.LowerRecommendedAccuracy = Math.Round(DecimalExtensions.SafeDivide(testResults.Where(x => x.MatchOdds.LowerRecommended == x.FullTimeResult).Count(), accuracy.LowerRecommended), 2);
            accuracy.Calculated = DateTime.UtcNow;

            dbContext.Accuracy.Upsert(accuracy)
                .On(x => new { x.Division })
                .Run();
        }
    }
}
