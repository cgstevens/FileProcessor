using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using SharedLibrary.Actors.RemoteDemo;
using SharedLibrary.Helpers;

namespace Demo.Actors.ActorSelection
{
    public static class WildCardReporting
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

            var props = Props.Create<JobWithBehaviorActor>().WithRouter(new RoundRobinPool(5));

            var remoteEcho1 = SystemActors.System.ActorOf(props, "remotejob");
            SystemActors.System.ActorOf(Props.Create(() => new JobManagerActor(remoteEcho1)), "JobManager");
            SystemActors.System.ActorOf(Props.Create(() => new ReportActor()), "Reporter");
        }
    }
}