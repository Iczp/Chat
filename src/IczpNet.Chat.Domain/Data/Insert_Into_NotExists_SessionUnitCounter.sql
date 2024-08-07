/****** Script for SelectTopNRows command from SSMS  ******/

select top 1000 * from [dbo].[Chat_SessionUnitCounter]  
--where sessionUnitId='a5b0977f-8aa1-a0c9-14a5-3a0b0cf57204'
order by [LastMessageId] desc

select count(1) from [dbo].[Chat_SessionUnitCounter]

--delete from [dbo].[Chat_SessionUnitCounter]

declare @sessionId uniqueidentifier
set @sessionId='5218F316-B3F8-5B75-1C61-3A06AA18010D'
--select count(1) from [dbo].[Chat_SessionUnit] where SessionId=@sessionId

INSERT INTO [dbo].[Chat_SessionUnitCounter] ([SessionUnitId],[PublicBadge],[PrivateBadge],[RemindAllCount],[RemindMeCount],[FollowingCount],[LastMessageId],[CreationTime])

SELECT  [Id],[PublicBadge],[PrivateBadge],[RemindAllCount],[RemindMeCount],[FollowingCount],[LastMessageId],[CreationTime]
FROM [dbo].[Chat_SessionUnit] x
WHERE 1=1
and not exists (select 1 from [dbo].[Chat_SessionUnitCounter] where [SessionUnitId]=x.Id)

and SessionId=@sessionId 