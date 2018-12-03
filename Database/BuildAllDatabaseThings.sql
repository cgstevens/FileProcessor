-- ******************************************************************************************************************************
-- DATABASE
-- ******************************************************************************************************************************


-- ==============================================================================================================================
-- BEGIN database
-- ------------------------------------------------------------------------------------------------------------------------------

SET NOCOUNT ON

USE master
GO

IF NOT EXISTS (SELECT * FROM sys.databases d WHERE d.name = 'AkkaFileProcessor')
BEGIN
  PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - Creating AkkaFileProcessor Database...'
  BEGIN
    DECLARE @sql nVARCHAR(4000)
    declare @dataPath nVARCHAR(512)
    declare @logPath nVARCHAR(512)

    -- these should be defined in production
    declare @DefaultData nVARCHAR(512)
    exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultData', @DefaultData output

    declare @DefaultLog nVARCHAR(512)
    exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultLog', @DefaultLog output

    -- if not then fall back to where master is located
    declare @MasterData nVARCHAR(512)
    exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer\Parameters', N'SqlArg0', @MasterData output
    select @MasterData=substring(@MasterData, 3, 255)
    select @MasterData=substring(@MasterData, 1, len(@MasterData) - charindex('\', reverse(@MasterData)))
    --'-- Ignore this, it fixes NotePad++ incorrect syntax highlighting caused by the charindex
    declare @MasterLog nVARCHAR(512)
    exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer\Parameters', N'SqlArg2', @MasterLog output
    select @MasterLog=substring(@MasterLog, 3, 255)
    select @MasterLog=substring(@MasterLog, 1, len(@MasterLog) - charindex('\', reverse(@MasterLog)))
    --'-- Ignore this, it fixes NotePad++ incorrect syntax highlighting caused by the charindex
    select @dataPath=isnull(@DefaultData, @MasterData)
    select @logPath=isnull(@DefaultLog, @MasterLog)

    PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - ...DATA file path will be: ''' + @dataPath + '''...'
    PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - ...LOG file path will be: ''' + @logPath + '''...'

    SET @sql = 'CREATE DATABASE [AkkaFileProcessor]
      CONTAINMENT = PARTIAL
      ON PRIMARY
      (
        NAME = N''AkkaFileProcessor'',
        FILENAME = N''' + @dataPath + '\AkkaFileProcessor.mdf'',
        SIZE = 128MB , MAXSIZE = UNLIMITED, FILEGROWTH = 128MB
      )
      LOG ON
      (
        NAME = N''AkkaFileProcessor_log'',
        FILENAME = N''' + @logPath + '\AkkaFileProcessor_log.ldf'',
        SIZE = 1024MB , MAXSIZE = UNLIMITED , FILEGROWTH = 1024MB
      )'
    
    
    EXECUTE sp_executesql @sql
    END

    ALTER DATABASE AkkaFileProcessor SET COMPATIBILITY_LEVEL = 110
    ALTER DATABASE AkkaFileProcessor SET ANSI_NULL_DEFAULT OFF 
    ALTER DATABASE AkkaFileProcessor SET ANSI_NULLS OFF 
    ALTER DATABASE AkkaFileProcessor SET ANSI_PADDING OFF
    ALTER DATABASE AkkaFileProcessor SET ANSI_WARNINGS OFF
    ALTER DATABASE AkkaFileProcessor SET ARITHABORT OFF
    ALTER DATABASE AkkaFileProcessor SET AUTO_CLOSE OFF
    ALTER DATABASE AkkaFileProcessor SET AUTO_CREATE_STATISTICS ON
    ALTER DATABASE AkkaFileProcessor SET AUTO_SHRINK OFF
    ALTER DATABASE AkkaFileProcessor SET AUTO_UPDATE_STATISTICS ON
    ALTER DATABASE AkkaFileProcessor SET CURSOR_CLOSE_ON_COMMIT OFF
    ALTER DATABASE AkkaFileProcessor SET CURSOR_DEFAULT  GLOBAL
    ALTER DATABASE AkkaFileProcessor SET CONCAT_NULL_YIELDS_NULL OFF
    ALTER DATABASE AkkaFileProcessor SET NUMERIC_ROUNDABORT OFF
    ALTER DATABASE AkkaFileProcessor SET QUOTED_IDENTIFIER OFF
    ALTER DATABASE AkkaFileProcessor SET RECURSIVE_TRIGGERS OFF
    ALTER DATABASE AkkaFileProcessor SET DISABLE_BROKER
    ALTER DATABASE AkkaFileProcessor SET AUTO_UPDATE_STATISTICS_ASYNC OFF
    ALTER DATABASE AkkaFileProcessor SET DATE_CORRELATION_OPTIMIZATION OFF
    ALTER DATABASE AkkaFileProcessor SET TRUSTWORTHY OFF
    ALTER DATABASE AkkaFileProcessor SET ALLOW_SNAPSHOT_ISOLATION OFF
    ALTER DATABASE AkkaFileProcessor SET PARAMETERIZATION SIMPLE
    ALTER DATABASE AkkaFileProcessor SET READ_COMMITTED_SNAPSHOT OFF
    ALTER DATABASE AkkaFileProcessor SET HONOR_BROKER_PRIORITY OFF
    ALTER DATABASE AkkaFileProcessor SET RECOVERY FULL
    ALTER DATABASE AkkaFileProcessor SET MULTI_USER
    ALTER DATABASE AkkaFileProcessor SET PAGE_VERIFY CHECKSUM
    ALTER DATABASE AkkaFileProcessor SET DB_CHAINING OFF
    ALTER DATABASE AkkaFileProcessor SET TARGET_RECOVERY_TIME = 0 SECONDS
    ALTER DATABASE AkkaFileProcessor SET READ_WRITE

	PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - Database AkkaFileProcessor - Creation Complete...'
END
GO

-- MAKE SURE IT GOT CREATED. IF IT DIDN'T, STOP RUNNING THE SCRIPT
IF NOT EXISTS (SELECT * FROM sys.databases d WHERE d.name = 'AkkaFileProcessor')
BEGIN
    PRINT ''
    PRINT '============================================================'
    PRINT 'FATAL ERROR:'
    PRINT '------------------------------------------------------------'
    PRINT 'Unable to create database. The script cannot continue.'
    PRINT 'Please examine the errors that have been output and correct'
    PRINT 'them and try again.'
    PRINT '============================================================'
    PRINT ''
    RAISERROR('Unable to create database.', 20, 1) with log
END
GO


-- ------------------------------------------------------------------------------------------------------------------------------
-- END database
-- ==============================================================================================================================


-- ******************************************************************************************************************************
-- Now that we have created and validated the database lets switch to that context.
-- ******************************************************************************************************************************
USE AkkaFileProcessor
GO


-- ******************************************************************************************************************************
-- TABLES
-- ******************************************************************************************************************************


-- ==============================================================================================================================
-- BEGIN dbo.Location
-- ------------------------------------------------------------------------------------------------------------------------------

IF OBJECT_ID('dbo.Location') IS NULL 
BEGIN
    PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - Table [dbo].[Location] - Creating...'

	CREATE TABLE [dbo].[Location](
		[LocationId] [int] IDENTITY(1,1) NOT NULL,
		[Name] [varchar](100) NOT NULL,
		CONSTRAINT [PK_Location_LocationId] PRIMARY KEY ([LocationId])
	)	

    PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - Table [dbo].[Location] - Created...'
END
GO


-- ------------------------------------------------------------------------------------------------------------------------------
-- END dbo.Location
-- ==============================================================================================================================


-- ==============================================================================================================================
-- BEGIN dbo.FileSettings
-- ------------------------------------------------------------------------------------------------------------------------------

IF OBJECT_ID('dbo.FileSettings') IS NULL 
BEGIN
    PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - Table [dbo].[FileSettings]- Creating...'

	CREATE TABLE [dbo].[FileSettings](
		[FileSettingsId] [int] IDENTITY(1,1) NOT NULL,
		[LocationId] [int] NOT NULL,
		[ErrorFolder] [varchar](260) NOT NULL,
		[InboundFolder] [varchar](260) NOT NULL,
		[ProcessedFolder] [varchar](260) NOT NULL,
		CONSTRAINT [PK_FileSettings_FileSettingsId] PRIMARY KEY ([FileSettingsId])
	)

	ALTER TABLE [dbo].[FileSettings]  WITH CHECK ADD  CONSTRAINT [FK_FileSettings_Location_LocationId] FOREIGN KEY([LocationId])
		REFERENCES [dbo].[Location] ([LocationId]) 
		ON UPDATE  CASCADE 
		ON DELETE  CASCADE 

	ALTER TABLE [dbo].[FileSettings] CHECK CONSTRAINT [FK_FileSettings_Location_LocationId]
	
	CREATE UNIQUE NONCLUSTERED INDEX UIX_FileSettings_LocationId ON dbo.FileSettings
	(
		LocationId
	) 
	
    PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - Table [dbo].[FileSettings] - Created...'
END
GO


-- ------------------------------------------------------------------------------------------------------------------------------
-- END dbo.FileSettings
-- ==============================================================================================================================


-- ******************************************************************************************************************************
-- STORED PROCEDURES
-- ******************************************************************************************************************************

-- ==============================================================================================================================
-- BEGIN dbo.spLongRunningProcess_ProcessAllThingsMagically
-- ------------------------------------------------------------------------------------------------------------------------------

IF OBJECT_ID('dbo.spLongRunningProcess_ProcessAllThingsMagically', 'P') IS NULL
BEGIN
	PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - Stored Procedure [dbo].[spLongRunningProcess_ProcessAllThingsMagically]- Creating...'

	EXECUTE sp_executesql @stmt = N'CREATE PROCEDURE [dbo].[spLongRunningProcess_ProcessAllThingsMagically] AS BEGIN SET NOCOUNT ON; END';
END
ELSE
BEGIN
	PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - Stored Procedure [dbo].[spLongRunningProcess_ProcessAllThingsMagically]- Altering...'
END
GO

--------------------------------------------------------------------------------------
--
-- Name: spLongRunningProcess_ProcessAllThingsMagically
--
-- Description: Gets all the File Settings for all Locations
--
-- Revision History
-- 11/23/2018		cstevens		Initial Creation
--
--------------------------------------------------------------------------------------
ALTER PROCEDURE [dbo].[spLongRunningProcess_ProcessAllThingsMagically]
(
	@userName varchar(50),
	@rnd int
)
AS
	
	DECLARE @msg nvarchar(1000);
	SET @msg = @userName + ' is starting.';
	RAISERROR(@msg, 0, 10) WITH NOWAIT;

	DECLARE @i int;
    SET @i = 1;
    WHILE @i < 100
    BEGIN
		SET @msg = @userName + ' is %d complete';
        RAISERROR(@msg, 0, 10, @i) WITH NOWAIT;

        -- Do some processing!		
        WAITFOR DELAY '00:00:01';

        SET @i = @i + @rnd;
				
		if(@i + @rnd >= 100)
			set @i = 100;
    END
	
	
	SET @msg = @userName + ' is 100 percent complete.';
	RAISERROR(@msg, 0, 10) WITH NOWAIT;
GO


PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - Stored Procedure [dbo].[spLongRunningProcess_ProcessAllThingsMagically]- Modified...'

-- ------------------------------------------------------------------------------------------------------------------------------
-- END dbo.spLongRunningProcess_ProcessAllThingsMagically
-- ==============================================================================================================================


-- ==============================================================================================================================
-- BEGIN dbo.spLocationsWithFileSettings_Get
-- ------------------------------------------------------------------------------------------------------------------------------

IF OBJECT_ID('dbo.spLocationsWithFileSettings_Get', 'P') IS NULL
BEGIN
	PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - Stored Procedure [dbo].[spLocationsWithFileSettings_Get]- Creating...'

	EXECUTE sp_executesql @stmt = N'CREATE PROCEDURE [dbo].[spLocationsWithFileSettings_Get] AS BEGIN SET NOCOUNT ON; END';
END
ELSE
BEGIN
	PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - Stored Procedure [dbo].[spLocationsWithFileSettings_Get]- Altering...'
END
GO

--------------------------------------------------------------------------------------
--
-- Name: spLocationsWithFileSettings_Get
--
-- Description: Gets all the File Settings for all Locations
--
-- Revision History
-- 11/23/2018		cstevens		Initial Creation
--
--------------------------------------------------------------------------------------
ALTER PROCEDURE [dbo].[spLocationsWithFileSettings_Get]
AS

	SELECT [T].[LocationId]
		,[T].[Name]
		,[CTS].[ErrorFolder]
		,[CTS].[InboundFolder]
		,[CTS].[ProcessedFolder]
	FROM [dbo].[Location] AS [T]
		INNER JOIN [dbo].[FileSettings] AS [CTS] ON [T].[LocationId] = [CTS].[LocationId]

GO

PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + N' - Stored Procedure [dbo].[spLocationsWithFileSettings_Get]- Modified...'

-- ------------------------------------------------------------------------------------------------------------------------------
-- END dbo.spLocationsWithFileSettings_Get
-- ==============================================================================================================================


PRINT ''
PRINT FORMAT(getdate(), 'MM/dd/yyyy hh:mm:ss:fff tt', 'en-US') + ' - SCRIPT COMPLETED'
