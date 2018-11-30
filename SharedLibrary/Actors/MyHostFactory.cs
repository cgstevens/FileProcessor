using System;
using System.IO;
using System.Linq;
using Akka.Actor;
using Akka.Cluster.Tools.Client;
using Akka.Configuration;
using ConfigurationException = Akka.Configuration.ConfigurationException;

namespace Shared.Actors
{
    public static class SystemHostFactory
    {
        public static ActorSystem Launch()
        {
            var systemName = string.Empty;
            var ipAddress = string.Empty;
            var clusterConfig = ConfigurationFactory.ParseString(File.ReadAllText("akka.hocon"));

            var myConfig = clusterConfig.GetConfig("myactorsystem");
            systemName = myConfig.GetString("actorsystem", systemName);


            var remoteConfig = clusterConfig.GetConfig("akka.remote");
            ipAddress = remoteConfig.GetString("dot-netty.tcp.public-hostname") ??
                            "127.0.0.1"; //localhost as a final default
           
            int port = remoteConfig.GetInt("dot-netty.tcp.port");

            var selfAddress = $"akka.tcp://{systemName}@{ipAddress}:{port}";

            /*
             * Sanity check
             */
            Console.WriteLine($"ActorSystem: {systemName}; IP: {ipAddress}; PORT: {port}");
            Console.WriteLine("Performing pre-boot sanity check. Should be able to parse address [{0}]", selfAddress);
            selfAddress = new Address("akka.tcp", systemName, ipAddress.Trim(), port).ToString();
            Console.WriteLine("Parse successful.");

            var clusterSeeds = Environment.GetEnvironmentVariable("CLUSTER_SEEDS")?.Trim();

            var seeds = clusterConfig.GetStringList("akka.cluster.seed-nodes");
            if (!string.IsNullOrEmpty(clusterSeeds))
            {
                var tempSeeds = clusterSeeds.Trim('[', ']').Split(',');
                if (tempSeeds.Any())
                {
                    seeds = tempSeeds;
                }
            }
            
            if (!seeds.Contains(selfAddress))
            {
                seeds.Add(selfAddress);
            }

            var injectedClusterConfigString = seeds.Aggregate("akka.cluster.seed-nodes = [", (current, seed) => current + (@"""" + seed + @""", "));
            injectedClusterConfigString += "]";

            var finalConfig = ConfigurationFactory.ParseString(
                string.Format(@"akka.remote.dot-netty.tcp.public-hostname = {0} 
akka.remote.dot-netty.tcp.port = {1}", ipAddress, port))
                .WithFallback(ConfigurationFactory.ParseString(injectedClusterConfigString))
                .WithFallback(clusterConfig);

            return ActorSystem.Create(systemName, finalConfig
                .WithFallback(ClusterClientReceptionist.DefaultConfig())
                .WithFallback(Akka.Cluster.Tools.PublishSubscribe.DistributedPubSub.DefaultConfig()));
        }
    }
}
