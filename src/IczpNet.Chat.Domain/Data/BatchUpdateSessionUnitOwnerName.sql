use [Chat_Module_v3]


select COUNT(1) from [dbo].[Chat_SessionUnit] 


update [dbo].[Chat_SessionUnit] 
set 
[dbo].[Chat_SessionUnit].[OwnerName] = (
	select top 1 [Name] from [dbo].[Chat_ChatObject] c
	where Id=x.OwnerId
	),
[dbo].[Chat_SessionUnit].[OwnerNameSpellingAbbreviation] = (
	select top 1 [NameSpellingAbbreviation] from [dbo].[Chat_ChatObject] c
	where Id=x.OwnerId
	),
[dbo].[Chat_SessionUnit].[OwnerObjectType] = (
	select top 1 [ObjectType] from [dbo].[Chat_ChatObject] c
	where Id=x.OwnerId
	),
---------------------------------------------------
[dbo].[Chat_SessionUnit].[DestinationName] = (
	select top 1 [Name] from [dbo].[Chat_ChatObject] c
	where Id=x.DestinationId
	),
[dbo].[Chat_SessionUnit].[DestinationNameSpellingAbbreviation] = (
	select top 1 [NameSpellingAbbreviation] from [dbo].[Chat_ChatObject] c
	where Id=x.DestinationId
	),
[dbo].[Chat_SessionUnit].[DestinationObjectType] = (
	select top 1 [ObjectType] from [dbo].[Chat_ChatObject] c
	where Id=x.DestinationId
	)
from [dbo].[Chat_SessionUnit] x
Where 1=1
--and [OwnerName] is null
and Id='719D7227-030A-326A-BCE9-3A0B0CF571FE'

declare @aa nvarchar(50) 

DECLARE  @keyword NVARCHAR(50)
set @keyword  = 'Õı¿º%'
select top 1000 * from [dbo].[Chat_SessionUnit] 
where 1=1
--and  Id='719D7227-030A-326A-BCE9-3A0B0CF571FE'
and ([OwnerName] like @keyword Or [OwnerNameSpellingAbbreviation] like @keyword)
ORDER by [LastMessageId] desc