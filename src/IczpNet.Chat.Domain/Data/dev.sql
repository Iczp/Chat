/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) * FROM [Chat_Module_v3].[dbo].[Chat_Session]
SELECT TOP (1000) *  FROM [Chat_Module_v3].[dbo].[Chat_SessionUnit]
SELECT TOP (1000) *  FROM [Chat_Module_v3].[dbo].[Chat_Message]


update [Chat_Module_v3].[dbo].[Chat_Session] set LastMessageId =null
update [Chat_Module_v3].[dbo].[Chat_SessionUnit] set LastMessageId =null


delete FROM [Chat_Module_v3].[dbo].[Chat_Message]
delete FROM [Chat_Module_v3].[dbo].[Chat_Session]