﻿using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Messages;
using IczpNet.Chat.Messages.Dtos;
using System;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
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