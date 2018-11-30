using Akka.Actor;

namespace WorkerEastCoast
{

    public static class SystemActors
    {
        public static ActorSystem ClusterSystem;
        public static IActorRef Mediator = ActorRefs.Nobody;
    }
}