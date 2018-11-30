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
 
