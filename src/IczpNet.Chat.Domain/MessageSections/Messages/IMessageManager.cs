using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageManager
    {
        Task<Message> CreateMessageAsync(ChatObjectInfo sender, ChatObjectInfo receiver, Func<Message, Task<IMessageContentEntity>> func);
        Task<Message> CreateMessageAsync<TMessageInput>(TMessageInput input, Func<Message, Task<IMessageContentEntity>> func) where TMessageInput : class, IMessageInput;

        Task<MessageInfo<TContentInfo>> SendMessageAsync<TContentInfo>(MessageInput input, Func<Message, Task<IMessageContentEntity>> func);

        Task<List<Message>> ForwardMessageAsync(long sourceMessageId, long senderId, List<long> receiverIdList);
        Task<List<Message>> ForwardMessageAsync(Message source, ChatObjectInfo sender, List<long> receiverIdList);

        Task<Dictionary<string, long>> RollbackMessageAsync(Message message);

        
    }
}
