﻿using IczpNet.Chat.Enums.Dtos;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageAppService
    {
        Task<List<EnumDto>> GetDisabledForwardListAsync();

        Task<PagedResultDto<MessageOwnerDto>> GetListAsync(MessageGetListInput input);

        Task<MessageDto> GetItemAsync(MessageGetItemInput input);


    }
}
