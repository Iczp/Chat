using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using System;
using Volo.Abp.Domain.Repositories;

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
        public MessageAppService(IRepository<Message, Guid> repository) : base(repository)
        {
        }
    }
}
