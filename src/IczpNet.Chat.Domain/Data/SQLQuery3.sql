/****** Script for SelectTopNRows command from SSMS  ******/

--INSERT INTO [Chat_Module].[dbo].[Chat_ChatObject] 
--      ([Id],[Name],[Code],[CreationTime] )
--SELECT [Id],[Name],[Code],[CreationTime]  from [Organization_Module].[dbo].[Organization_Employee] 

--INSERT INTO [Chat_Module_v3].[dbo].[Chat_ChatObject] 
--      ([Name],[Code],[CreationTime],[MaxMessageAutoId],[IsStatic],[IsActive],[FullPath],[FullPathName],[Depth],[Sorting] )
--SELECT [Name],[Code],[CreationTime],0,0,0,'','',0,0  from [Organization_Module].[dbo].[Organization_Employee] 

SELECT id,lastMessageId,LastMessageAutoId FROM [Chat_Module].[dbo].[Chat_Session] Order by LastMessageAutoId desc
SELECT * FROM [Chat_Module].[dbo].[Chat_SessionUnit] order by Sorting desc

SELECT count(Id) FROM [Chat_Module].[dbo].[Chat_Message]
SELECT count(Id) FROM [Chat_Module].[dbo].[Chat_SessionUnit]

SELECT top 100 Id,AutoId,SenderId,SessionUnitCount,SessionId  FROM [Chat_Module].[dbo].[Chat_Message] Order by AutoId desc
SELECT top 100 *  FROM [Chat_Module].[dbo].[Chat_SessionUnit] Order by CreationTime desc

SELECT * FROM [Chat_Module].[dbo].[Chat_SessionTag]

SELECT * FROM [Chat_Module].[dbo].[Chat_SessionRole]

SELECT * FROM [Chat_Module].[dbo].[Chat_ChatObject] where Id='0d1c42e5-bcc1-bdc8-355e-3a085097a2db'


SELECT top 1000 *  FROM [Chat_Module].[dbo].[Chat_Session] where id='003ddfa5-125f-4c53-8625-3a085097a2fe'
SELECT top 1000 * FROM [Chat_Module].[dbo].[Chat_SessionUnit] where SessionId='003ddfa5-125f-4c53-8625-3a085097a2fe'
SELECT top 1000 * FROM [Chat_Module].[dbo].[Chat_Message] where SessionId='003ddfa5-125f-4c53-8625-3a085097a2fe'

update [Chat_Module].[dbo].[Chat_Message] set IsRemindAll = 1 where autoid=561

update [Chat_Module].[dbo].[Chat_SessionUnit] set [ReadedMessageAutoId]=564 where [OwnerId]= '5B6A6100-CA52-8040-09F2-3A07D4A367ED'

update [Chat_Module].[dbo].[Chat_Message] set channel='PrivateChannel'
update [Chat_Module].[dbo].[Chat_Message] set SessionId=null
--delete FROM [Chat_Module].[dbo].[Chat_Session]
--delete from [Chat_Module].[dbo].[Chat_SessionUnit]

update  [Chat_Module].[dbo].[Chat_SessionUnit] set [ReadedMessageAutoId]=300


select * from [Chat_Module].[dbo].[Chat_Room]

/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (50000) *
  FROM [Chat_Module].[dbo].[Chat_SessionUnit]  A
  left join [Chat_Module].[dbo].[Chat_Session] B on A.SessionId = B.Id
  Order by A.LastMessageAutoId desc
  --Order by ReadedMessageAutoId desc
  --Order by LastModifierId desc


SELECT count(1) as [TotalCount]  FROM [Chat_Module].[dbo].[Chat_SessionUnit]

SELECT *  FROM [Chat_Module].[dbo].[Chat_SessionUnit] where [LastMessageAutoId]=3660250


--Update [Chat_Module].[dbo].[Chat_SessionUnit] set ReadedMessageAutoId = 457,LastModificationTime = GetDate()
--	Where Id in(
--		SELECT TOP (10000) Id  FROM [Chat_Module].[dbo].[Chat_SessionUnit]
--	)


SELECT COUNT(*)
FROM [Chat_Module].[dbo].[Chat_SessionUnit] AS [c]
WHERE ((@__ef_filter__p_0 = CAST(1 AS bit)) OR ([c].[IsDeleted] = CAST(0 AS bit))) AND ((@__ef_filter__p_1 = CAST(1 AS bit)) OR ([c].[TenantId] IS NULL))


--更新 LastMessageAutoId
--Update [Chat_Module].[dbo].[Chat_SessionUnit] set [LastMessageAutoId]=B.LastMessageAutoId

--	FROM [Chat_Module].[dbo].[Chat_SessionUnit] A
--	INNER join [Chat_Module].[dbo].[Chat_Session] B 
--		on A.SessionId=B.Id
--	Where B.LastMessageAutoId!=0 and B.LastMessageAutoId is not null
	

SELECT TOP (50000) *
  FROM [Chat_Module].[dbo].[Chat_SessionUnit] Order by [LastMessageAutoId] desc


--  SELECT TOP (50000) *
--  FROM [Chat_Module].[dbo].[Chat_SessionUnit]  A
--  left join [Chat_Module].[dbo].[Chat_Session] B on A.SessionId = B.Id
--  Order by A.LastMessageAutoId desc



