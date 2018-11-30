using Akka.Actor;

namespace ProcessorEastCoast.Actors
{

    public static class SystemActors
    {
        public static ActorSystem ClusterSystem;

        public static IActorRef Mediator = ActorRefs.Nobody;
        public static IActorRef JobManagerActorRef = ActorRefs.Nobody;
        public static IActorRef JobManagerProxyRef = ActorRefs.Nobody;
        public static IActorRef SettingsWatcherRef = ActorRefs.Nobody;
    }
}