using System;
using Akka.Actor;

namespace Shared.Messages
{
    
    public class RepoMessage
    {
        public string Message { get; }

        public RepoMessage(string message)
        {
            Message = message;
        }

    }
}
