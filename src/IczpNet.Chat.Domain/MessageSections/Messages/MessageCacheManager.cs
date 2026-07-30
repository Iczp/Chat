using IczpNet.Chat.RedisServices;
using StackExchange.Redis;
using System;

namespace IczpNet.Chat.MessageSections.Messages;

public class MessageCacheManager : RedisService, IMessageCacheManager
{

    protected string MessagesPrefix => $"{Options.Value.KeyPrefix}Messages:";

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private RedisKey SessionLastMessageSetKey() => $"{MessagesPrefix}SessionLastMessage";

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private RedisKey SessionMessageSetKey(Guid sessionId) => $"{MessagesPrefix}SessionMessage:{sessionId}";
}
