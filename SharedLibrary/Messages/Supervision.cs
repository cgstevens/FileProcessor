using System;
using Akka.Actor;

namespace SharedLibrary.Messages
{
    public class ValidInput { }

    public class BadDataShutdown {
        public IActorRef FailedActor { get; }
        public string CurrentRecord { get; }

        public BadDataShutdown(IActorRef failedActor, string currentRecord)
        {
            FailedActor = failedActor;
            CurrentRecord = currentRecord;
        }
    }

    public class BadDataException : Exception
    {
        public BadDataException(string message) : base(message) { }

        public BadDataException(string message, Exception innerEx) : base(message, innerEx) { }
    }
}
