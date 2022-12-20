using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageManager
    {
        Task<Message> CreateMessageAsync(ChatObject sender, ChatObject receiver, Func<Message, Task<IMessageContentEntity>> func);
        Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, Func<Message, Task<IMessageContentEntity>> func) where TMessageInput : class, IMessageInput;

        Task<MessageInfo<TContentInfo>> SendMessageAsync<TContentInfo>(MessageInput input, Func<Message, Task<IMessageContentEntity>> func);

        Task<List<Message>> ForwardMessageAsync(Guid sourceMessageId, Guid senderId, List<Guid> receiverIdList);
        Task<List<Message>> ForwardMessageAsync(Message source, ChatObject sender, List<Guid> receiverIdList);

        Task<long> RollbackMessageAsync(Message message);

        
    }
}
