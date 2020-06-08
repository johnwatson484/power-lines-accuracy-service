using System;

namespace PowerLinesAccuracyService.Messaging
{
    public interface IMessageService
    {
        void Listen();
        void CreateConnectionToQueue();
    }
}
