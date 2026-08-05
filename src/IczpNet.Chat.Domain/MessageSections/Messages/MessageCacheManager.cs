using IczpNet.Chat.RedisServices;
using StackExchange.Redis;
using System;

namespace IczpNet.Chat.MessageSections.Messages;

public class MessageCacheManager : RedisService, IMessageCacheManager
{

    protected string MessagesPrefix => $"{Options.Value.KeyPrefix}Messages:";


}
