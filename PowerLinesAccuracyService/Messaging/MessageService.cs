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
using PowerLinesAccuracyService.Accuracy;

namespace PowerLinesAccuracyService.Messaging
{
    public class MessageService : BackgroundService, IMessageService
    {
        private IConsumer resultsConsumer;
        private IConsumer oddsConsumer;
        private MessageConfig messageConfig;
        private IServiceScopeFactory serviceScopeFactory;
        private ISender sender;

        public MessageService(IConsumer resultsConsumer, IConsumer oddsConsumer, ISender sender, MessageConfig messageConfig, IServiceScopeFactory serviceScopeFactory)
        {
            this.resultsConsumer = resultsConsumer;
            this.oddsConsumer = oddsConsumer;
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
            oddsConsumer.Listen(new Action<string>(ReceiveOddsMessage));
        }

        public void CreateConnectionToQueue()
        {
            sender.CreateConnectionToQueue(new BrokerUrl(messageConfig.Host, messageConfig.Port, messageConfig.OddsUsername, messageConfig.OddsPassword).ToString(),
                messageConfig.OddsQueue);

            resultsConsumer.CreateConnectionToQueue(QueueType.ExchangeFanout, new BrokerUrl(messageConfig.Host, messageConfig.Port, messageConfig.ResultUsername, messageConfig.ResultPassword).ToString(),
                messageConfig.ResultQueue);

            oddsConsumer.CreateConnectionToQueue(QueueType.ExchangeDirect, new BrokerUrl(messageConfig.Host, messageConfig.Port, messageConfig.OddsUsername, messageConfig.OddsPassword).ToString(),
                messageConfig.OddsQueue);
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

        private void ReceiveOddsMessage(string message)
        {
            Console.WriteLine("New match odds received: {0}", message);
            var matchOdds = JsonConvert.DeserializeObject<MatchOdds>(message);
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.MatchOdds.Upsert(matchOdds)
                    .On(x => new { x.ResultId })
                    .Run();
            }
        }
    }
}
