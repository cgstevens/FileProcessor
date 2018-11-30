using Akka.Actor;

namespace WinForms.Actors
{

    public static class SystemActors
    {
        public static ActorSystem ClusterSystem;
        public static IActorRef Mediator = ActorRefs.Nobody;
        public static IActorRef ClusterManagerActor = ActorRefs.Nobody;
    }
}