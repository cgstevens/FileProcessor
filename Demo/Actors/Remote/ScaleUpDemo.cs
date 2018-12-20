using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using Akka.Persistence;
using Akka.Routing;
using SharedLibrary.Actors.RemoteDemo;
using SharedLibrary.Helpers;

namespace Demo.Actors.Remote
{
    public static class ScaleUpDemo
    {
        public static void Start()
        {
            var systemConfig = ConfigurationFactory.ParseString(File.ReadAllText("akka.hocon"));
            var myConfig = systemConfig.GetConfig("myactorsystem");
            var systemName = myConfig.GetString("actorsystem");

            var remoteString = @"
                    akka {  
                        actor{
                            provider = remote
                            deployment {
                                /remotejob {
                                    remote = ""akka.tcp://MyWorker@127.0.0.1:4080""
                                }
                            }
                        }
                        remote {
                            dot-netty.tcp {
                                port = 0
                                hostname = 127.0.0.1
                            }
                        }
                    }";


            SystemActors.System = ActorSystem.Create(systemName, remoteString);

            var scaleUp = 1;
            var props = Props.Create<JobActor>().WithRouter(new RoundRobinPool(scaleUp));


            //var broadcastOut = 5;
            //var props = Props.Create<JobActor>().WithRouter(new BroadcastPool(broadcastOut));


            var remoteEcho1 = SystemActors.System.ActorOf(props, "remotejob");

            SystemActors.System.ActorOf(Props.Create(() => new JobManagerActor(remoteEcho1)), "JobStarter1");
            
        }
    }
}