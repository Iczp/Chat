using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages;

public interface IMessageManager
{
    Task<Message> CreateMessageAsync(SessionUnit senderSessionUnit,
        Func<Message, Task<IContentEntity>> action,
        SessionUnit receiverSessionUnit = null,
        long? quoteMessageId = null,
        List<Guid> remindList = null);

    Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
        SessionUnit senderSessionUnit,
        MessageInput<TContentInfo> input,
        SessionUnit receiverSessionUnit = null)
        where TContentInfo : IContentInfo
        where TContentEntity : IContentEntity;

    Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
        SessionUnit senderSessionUnit,
        MessageInput input,
        TContentEntity contentEntity,
        SessionUnit receiverSessionUnit = null)
        where TContentInfo : IContentInfo
        where TContentEntity : IContentEntity;

    Task<List<Message>> ForwardAsync(Guid currentSessionUnitId, long sourceMessageId, List<Guid> targetSessionUnitIdList);

    Task<Dictionary<string, long>> RollbackAsync(Message message);

    Task<bool> IsRemindAsync(long messageId, Guid sessionUnitId);
}
