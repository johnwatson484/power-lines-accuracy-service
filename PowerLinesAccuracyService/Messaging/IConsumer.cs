using System;

namespace PowerLinesAccuracyService.Messaging
{
    public interface IConsumer
    {
        void CreateConnectionToQueue(QueueType queueType, string brokerUrl, string queue);

        void CloseConnection();

        void Listen(Action<string> messageAction);
    }
}
