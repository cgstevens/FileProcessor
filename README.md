Distributed workers with Akka.Net
Using the actor model we can create powerful concurrent scalable & distributed applications using Akka.Net.
	
Scaling up and out using Akka.net
 
Are you tired of having to put locks on your shared objects and having to manage your threads.
Do you want to be able to scale your application up by adding actors and even scale out the cluster by just bringing up another service?  
Want to make your application real-time by using a message bus?
Then hopefully this will show you how to create powerful concurrent & distributed applications using Akka.Net and SignalR.
 

To get the solution running follow these step.

1. Install the database - Run the database script Database\BuildAllDatabaseThings.sql.
   This will setup 2 tables and 2 stored procedues.
        Tables  
		dbo.Location - Contains a location (this is just an example and could be anything from a state or a region)
		dbo.FileSetting - Contains the settings about what folder should be monitored.  
		
	Store Procedues
		dbo.spLocationsWithFileSettings_Get - Gets all the Location and FileSettings.
		dbo.spLongRunningProcess_ProcessAllThingsMagically - Is used to fake like there is a long running process which raises messages for status update.

2. Build the solution and launch the following applications.
 
