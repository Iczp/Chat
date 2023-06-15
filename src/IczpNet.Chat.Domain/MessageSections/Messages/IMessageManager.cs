using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageManager
    {
        Task<Message> CreateMessageAsync(SessionUnit senderSessionUnit,
            Func<Message, SessionUnitIncrementArgs, Task<IContentEntity>> action,
            SessionUnit receiverSessionUnit = null,
            long? quoteMessageId = null,
            List<Guid> remindList = null);

        Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
            SessionUnit senderSessionUnit,
            MessageSendInput<TContentInfo> input,
            SessionUnit receiverSessionUnit = null)
            where TContentInfo : IContentInfo
            where TContentEntity : IContentEntity;

        Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
            SessionUnit senderSessionUnit,
            MessageSendInput input,
            TContentEntity contentEntity,
            SessionUnit receiverSessionUnit = null)
            where TContentInfo : IContentInfo
            where TContentEntity : IContentEntity;

        Task<List<Message>> ForwardMessageAsync(Guid currentSessionUnitId, long sourceMessageId, List<Guid> targetSessionUnitIdList);

        Task<Dictionary<string, long>> RollbackMessageAsync(Message message);
    }
}
