using System;
using Akka.Actor;
using Microsoft.AspNetCore.SignalR;
using SharedLibrary.Helpers;
using SharedLibrary.Messages;
using WebMonitor.Actors;

namespace WebMonitor.Hubs
{

    public class StatusHub : Hub
    {
        public void Send(string name, string message)
        {
            SystemActors.SignalRActor.Tell(new SignalRMessage($"{DateTime.Now}: {StaticMethods.GetSystemUniqueName()}", name, message), ActorRefs.Nobody);
        }
}



}