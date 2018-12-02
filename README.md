## Monitor and get real-time statuses using Akka.Net and SignalR

Using the actor model we can create powerful concurrent scalable & distributed applications with Akka.Net.  


Are you tired of having to put locks on your shared objects and having to manage your threads.
Do you want to be able to scale your application up by adding actors and even scale out the cluster by just bringing up another service?  
Want to make your application real-time by using a message bus?
Then hopefully this will show you how to create powerful concurrent & distributed applications using Akka.Net and SignalR.
 

To get the solution running follow these step.

1. Install the database - Run the database script Database\BuildAllDatabaseThings.sql.
   	This will setup 2 tables and 2 stored procedues.
   
	**Tables**  
	
		dbo.Location - Contains a location (this is just an example and could be anything from a state or a region)
		dbo.FileSetting - Contains the settings about what folder should be monitored.  
		
	**Store Procedues**
	
		dbo.spLocationsWithFileSettings_Get - Gets all the Location and FileSettings.
		dbo.spLongRunningProcess_ProcessAllThingsMagically - Is used to fake like there is a long running process 
									which raises messages for status update.

2. Create a location - Run the database script Database\CreateDummyRecord.sql.
	This will create a location and file setting for the actor system to use.
	The setting default to c:\common\<LocationName>.  
	When the Processor project runs and picks up the location it will create the folder.

3. Build the solution and launch the following applications.
 

## Frameworks
* **DotNetCore 2.1**

	https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-core-2-1

* **SignalR**

	https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr?view=aspnetcore-2.1&tabs=visual-studio

* **NetStandard 2.0**
	
	https://docs.microsoft.com/en-us/dotnet/standard/net-standard

* **Akka.Net 1.3.10**

	https://getakka.net/ 

	Allows you to build powerful distributed event driven sytems using actors.  
	
* **Akka.Cluster 1.3.10**

	https://getakka.net/articles/clustering/cluster-overview.html
	

* **Akka.Cluster.Tools 1.3.10** - The cluster tools brings us the ability to have the following.
	- Singleton: https://getakka.net/articles/clustering/cluster-singleton.html
	- Distributed Pub/Sub: https://getakka.net/articles/clustering/distributed-publish-subscribe.html
 	- Sharding: https://getakka.net/articles/clustering/cluster-sharding.html
	
* **FileHelpers 3.3.0**

	https://www.filehelpers.net/

	By creating a simple class that describes the file I wanted to import allowed me to easily read the contents of a file.
	You can see the simple example in the SharedLibrary\Actors\FileReaderActor.cs file.
	
		var engine = new FileHelperEngine<FileModel>();
		var records = engine.ReadFile(file.Args.FullPath);
	
* **Knockout 3.3.0**

	https://knockoutjs.com/

	This made it easy to wire up bindings for a quick demo.  
	You can see this in action in the WebMonitor in the WebMonitor\wwwroot\index.html file.


## Projects 

### Lighthouse 
It is a service-discovery service called a seed node. To maintain fault tolerance you should always run two instances which allows other members to join when needed. 


### Processor
In a highly-available application, it is occasionally necessary to have some processing performed in isolation within a cluster, while still allowing for processing to continue should the designated worker node fail. Cluster Singleton pattern makes this very easy. For example, consider a system that processes a file from a clustered file system or aggregating and then needs to perform some sort of action on these records within the file. 

You can see my older version created using DotNet 4.6 and AngularJS.  Same concept as this example but dealt with invoices.
	https://github.com/cgstevens/MyClusterServices

In the FileProcessor example I have created a Singleton Actor.  This actor will only exist on either the ProcessorEastCoast or ProcessorWestCoast services.  Since all actor systems are hierarchical; lets start are the top.

* **DatabaseWatcherActor**
	This actor is created for the sole purpose to watch the database for changes and tell actors about these changes.
	It maintains its own cache of the Locations and its FileSettings in this example.  

	If an actor would like to subscribe to the InboundFolder within the FileSettings model that is a property of the Location model.
	All it would need to do is send the following tell.  This example demonstrates the usage of **ActorSelection** to tell the DatabaseWatcher that it would like to subscribe to FileSettings.InboundFolder.  Then when that property changes this actor is sent those changes.

		Context.ActorSelection("/user/DatabaseWatcher").Tell(new SubscribeToObjectChanges(self, _name, "FileSettings.InboundFolder"));
            
		

* **LocationManagerActor**
	This is the Singleton Actor.  Its sole purpose is to track 

	My previous example, MyClusterServices, I created remote actors and then sent a message to them to start working.
	In this example I want to keep the managing of the work isolated and the real work I want to have a worker.
	This means that instead of creating this monolitic actors structure remotely; 
	I would create smaller units of work that would be perform remotely.

	Since this actor subcribes to, tenant, the DatabaseWatcher will let the manager know when a location has been addded or removed.
	This actor will create actors based on the name of the Location.	

	* **LocationActor**
		
	



### Worker
The worker is a cluster member ready for the manager to task work off to.
Demonstrates clean exit when itself is removed from the cluster.
The worker will get a set of records and then process those records.
Report back what the status of the work back to the Tasker.

      EastCoast
      WestCoast

### WebMonitor

### WindowsMonitor
Shows the state of the cluster from its point of view.  Allows you to tell a member of the clister to leave or be considered down. 

### SharedLibrary
Contains the messages, paths and actors that are shared between the above projects.




## Common Akka Links to help you along with your Akka adventure!
Main Site: http://getakka.net/

Documentation: http://getakka.net/docs/

The Code (includes basic examples): https://github.com/akkadotnet/getakka.net

Need to ask a question: https://gitter.im/akkadotnet/akka.net

Where do you begin: https://github.com/petabridge/akka-bootcamp

Where do you begin Part2: https://github.com/petabridge/akkadotnet-code-samples

Webcrawler: https://github.com/petabridge/akkadotnet-code-samples/tree/master/Cluster.WebCrawler

Persistent Actors: https://petabridge.com/blog/intro-to-persistent-actors/



