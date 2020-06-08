using System;
using PowerLinesAccuracyService.Data;
using PowerLinesAccuracyService.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PowerLinesAccuracyService.Analysis;

namespace PowerLinesAccuracyService.Messaging
{
    public class MessageService : BackgroundService, IMessageService
    {
        private IConsumer resultsConsumer;
        private IConsumer analysisConsumer;
        private MessageConfig messageConfig;
        private IServiceScopeFactory serviceScopeFactory;
        private ISender sender;

        public MessageService(IConsumer resultsConsumer, IConsumer analysisConsumer, ISender sender, MessageConfig messageConfig, IServiceScopeFactory serviceScopeFactory)
        {
            this.resultsConsumer = resultsConsumer;
            this.analysisConsumer = analysisConsumer;
            this.messageConfig = messageConfig;
            this.serviceScopeFactory = serviceScopeFactory;
            this.sender = sender;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {            
            Listen();
            return Task.CompletedTask;
        }

        public void Listen()
        {
            CreateConnectionToQueue();
            resultsConsumer.Listen(new Action<string>(ReceiveResultMessage));
            analysisConsumer.Listen(new Action<string>(ReceiveAnalysisMessage));
        }

        public void CreateConnectionToQueue()
        {
            sender.CreateConnectionToQueue(new BrokerUrl(messageConfig.Host, messageConfig.Port, messageConfig.OddsUsername, messageConfig.OddsPassword).ToString(),
                messageConfig.OddsQueue);

            resultsConsumer.CreateConnectionToQueue(new BrokerUrl(messageConfig.Host, messageConfig.Port, messageConfig.ResultUsername, messageConfig.ResultPassword).ToString(),
                messageConfig.ResultQueue);

            analysisConsumer.CreateConnectionToQueue(new BrokerUrl(messageConfig.Host, messageConfig.Port, messageConfig.AnalysisUsername, messageConfig.AnalysisPassword).ToString(),
                messageConfig.AnalysisQueue);
        }

        private void ReceiveResultMessage(string message)
        {
            var result = JsonConvert.DeserializeObject<Result>(message);
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    dbContext.Results.Add(result);
                    dbContext.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    Console.WriteLine("{0} v {1} {2} exists, skipping", result.HomeTeam, result.AwayTeam, result.Date.Year);
                }
            }
        }

        private void ReceiveAnalysisMessage(string message)
        {
            var fixture = JsonConvert.DeserializeObject<Fixture>(message);
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var analysisService = scope.ServiceProvider.GetRequiredService<IAnalysisService>();
                try
                {
                    var matchOdds = analysisService.GetMatchOdds(fixture);
                    sender.SendMessage(matchOdds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to calculate match odds for {0} v {1}: {2}", fixture.HomeTeam, fixture.AwayTeam, ex);
                }
            }
        }
    }
}
