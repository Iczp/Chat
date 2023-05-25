using IczpNet.Chat.SessionSections.SessionUnitCounters;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageManager
    {
        //Task<Message> CreateMessageAsync(IChatObject sender, IChatObject receiver, Func<Message, Task<IContentEntity>> func);

        //Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, Func<Message, Task<IContentEntity>> func) where TMessageInput : class, IMessageInput;

        //Task<MessageInfo<TContentInfo>> SendMessageAsync<TContentInfo>(MessageInput input, Func<Message, Task<IContentEntity>> func);

        Task<Message> CreateMessageBySessionUnitAsync(SessionUnit sessionUnit, Func<Message, SessionUnitCounterArgs, Task> action, SessionUnit receiverSessionUnit = null);

        Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContent>(SessionUnit senderSessionUnit, MessageSendInput<TContentInfo> input, SessionUnit receiverSessionUnit = null)
            where TContentInfo :  IContentInfo
            where TContent :  IContentEntity;

        //Task<List<Message>> ForwardMessageAsync(long sourceMessageId, long senderId, List<long> receiverIdList);

        //Task<List<Message>> ForwardMessageAsync(Message source, IChatObject sender, List<long> receiverIdList);

        Task<List<Message>> ForwardMessageAsync(Guid currentSessionUnitId, long sourceMessageId, List<Guid> targetSessionUnitIdList);

        Task<Dictionary<string, long>> RollbackMessageAsync(Message message);


    }
}
