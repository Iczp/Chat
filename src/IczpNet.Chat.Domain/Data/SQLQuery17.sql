/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) [SessionUnitId]
      ,[DeviceId]
      ,[MessageId]
      ,[ExtraProperties]
      ,[ConcurrencyStamp]
      ,[CreationTime]
      ,[CreatorId]
      ,[LastModificationTime]
      ,[LastModifierId]
      ,[OwnerId]
      ,[DestinationId]
  FROM [Chat_Module_v3].[dbo].[Chat_OpenedRecorder] WHERE MessageId=1491769


---------------------------------------------

INSERT INTO [dbo].[Chat_OpenedCounter] ([MessageId]
      ,[Count]
      ,[CreationTime]
      ,[LastModificationTime])

SELECT  [Id] 
      ,[ReadedCount]
      ,[CreationTime]
      ,[LastModificationTime]
FROM [dbo].[Chat_Message] x
WHERE 1=1
and not exists (select 1 from [dbo].[Chat_OpenedCounter] where [MessageId]=x.Id)

---------------------------------------------------------

SELECT TOP 100 * FROM [dbo].[Chat_OpenedCounter] WHERE 1=1
--And MessageId=1491769 
Order by [Count] Desc

SELECT TOP (1000) [SessionUnitId]
      ,[DeviceId]
      ,[MessageId]
      ,[ExtraProperties]
      ,[ConcurrencyStamp]
      ,[CreationTime]
      ,[CreatorId]
      ,[LastModificationTime]
      ,[LastModifierId]
      ,[OwnerId]
      ,[DestinationId]
  FROM [dbo].[Chat_OpenedRecorder] WHERE MessageId=1491769

------------------------------------------------------------
--Opened
UPDATE [dbo].[Chat_OpenedCounter] 
SET [Count] = B.Num
FROM [dbo].[Chat_OpenedCounter] AS A INNER JOIN (
	SELECT count(1) AS Num, MessageId FROM [dbo].[Chat_OpenedRecorder]
	GROUP BY MessageId
	) AS B ON A.MessageId = B.MessageId


------------------------------------------------------------
--Readed
UPDATE [dbo].[Chat_ReadedCounter] 
SET [Count] = B.Num
FROM [dbo].[Chat_ReadedCounter] AS A INNER JOIN (
	SELECT count(1) AS Num, MessageId FROM [dbo].[Chat_ReadedRecorder]
	GROUP BY MessageId
	) AS B ON A.MessageId = B.MessageId

------------------------------------------------------------
--Favorited
UPDATE [dbo].[Chat_FavoritedCounter] 
SET [Count] = B.Num
FROM [dbo].[Chat_Favorite] AS A INNER JOIN (
	SELECT count(1) AS Num, MessageId FROM [dbo].[Chat_Favorite]
	GROUP BY MessageId
	) AS B ON A.MessageId = B.MessageId






