using IczpNet.Chat.MessageSections.Templates;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageAppService
    {

        Task<List<Guid>> ForwardMessageAsync(Guid sourceMessageId, Guid senderId, List<Guid> receiverIdList);

        Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input);

        Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input);
    }
}
