using System.IO;
using Akka.Actor;
using Akka.Configuration;

namespace Demo.Actors.AtLeastOnceDelivery
{
    public static class AtLeastOnceDeliveryDemo
    {
        public static void Start()
        {
            var systemConfig = ConfigurationFactory.ParseString(File.ReadAllText("akka.hocon"));
            var myConfig = systemConfig.GetConfig("myactorsystem");
            var systemName = myConfig.GetString("actorsystem");

            SystemActors.System = ActorSystem.Create(systemName);

            var recipientActor = SystemActors.System.ActorOf(Props.Create(() => new MyRecipientActor()), "Worker");
            var atLeastOnceDeliveryActor = SystemActors.System.ActorOf(Props.Create(() => new MyAtLeastOnceDeliveryActor(recipientActor)), "Manager");
            
        }

    }
}