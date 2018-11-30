using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using WebMonitor.Actors;

namespace WebMonitor.Hubs
{
    /// <inheritdoc />
    /// <summary>
    /// Necessary for getting access to a hub and passing it along to our actors
    /// </summary>
    public class StatusHubHelper : IHostedService
    {
        private readonly IHubContext<StatusHub> _hub;

        public StatusHubHelper(IHubContext<StatusHub> hub)
        {
            _hub = hub;
        }

        internal void WriteMessage(string system, string actor, string message)
        {
            _hub.Clients.All.SendAsync("broadcastMessage", system, actor, message);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            SystemActors.SignalRActor.Tell(new SignalRActor.SetHub(this));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}