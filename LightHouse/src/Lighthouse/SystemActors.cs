using Akka.Actor;

namespace Lighthouse
{
    public static class SystemActors
    {
        public static ActorSystem ClusterSystem;

        public static IActorRef Mediator = ActorRefs.Nobody;
        
    }
}