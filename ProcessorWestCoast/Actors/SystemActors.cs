using Akka.Actor;

namespace ProcessorWestCoast.Actors
{

    public static class SystemActors
    {
        public static ActorSystem ClusterSystem;

        public static IActorRef Mediator = ActorRefs.Nobody;
        public static IActorRef LocationManagerActorRef = ActorRefs.Nobody;
        public static IActorRef LocationManagerProxyRef = ActorRefs.Nobody;
        public static IActorRef SettingsWatcherRef = ActorRefs.Nobody;
    }
}