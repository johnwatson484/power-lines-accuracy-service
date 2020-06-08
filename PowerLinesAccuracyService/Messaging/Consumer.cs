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
        protected string queue;

        public void CreateConnectionToQueue(string brokerUrl, string queue)
        {
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
            channel.BasicConsume(queue: queue,
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
            channel.QueueDeclare(queue: queue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }
    }
}
