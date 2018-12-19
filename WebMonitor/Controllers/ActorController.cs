using System;
using Akka.Actor;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Messages;
using WebMonitor.Actors;

namespace WebMonitor.Controllers
{
    public class DataDto
    {
        public string Message { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ActorController : ControllerBase
    {
        private readonly IActorRef _injectedActor;

        public ActorController(InjectedActorProvider injectedActorProvider)
        {
            _injectedActor = injectedActorProvider();
        }
        // GET: api/Actor
        [HttpPost]
        public IActionResult Post([FromBody] DataDto data)
        {
            _injectedActor.Tell(new SignalRMessage($"{DateTime.Now}: ActorController", "SignalR", data.Message));
            return Ok(new
            {
                message = $"Successfully sent [{data.Message}] to controller."
            });

        }


    }
}
