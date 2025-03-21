/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) [SessionUnitId]
      ,[MessageId]
      ,[ExtraProperties]
      ,[ConcurrencyStamp]
      ,[CreationTime]
      ,[CreatorId]
      ,[LastModificationTime]
      ,[LastModifierId]
  FROM [dbo].[Chat_Scoped]

delete FROM [dbo].[Chat_Scoped] where [MessageId]=1690847

declare @sessionId uniqueidentifier
set @sessionId='5218F316-B3F8-5B75-1C61-3A06AA18010D'
--select count(1) from [dbo].[Chat_SessionUnit] where SessionId=@sessionId

INSERT INTO [dbo].[Chat_Scoped] ([SessionUnitId],[MessageId],[ExtraProperties],[CreationTime])

SELECT  Id, 
(select top 1 id from [dbo].[Chat_Message] where SessionId=@sessionId order by id desc) as maxMessageId
, '{}',getdate()
FROM [dbo].[Chat_SessionUnit]
WHERE SessionId=@sessionId




