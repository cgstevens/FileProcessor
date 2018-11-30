using Akka.Actor;

namespace SharedLibrary.Messages
{

    public class FoundAvailableWorkers
    {
        public int WorkersAvailable { get; }
        public ReadFile ReadFile { get; }
        public FoundAvailableWorkers(int workersAvailable, ReadFile readFile)
        {
            ReadFile = readFile;
            WorkersAvailable = workersAvailable;
        }
    }

    public class SignalRMessage
    {
        public string System { get; }
        public string Actor { get; }
        public string Message { get; }

        public SignalRMessage(string system, string actor, string message)
        {
            System = system;
            Actor = actor;
            Message = message;
        }
    }
    public interface ISubscribe
    {
        IActorRef Requestor { get; }
        string Location { get; }
    }

    public class ObjectChanged
    {
        public string ObjectType { get; }
        public object ObjectValue { get; }

        public ObjectChanged(string objectType, object objectValue)
        {
            ObjectType = objectType;
            ObjectValue = objectValue;
        }
    }

    
    public class UnSubscribeToObjectChanges
    {
        public IActorRef Requestor { get; }

        public UnSubscribeToObjectChanges(IActorRef requestor)
        {
            Requestor = requestor;
        }
    }
    public class SubscribeToObjectChanges 
    {
        public IActorRef Requestor { get; }
        public string Location { get; }
        public string ObjectToSubscribeTo { get; }

        public SubscribeToObjectChanges(IActorRef requestor, string location, string objectToSubscribeTo)
        {
            Requestor = requestor;
            Location = location;
            ObjectToSubscribeTo = objectToSubscribeTo;
        }
    }
}
