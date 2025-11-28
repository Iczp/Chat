using IczpNet.Chat.Commands;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Pusher.Commands;
using System;

namespace IczpNet.Chat.MessageSections.Messages;

public class MessageWithQuoteInfo<T> : MessageInfo<T>
{
    public virtual MessageInfo QuoteMessage { get; set; }
}

public class MessageWithQuoteInfo : MessageInfo
{
    public virtual MessageInfo QuoteMessage { get; set; }
}

public class MessageAnyInfo : MessageInfo<object>
{

}

public class MessageInfo<T> : MessageInfo
{
    public virtual T Content { get; set; }
}

[Command(CommandConsts.Chat)]
public class MessageInfo : MessageCacheItem
{


    /// <summary>
    /// 发送人信息
    /// </summary>
    public virtual SessionUnitSenderInfo SenderSessionUnit { get; set; }
}
