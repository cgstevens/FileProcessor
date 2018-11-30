using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Akka.Configuration;

namespace Shared.Helpers
{
    public static class StaticMethods
    {

        public static string GetHostIpAddress()
        {
            return ((IEnumerable<IPAddress>)Dns.GetHostEntry(Dns.GetHostName()).AddressList).First().ToString();
        }

        public static string GetServiceWorkerRole()
        {
            var clusterConfig = ConfigurationFactory.ParseString(File.ReadAllText("akka.hocon"));

            return clusterConfig.GetConfig("akka.cluster.roles").Root.GetStringList().First();
        }

        public static string GetSystemName()
        {
            var clusterConfig = ConfigurationFactory.ParseString(File.ReadAllText("akka.hocon"));
            var myConfig = clusterConfig.GetConfig("myactorsystem");
            return myConfig.GetString("systemname"); 
        }

        public static string GetSystemUniqueName()
        {
            var clusterConfig = ConfigurationFactory.ParseString(File.ReadAllText("akka.hocon"));
            var myConfig = clusterConfig.GetConfig("myactorsystem");
            var name = myConfig.GetString("systemname");

            var port = clusterConfig.GetInt("akka.remote.dot-netty.tcp.port");
            if (port == 0)
            {
                var random = new Random();
                port = random.Next(1000, 9999);
            }


            return $"{name} [{port.ToString()}]";
        }

    }
}
