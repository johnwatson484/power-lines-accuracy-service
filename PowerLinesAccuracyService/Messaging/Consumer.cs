using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;

namespace PowerLinesAccuracyService.Messaging
{
    public class Consumer : IConsumer
    {
        protected ConnectionFactory connectionFactory;
        protected RabbitMQ.Client.IConnection connection;
        protected IModel channel;
        protected QueueType queueType;
        protected string queue;
        protected string tempQueue;

        public void CreateConnectionToQueue(QueueType queueType, string brokerUrl, string queue)
        {
            this.queueType = queueType;
            this.queue = queue;
            CreateConnectionFactory(brokerUrl);
            CreateConnection();
            CreateChannel();
            CreateQueue();
        }

        public void CloseConnection()
        {
            connection.Close();
        }

        public void Listen(Action<string> messageAction)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                messageAction(message);
            };
            channel.BasicConsume(queue: GetQueueName(),
                                 autoAck: true,
                                 consumer: consumer);
        }

        private void CreateConnectionFactory(string brokerUrl)
        {
            connectionFactory = new ConnectionFactory() {
                Uri = new Uri(brokerUrl)
            };
        }

        private void CreateConnection()
        {
            connection = connectionFactory.CreateConnection();
        }

        private void CreateChannel()
        {
            channel = connection.CreateModel();
        }

        private void CreateQueue()
        {
            if(queueType == QueueType.Worker)
            {
                CreateWorkerQueue();
            }
            else
            {
                CreateExchange();
                BindQueue();
            }
        }

        private void CreateWorkerQueue()
        {
            channel.QueueDeclare(queue: queue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        private void CreateExchange()
        {
            channel.ExchangeDeclare(queue, queueType == QueueType.ExchangeDirect ? ExchangeType.Direct : ExchangeType.Fanout, true, false);
        }
        
        private void BindQueue()
        {
            tempQueue = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: tempQueue,
                              exchange: queue,
                              routingKey: GetExchangeRoutingKey());
        }

        private string GetQueueName()
        {
            return queueType == QueueType.Worker ? queue : tempQueue;
        }

        private string GetExchangeRoutingKey()
        {
            return queueType == QueueType.ExchangeDirect ? "power-lines-accuracy-service" : "";
        }
    }
}
