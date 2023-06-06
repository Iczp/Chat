using IczpNet.Chat.EntryNames.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace IczpNet.Chat.EntryNames
{
    public interface IEntryNameAppService
        : ICrudChatAppService<
            EntryNameDetailDto,
            EntryNameDto,
            Guid,
            EntryNameGetListInput,
            EntryNameCreateInput,
            EntryNameUpdateInput>
    {


    }
}
