using Akka.Actor;

namespace AvalonMonitor.Actors
{
    public class SystemActors
    {
        public static ActorSystem ClusterSystem;
        public static IActorRef Mediator = ActorRefs.Nobody;
        public static IActorRef ClusterManagerActor = ActorRefs.Nobody;
    }
}