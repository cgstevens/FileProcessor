using System;
using System.Threading.Tasks;
using Akka.Actor;
using LocationStatusViewer.Actors;
using LocationStatusViewer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Shared.Helpers;
using Shared.Messages;

namespace LocationStatusViewer.Hubs
{

    public class StatusHub : Hub
    {
        public void Send(string name, string message)
        {
            SystemActors.SignalRActor.Tell(new SignalRMessage($"{DateTime.Now}: {StaticMethods.GetSystemUniqueName()}", name, message), ActorRefs.Nobody);
        }
}



}