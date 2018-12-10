using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Akka.Actor;
using Akka.Event;
using Akka.TestKit.NUnit;
using Moq;
using Moq.Language.Flow;
using Ninject.Planning.Bindings.Resolvers;
using NUnit.Framework;
using SharedLibrary.Actors;
using SharedLibrary.Messages;
using SharedLibrary.Models;
using SharedLibrary.Repos;

namespace WorkerTests
{


    [TestFixture]
    public class DatabaseWatcherActorTests : TestKit
    {
        protected Mock<IFileProcessorRepository> MockFileProcessorRepository { get; set; }

        [SetUp]
        public virtual void BeforeEachTest()
        {
            MockFileProcessorRepository = new Mock<IFileProcessorRepository>();



            Sys.Settings.Config.WithFallback(Akka.Cluster.Tools.PublishSubscribe.DistributedPubSub.DefaultConfig());
        }


        [Test]
        public void TaskException()
        {
            var locations = new List<LocationModel>();
            locations.Add(new LocationModel(1, "Colorado"));

            MockFileProcessorRepository
                .Setup(repo => repo.GetLocationsWithFileSettings())
                .Throws(new Exception("Error getting locations"));


           
            // expect exception
            EventFilter.Exception<Exception>().Expect(2, () =>
            {
                Sys.ActorOf(Props.Create(() => new DatabaseWatcherActor(MockFileProcessorRepository.Object)), "DatabaseWatcher");
            });
        }

        [Test]
        public void AddRemoveLocationsFromCollection()
        {
            var locations = new List<LocationModel>();
            locations.Add(new LocationModel(1, "Colorado"));
            locations.Add(new LocationModel(2, "Wyoming"));
            locations.Add(new LocationModel(3, "Florida"));
            locations.Add(new LocationModel(4, "NewYork"));

            MockFileProcessorRepository
                .Setup(repo => repo.GetLocationsWithFileSettings())
                .Returns(locations);

            var coordinator = Sys.ActorOf(Props.Create(() => new DatabaseWatcherActor(MockFileProcessorRepository.Object)), "DatabaseWatcher");

            coordinator.Tell(new SubscribeToObjectChanges(ActorRefs.Nobody, "*", "*"));

            ExpectMsg<AddLocation>();
        }


    }

}
