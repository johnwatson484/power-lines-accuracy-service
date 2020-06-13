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

namespace PowerLinesAccuracyService.Accuracy
{
    public class AnalysisService : BackgroundService, IAnalysisService
    {
        private IServiceScopeFactory serviceScopeFactory;
        private IAnalysisApi analysisApi;
        private MessageConfig messageConfig;
        private Timer timer;
        private int frequencyInMinutes;
        private ISender sender;

        public AnalysisService(IServiceScopeFactory serviceScopeFactory, IAnalysisApi analysisApi, MessageConfig messageConfig, int frequencyInMinutes = 1440)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.analysisApi = analysisApi;
            this.messageConfig = messageConfig;
            this.frequencyInMinutes = frequencyInMinutes;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            timer = new Timer(GetMatchOdds, null, TimeSpan.Zero, TimeSpan.FromMinutes(frequencyInMinutes));
            return Task.CompletedTask;
        }

        public void GetMatchOdds(object state)
        {
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

        public void CheckPendingAccuracy(DateTime lastResultDate)
        {
            DateTime startDate = new DateTime(DateTime.UtcNow.Year - 3, 9, 1);
            List<Result> pendingResults;
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                pendingResults = dbContext.Results.AsNoTracking().Where(x => x.Date >= startDate && (x.MatchOdds == null || x.MatchOdds.Calculated < lastResultDate)).ToList();
            }

            if (pendingResults.Count > 0)
            {
                SendFixturesForAnalysis(ConvertResultsToFixtures(pendingResults));
            }
        }

        public List<Fixture> ConvertResultsToFixtures(List<Result> results)
        {
            List<Fixture> fixtures = new List<Fixture>();

            foreach(var result in results)
            {
                fixtures.Add(new Fixture
                {
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
            sender = new Sender();
            CreateConnectionToQueue();

            foreach (var fixture in fixtures)
            {
                sender.SendMessage(new AnalysisMessage(fixture));
            }
        }

        public void CreateConnectionToQueue()
        {
            Task.Run(() =>
                sender.CreateConnectionToQueue(new BrokerUrl(messageConfig.Host, messageConfig.Port, messageConfig.AnalysisUsername, messageConfig.AnalysisPassword).ToString(),
                    messageConfig.AnalysisQueue))
            .Wait();
        }
    }
}
