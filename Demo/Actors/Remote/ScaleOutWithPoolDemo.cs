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
    public static class ScaleOutWithPoolDemo
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
                                /EastCoast {
                                    remote = ""akka.tcp://MyWorker@127.0.0.1:4080""
                                }
                                /WestCoast {
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

            var scaleUp = 5;
            var props = Props.Create<JobActor>().WithRouter(new RoundRobinPool(scaleUp));

            var eastcoastActor = SystemActors.System.ActorOf(Props.Create(() => new JobActor()), "EastCoast"); 
            var westcoastActor = SystemActors.System.ActorOf(props, "WestCoast"); 

            var workers = new[] { eastcoastActor.Path.ToString(), westcoastActor.Path.ToString() };
            var workerRouter = SystemActors.System.ActorOf(Props.Empty.WithRouter(new RoundRobinGroup(workers)), "WorkersGroup");

            SystemActors.System.ActorOf(Props.Create(() => new JobManagerActor(workerRouter)), "JobManager");
            
        }
    }
}