/****** Script for SelectTopNRows command from SSMS  ******/
SELECT * FROM [Chat_Module].[dbo].[Chat_ChatObject]

  update [Chat_Module].[dbo].[Chat_ChatObject] set ExtraProperties='{}' where ExtraProperties is null

  select b.SessionId,MAX(b.AutoId)  FROM [Chat_Module].[dbo].[Chat_Session] a 
  join [Chat_Module].[dbo].[Chat_Message] b on a.Id=b.SessionId
  group by b.SessionId

  select id,LastMessageId  FROM [Chat_Module].[dbo].[Chat_Session]


select * from [Chat_Module].[dbo].[Chat_Session] a
inner join (
	select id, AutoId,SessionId from [Chat_Module].[dbo].[Chat_Message] where AutoId in (SELECT MAX(AutoId) FROM [Chat_Module].[dbo].[Chat_Message] group by SessionId)
) b on a.Id=b.SessionId


update [Chat_Module].[dbo].[Chat_Session] 
set [Chat_Module].[dbo].[Chat_Session].LastMessageId = b.Id
from [Chat_Module].[dbo].[Chat_Session]
inner join (
	select id, AutoId,SessionId from [Chat_Module].[dbo].[Chat_Message] where AutoId in (SELECT MAX(AutoId) FROM [Chat_Module].[dbo].[Chat_Message] group by SessionId)
) b
on [Chat_Module].[dbo].[Chat_Session].id=b.SessionId