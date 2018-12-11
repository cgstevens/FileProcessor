using Akka.Actor;

namespace WebMonitor.Actors
{

    public delegate IActorRef InjectedActorProvider();

    public static class SystemActors
    {
        public static ActorSystem ClusterSystem;
        public static IActorRef Mediator = ActorRefs.Nobody; 
        public static IActorRef SignalRActor = ActorRefs.Nobody;
    }
}