

use [Chat_Module_v3]

--PublicBadge
update [dbo].[Chat_SessionUnit] 
set [dbo].[Chat_SessionUnit].PublicBadge = (

	select count(1) from [dbo].[Chat_Message] m
	where 1=1
	and x.Id!=m.SessionUnitId
	and x.SessionId=m.SessionId
	and (x.ReadedMessageId is null or m.Id > x.ReadedMessageId)
	and x.OwnerId!=m.SenderId
	and (x.HistoryFristTime is null or m.CreationTime> x.HistoryFristTime)
	and (x.HistoryLastTime is null or m.CreationTime> x.HistoryLastTime)
	and (x.ClearTime is null or m.CreationTime> x.ClearTime)
	)
from [dbo].[Chat_SessionUnit] x

