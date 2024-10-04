using PowerLinesAccuracyService.Data;
using PowerLinesAccuracyService.Models;
using Microsoft.EntityFrameworkCore;
using PowerLinesAccuracyService.Analysis;
using PowerLinesMessaging;
using PowerLinesAccuracyService.Messaging;
using Microsoft.Extensions.Options;

namespace PowerLinesAccuracyService.Accuracy;

public class AnalysisService(IServiceScopeFactory serviceScopeFactory, IAnalysisApi analysisApi, IOptions<MessageOptions> messageOptions, int frequencyInMinutes = 60) : BackgroundService
{
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;
    private readonly IAnalysisApi analysisApi = analysisApi;
    private readonly MessageOptions messageOptions = messageOptions.Value;
    private Timer timer;
    private readonly int frequencyInMinutes = frequencyInMinutes;
    private Connection connection;
    private Sender sender;

    public override Task StartAsync(CancellationToken stoppingToken)
    {
        CreateConnection();
        CreateSender();

        return base.StartAsync(stoppingToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        timer = new Timer(GetMatchOdds, null, TimeSpan.Zero, TimeSpan.FromMinutes(frequencyInMinutes));
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        connection.CloseConnection();
    }

    protected void CreateConnection()
    {
        var options = new ConnectionOptions
        {
            Host = messageOptions.Host,
            Port = messageOptions.Port,
            Username = messageOptions.Username,
            Password = messageOptions.Password
        };
        connection = new Connection(options);
    }

    protected void CreateSender()
    {
        var options = new SenderOptions
        {
            Name = messageOptions.AnalysisQueue,
            QueueName = messageOptions.AnalysisQueue,
            QueueType = QueueType.ExchangeFanout
        };

        sender = connection.CreateSenderChannel(options);
    }

    public void GetMatchOdds(object state)
    {
        var lastResultDateLocal = GetLastResultDateLocal();

        if (lastResultDateLocal == null || (lastResultDateLocal.Value > DateTime.UtcNow.AddMinutes(-10)))
        {
            return;
        }

        var lastResultDate = GetLastResultDate();

        if (lastResultDate.HasValue)
        {
            CheckPendingAccuracy(lastResultDate.Value);
        }
    }

    private DateTime? GetLastResultDate()
    {
        return Task.Run(() => analysisApi.GetLastResultDate()).Result;
    }

    private DateTime? GetLastResultDateLocal()
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return dbContext.Results.AsNoTracking().OrderByDescending(x => x.Created).Select(x => x.Created).FirstOrDefault();
        }
    }

    public void CheckPendingAccuracy(DateTime lastResultDate)
    {
        DateTime startDate = new(DateTime.UtcNow.Year - 3, 9, 1);
        List<Result> pendingResults;
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            pendingResults = dbContext.Results.AsNoTracking().Include(x => x.MatchOdds).Where(x => x.Date >= startDate && (x.MatchOdds == null || x.MatchOdds.Calculated < lastResultDate)).ToList();
        }

        if (pendingResults.Count > 0)
        {
            SendFixturesForAnalysis(ConvertResultsToFixtures(pendingResults));
        }
    }

    public List<Fixture> ConvertResultsToFixtures(List<Result> results)
    {
        List<Fixture> fixtures = new List<Fixture>();

        foreach (var result in results)
        {
            fixtures.Add(new Fixture
            {
                FixtureId = result.ResultId,
                Date = result.Date,
                Division = result.Division,
                HomeTeam = result.HomeTeam,
                AwayTeam = result.AwayTeam
            });
        }

        return fixtures;
    }

    public void SendFixturesForAnalysis(List<Fixture> fixtures)
    {
        foreach (var fixture in fixtures)
        {
            sender.SendMessage(new AnalysisMessage(fixture));
        }
    }
}
