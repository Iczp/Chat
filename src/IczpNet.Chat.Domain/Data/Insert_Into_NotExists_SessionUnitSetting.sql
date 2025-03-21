/****** Script for SelectTopNRows command from SSMS  ******/

select top 1000 * from [dbo].[Chat_SessionUnitSetting]  
--where sessionUnitId='a5b0977f-8aa1-a0c9-14a5-3a0b0cf57204'
order by [LastMessageId] desc

select count(1) from [dbo].[Chat_SessionUnitSetting]

--delete from [dbo].[Chat_SessionUnitSetting]

declare @sessionId uniqueidentifier
set @sessionId='5218F316-B3F8-5B75-1C61-3A06AA18010D'
--select count(1) from [dbo].[Chat_SessionUnit] where SessionId=@sessionId

INSERT INTO [Chat_Module_v3].[dbo].[Chat_SessionUnitSetting] ([SessionUnitId]
      --,[ReadedMessageId]
      --,[HistoryFristTime]
      --,[HistoryLastTime]
      --,[ClearTime]
      --,[RemoveTime]
      --,[MemberName]
      --,[Rename]
      --,[Remarks]
      ,[IsContacts]
      ,[IsImmersed]
      ,[IsShowMemberName]
      ,[IsShowReaded]
      ,[IsStatic]
      ,[IsPublic]
      ,[IsInputEnabled]
      ,[IsEnabled]
      ,[IsCreator]
      --,[InviterId]
      ,[IsKilled]
      --,[KillType]
      --,[KillTime]
      --,[KillerId]
      ,[CreationTime]
      ,[LastModificationTime])

SELECT [Id] 
	--,[ReadedMessageId]
     -- ,[HistoryFristTime]
     -- ,[HistoryLastTime]
      --,[ClearTime]
      --,[RemoveTime]
      --,[MemberName]
      --,[Rename]
      --,[Remarks]
      ,0--,[IsContacts]
      ,0--,[IsImmersed]
      ,0--,[IsShowMemberName]
      ,0--,[IsShowReaded]
      ,0--,[IsStatic]
      ,0--,[IsPublic]
      ,[IsInputEnabled]
      ,[IsEnabled]
      ,[IsCreator]
      --,[InviterId]
      ,0--,[IsKilled]
      --,[KillType]
      --,[KillTime]
      --,[KillerId]
      ,[CreationTime]
      ,[LastModificationTime]
FROM [Chat_Module_v3].[dbo].[Chat_SessionUnit] x
WHERE 1=1
and not exists (select 1 from [dbo].[Chat_SessionUnitSetting] where [SessionUnitId]=x.Id)

and SessionId=@sessionId 