using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using Akka.Persistence;
using SharedLibrary.Actors.RemoteDemo;
using SharedLibrary.Helpers;

namespace Demo.Actors.Remote
{
    public static class RemoteDemo
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
                                port = 4050
                                hostname = 127.0.0.1
                            }
                        }
                    }";


            SystemActors.System = ActorSystem.Create(systemName, remoteString);
            var remoteAddress = Address.Parse($"akka.tcp://MyWorker@127.0.0.1:4080");
            //deploy remotely via config
            var remoteEcho1 = SystemActors.System.ActorOf(Props.Create(() => new JobActor()), "remotejob");

            //deploy remotely via code
            //var remoteEcho2 =
            //    SystemActors.System.ActorOf(
            //        Props.Create(() => new JobActor())
            //            .WithDeploy(Deploy.None.WithScope(new RemoteScope(remoteAddress))), "coderemotejob");


            SystemActors.System.ActorOf(Props.Create(() => new JobManagerActor(remoteEcho1)), "JobStarter1");
            //SystemActors.System.ActorOf(Props.Create(() => new JobManagerActor(remoteEcho2)), "JobStarter2");
        }
    }
}