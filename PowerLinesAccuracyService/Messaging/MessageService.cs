using System;
using PowerLinesAccuracyService.Data;
using PowerLinesAccuracyService.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PowerLinesMessaging;

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
            var resultOptions = new ConsumerOptions
            {
                Host = messageConfig.Host,
                Port = messageConfig.Port,
                Username = messageConfig.ResultUsername,
                Password = messageConfig.ResultPassword,
                QueueName = messageConfig.ResultQueue,
                SubscriptionQueueName = "power-lines-results-accuracy",
                QueueType = QueueType.ExchangeFanout            
            };

            resultsConsumer.CreateConnectionToQueue(resultOptions);

            var oddsConsumerOptions = new ConsumerOptions
            {
                Host = messageConfig.Host,
                Port = messageConfig.Port,
                Username = messageConfig.OddsUsername,
                Password = messageConfig.OddsPassword,
                QueueName = messageConfig.OddsQueue,
                SubscriptionQueueName = "power-lines-odds-accuracy",
                QueueType = QueueType.ExchangeDirect,
                RoutingKey = "power-lines-accuracy-service" 
            };

            oddsConsumer.CreateConnectionToQueue(oddsConsumerOptions);
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
            var matchOdds = JsonConvert.DeserializeObject<MatchOdds>(message);
            matchOdds.SetResultId();
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
