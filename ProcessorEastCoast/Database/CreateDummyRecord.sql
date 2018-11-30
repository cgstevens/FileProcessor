
DECLARE @TenantName varchar(50);
SET @TenantName = 'Colorado'


IF(NOT EXISTS(SELECT LocationId FROM [dbo].[Location] WHERE [Name] = @TenantName))
BEGIN 
	-- Insert dummy records if it doesn't exist.

	INSERT INTO [dbo].[Location] ([Name])
	VALUES (@TenantName)

	INSERT INTO [AkkaFileProcessor].[dbo].[FileSettings] ([LocationId]
		  ,[InboundFolder]
		  ,[ProcessedFolder]
		  ,[ErrorFolder])
	VALUES (SCOPE_IDENTITY(), 'C:\Common\' +@TenantName, 'C:\Common\' +@TenantName, 'C:\Common\' +@TenantName)

END
ELSE
BEGIN
	-- Remove dummy record.
	DECLARE @locationId int; 
	SELECT @locationId = Locationid from [dbo].[Location] WHERE name = @TenantName;
	DELETE FROM dbo.[FileSettings] WHERE LocationId = @locationId;
	DELETE FROM dbo.Location WHERE LocationId = @locationId;

END


exec [dbo].[spLocationsWithFileSettings_Get]