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
    public class MessageService : BackgroundService
    {
        private MessageConfig messageConfig;
        private IServiceScopeFactory serviceScopeFactory;
        private IConnection connection;
        private IConsumer resultConsumer;
        private IConsumer oddsConsumer;

        public MessageService(MessageConfig messageConfig, IServiceScopeFactory serviceScopeFactory)
        {
            this.messageConfig = messageConfig;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public override Task StartAsync(CancellationToken stoppingToken)
        {
            CreateConnection();
            CreateResultConsumer();
            CreateOddsConsumer();

            return base.StartAsync(stoppingToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            resultConsumer.Listen(new Action<string>(ReceiveResultMessage));
            oddsConsumer.Listen(new Action<string>(ReceiveOddsMessage));
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

        protected void CreateResultConsumer()
        {
            var options = new ConsumerOptions
            {
                Name = messageConfig.ResultQueue,
                QueueName = messageConfig.ResultQueue,
                SubscriptionQueueName = messageConfig.ResultSubscription,
                QueueType = QueueType.ExchangeFanout
            };

            resultConsumer = connection.CreateConsumerChannel(options);
        }

        protected void CreateOddsConsumer()
        {
            var options = new ConsumerOptions
            {
                Name = messageConfig.OddsQueue,
                QueueName = messageConfig.OddsQueue,
                SubscriptionQueueName = messageConfig.OddsSubscription,
                QueueType = QueueType.ExchangeDirect,
                RoutingKey = "power-lines-accuracy-service"
            };

            oddsConsumer = connection.CreateConsumerChannel(options);
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
