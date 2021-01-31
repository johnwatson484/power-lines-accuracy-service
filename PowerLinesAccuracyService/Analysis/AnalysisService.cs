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
using PowerLinesMessaging;

namespace PowerLinesAccuracyService.Accuracy
{
    public class AnalysisService : BackgroundService
    {
        private IServiceScopeFactory serviceScopeFactory;
        private IAnalysisApi analysisApi;
        private MessageConfig messageConfig;
        private Timer timer;
        private int frequencyInMinutes;
        private IConnection connection;
        private ISender sender;

        public AnalysisService(IServiceScopeFactory serviceScopeFactory, IAnalysisApi analysisApi, MessageConfig messageConfig, int frequencyInMinutes = 60)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.analysisApi = analysisApi;
            this.messageConfig = messageConfig;
            this.frequencyInMinutes = frequencyInMinutes;
        }

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
                Host = messageConfig.Host,
                Port = messageConfig.Port,
                Username = messageConfig.Username,
                Password = messageConfig.Password
            };
            connection = new Connection(options);
        }

        protected void CreateSender()
        {
            var options = new SenderOptions
            {
                Name = messageConfig.AnalysisQueue,
                QueueName = messageConfig.AnalysisQueue,
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
            DateTime startDate = new DateTime(DateTime.UtcNow.Year - 3, 9, 1);
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
}
