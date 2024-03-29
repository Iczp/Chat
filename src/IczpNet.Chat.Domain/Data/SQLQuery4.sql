/****** Script for SelectTopNRows command from SSMS  ******/
SELECT * FROM [Chat_Module_v3].[dbo].[Chat_SessionUnit]
where 1=1

and SessionId='5218f316-b3f8-5b75-1c61-3a06aa18010d'
--and OwnerId=120 and [IsInputEnabled]=1
order by LastMessageId desc

SELECT TOP (1000) id,lastMessageId,PublicBadge,privateBadge,remindAllCount,remindMeCount,followingCount FROM [Chat_Module_v3].[dbo].[Chat_SessionUnit]
where id='4C27C6CA-6E64-A42B-8635-3A0B0CF571F8'


--SELECT TOP (1000) * FROM [Chat_Module_v3].[dbo].[Chat_SessionRequest]

SELECT count(1) FROM [Chat_Module_v3].[dbo].[Chat_SessionUnit]

SELECT  count(1)  FROM [Chat_Module_v3].[dbo].[Chat_SessionRequest]

SELECT count(1) FROM [Chat_Module_v3].[dbo].[Chat_Follow]

SELECT count(1) FROM [Chat_Module_v3].[dbo].[Chat_MessageReminder]

DELETE FROM [dbo].[Chat_Follow] WHERE OwnerId=DestinationId

select * from [Chat_Module_v3].[dbo].[Chat_MessageReminder] where 1=1
and MessageId > 979121
and SessionUnitId in('719d7227-030a-326a-bce9-3a0b0cf571fe','4010ae00-041e-319b-88cf-3a0b0cf571fb')
order by MessageId desc



select * from [dbo].[Chat_ReadedRecorder] where 1=1 
and [MessageId]=1487936 
--and SessionUnitId='AB32BB26-3B6D-1C52-0555-3A0B0CF571FB'

select * from [dbo].[Chat_OpenedRecorder] where 1=1
and [MessageId]=1487936 
--and SessionUnitId='AB32BB26-3B6D-1C52-0555-3A0B0CF571FB'

select * from [dbo].[Chat_Favorite] where 1=1
and [MessageId]=1487936 
--and SessionUnitId='AB32BB26-3B6D-1C52-0555-3A0B0CF571FB'