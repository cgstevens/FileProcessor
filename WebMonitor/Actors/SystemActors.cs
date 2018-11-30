using Akka.Actor;

namespace LocationStatusViewer.Actors
{

    public static class SystemActors
    {
        public static ActorSystem ClusterSystem;
        public static IActorRef Mediator = ActorRefs.Nobody; 
        public static IActorRef SignalRActor = ActorRefs.Nobody;
    }
}