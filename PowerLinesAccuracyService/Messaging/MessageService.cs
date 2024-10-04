using PowerLinesAccuracyService.Data;
using PowerLinesAccuracyService.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using PowerLinesMessaging;
using Microsoft.Extensions.Options;

namespace PowerLinesAccuracyService.Messaging;

public class MessageService(IOptions<MessageOptions> messageOptions, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly MessageOptions messageOptions = messageOptions.Value;
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;
    private Connection connection;
    private Consumer resultConsumer;
    private Consumer oddsConsumer;

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
            Host = messageOptions.Host,
            Port = messageOptions.Port,
            Username = messageOptions.Username,
            Password = messageOptions.Password
        };
        connection = new Connection(options);
    }

    protected void CreateResultConsumer()
    {
        var options = new ConsumerOptions
        {
            Name = messageOptions.ResultQueue,
            QueueName = messageOptions.ResultQueue,
            SubscriptionQueueName = messageOptions.ResultSubscription,
            QueueType = QueueType.ExchangeFanout
        };

        resultConsumer = connection.CreateConsumerChannel(options);
    }

    protected void CreateOddsConsumer()
    {
        var options = new ConsumerOptions
        {
            Name = messageOptions.OddsQueue,
            QueueName = messageOptions.OddsQueue,
            SubscriptionQueueName = messageOptions.OddsSubscription,
            QueueType = QueueType.ExchangeDirect,
            RoutingKey = "power-lines-accuracy-service"
        };

        oddsConsumer = connection.CreateConsumerChannel(options);
    }

    private void ReceiveResultMessage(string message)
    {
        var result = JsonConvert.DeserializeObject<Result>(message);
        using var scope = serviceScopeFactory.CreateScope();
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

    private void ReceiveOddsMessage(string message)
    {
        var matchOdds = JsonConvert.DeserializeObject<MatchOdds>(message);
        matchOdds.SetResultId();
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.MatchOdds.Upsert(matchOdds)
            .On(x => new { x.ResultId })
            .Run();
    }
}
