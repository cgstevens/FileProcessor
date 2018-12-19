using System;
using System.Runtime.InteropServices;
using Akka.Actor;
using SharedLibrary.Helpers;

namespace DemoDeploy
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "DemoDeploy";

            var remoteString = @"
                    akka {
                        actor.provider = remote
                        remote {
                            dot-netty.tcp {
                                port = 4080
                                hostname = 127.0.0.1
                            }
                        }
                }";

            ActorSystem.Create("MyWorker", remoteString);
            Console.ReadKey();
        }
    }
}

