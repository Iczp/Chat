/****** Script for SelectTopNRows command from SSMS  ******/

--INSERT INTO [Chat_Module].[dbo].[Chat_ChatObject] 
--      ([Id],[Name],[Code],[CreationTime] )
--SELECT [Id],[Name],[Code],[CreationTime]  from [Organization_Module].[dbo].[Organization_Employee] 



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


select * from [dbo].[Chat_Room]


