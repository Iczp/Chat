/****** Script for SelectTopNRows command from SSMS  ******/

use [Chat_Module_v3]

SELECT count(1)  FROM [dbo].[Chat_SessionUnit]
WHERE 1=1
and [Key] is null
--and DestinationObjectType=2



SELECT TOP(1000) [Key],[OwnerId],[DestinationId],*  FROM [dbo].[Chat_SessionUnit]
Where 1=1
--and DestinationObjectType = 2
and SessionId='9BA196C7-A6E8-9EAF-D095-4DE80FC1B224'
order by CreationTime desc


SELECT TOP(1000) *  FROM [dbo].[Chat_Message]
Where 1=1
--and DestinationObjectType = 2
order by CreationTime desc


select top 1000 id,[BadgePublic]  FROM [dbo].[Chat_SessionUnit]
where [BadgePublic]>10000

order by [BadgePublic] desc

----更新Badge

--update [dbo].[Chat_SessionUnit] 
--set [dbo].[Chat_SessionUnit].BadgePublic = (

--	select count(1) from [dbo].[Chat_Message] m
--	where 1=1
--	and x.SessionId=m.SessionId
--	and (x.ReadedMessageId is null or m.Id > x.ReadedMessageId)
--	and x.OwnerId!=m.SenderId
--	and (x.HistoryFristTime is null or m.CreationTime> x.HistoryFristTime)
--	and (x.HistoryLastTime is null or m.CreationTime> x.HistoryLastTime)
--	and (x.ClearTime is null or m.CreationTime> x.ClearTime)
--	)
--from [dbo].[Chat_SessionUnit] x




select top 100
id,
badge=(

	select count(1) from [dbo].[Chat_Message] m
	where 1=1
	and x.SessionId=m.SessionId
	and (x.ReadedMessageId is null or m.Id > x.ReadedMessageId)
	and x.OwnerId!=m.SenderId
	and (x.HistoryFristTime is null or m.CreationTime> x.HistoryFristTime)
	and (x.HistoryLastTime is null or m.CreationTime> x.HistoryLastTime)
	and (x.ClearTime is null or m.CreationTime> x.ClearTime)

	)
FROM [dbo].[Chat_SessionUnit] x
where id in(
'8C7E2673-B1E1-3259-93AD-3A0B0CF57200',
'BBFB23C1-487B-8621-9415-3A0B0CF57200',
'E654CFD2-5A59-1DA4-9595-3A0B0CF57200',
'F8D56229-57CD-5577-9787-3A0B0CF57200',
'3619EC2B-60A2-266D-9788-3A0B0CF57200',
'79A5C875-4230-26F4-97AF-3A0B0CF57200',
'52187C2E-E108-6339-98AA-3A0B0CF57200',
'5275FDD7-94B2-A5CC-9979-3A0B0CF57200',
'2F1B1439-0771-48A9-9AAA-3A0B0CF57200',
'6119C1F0-09B4-0EE2-9B7C-3A0B0CF57200',
'51A78435-6291-6AE1-9B8B-3A0B0CF57200',
'2D3E8395-9E26-8C80-9C49-3A0B0CF57200',
'70ED296F-510D-D278-9CD3-3A0B0CF57200',
'B5E7ED08-347A-A5B4-9CD4-3A0B0CF57200')

order by x.Sorting, x.LastMessageId desc

