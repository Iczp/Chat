using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.MessageSections.Inputs;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageSections.Outputs;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.MessageServices
{
    public class MessageAppService
        : CrudChatAppService<
        Message,
        MessageDetailDto,
        MessageDto,
        Guid,
        MessageGetListInput,
        MessageCreateInput,
        MessageUpdateInput>,
        IMessageAppService
    {

        protected IMessageManager MessageManager { get; set; }

        public MessageAppService(
            IRepository<Message, Guid> repository,
            IMessageManager messageManager) : base(repository)
        {
            MessageManager = messageManager;
        }

        public virtual Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input)
        {
            return MessageManager.SendTextMessageAsync(input);
        }

        public Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input)
        {
            return MessageManager.SendCmdMessageAsync(input);
        }
    }
}
