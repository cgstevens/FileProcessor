using System;
using System.Collections.Generic;
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

            var locations = new List<LocationModel>();
            locations.Add(new LocationModel(1, "Colorado"));
            locations.Add(new LocationModel(2, "Wyoming"));
            locations.Add(new LocationModel(3, "Florida"));
            locations.Add(new LocationModel(4, "NewYork"));

            MockFileProcessorRepository
                .Setup(repo => repo.GetLocationsWithFileSettings())
                .Returns(locations);


            Sys.Settings.Config.WithFallback(Akka.Cluster.Tools.PublishSubscribe.DistributedPubSub.DefaultConfig());
        }


        //[Test]
        //public void AddRemoveLocationsFromCollection2()
        //{

        //    var mockRepo = MockFileProcessorRepository
        //        .Setup(x => x.GetLocationsWithFileSettings());


        //    // create coordinator, which spins up odd/even child actors
        //    var coordinator = Sys.ActorOf(Props.Create(() => new CreateUserActor(MockFileProcessorRepository.Object)), "CreateUser");

        //    coordinator.Tell(new SubscribeToObjectChanges(ActorRefs.Nobody, "*", "*"));

        //    // TestActor is sender so it will get reply (coordinator forwards instead of Tells)
        //    ExpectMsg<ValidInput>();
        //}

        [Test]
        public void AddRemoveLocationsFromCollection()
        {
            var coordinator = Sys.ActorOf(Props.Create(() => new DatabaseWatcherActor(MockFileProcessorRepository.Object)), "DatabaseWatcher");

            coordinator.Tell(new SubscribeToObjectChanges(ActorRefs.Nobody, "*", "*"));

            ExpectMsg<AddLocation>();
        }


    }

}
