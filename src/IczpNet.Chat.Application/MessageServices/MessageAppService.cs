using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
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

        public async Task<MessageDto> SendTextMessageAsync(MessageInput<TextContentInfo> input)
        {
            var message = await MessageManager.SendTextMessageAsync(input);
            return ObjectMapper.Map<Message, MessageDto>(message);
        }
    }
}
