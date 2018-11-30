using Akka.Actor;

namespace ClusterWorkerEastCoast
{

    public static class SystemActors
    {
        public static ActorSystem ClusterSystem;
        public static IActorRef Mediator = ActorRefs.Nobody;
        public static IActorRef SettingsWatcherRef = ActorRefs.Nobody;
    }
}