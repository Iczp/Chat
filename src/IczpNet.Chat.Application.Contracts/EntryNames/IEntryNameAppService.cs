using IczpNet.AbpTrees;
using IczpNet.Chat.EntryNames.Dtos;
using System;

namespace IczpNet.Chat.EntryNames
{
    public interface IEntryNameAppService
        : ITreeAppService<
            EntryNameDetailDto,
            EntryNameDto,
            Guid,
            EntryNameGetListInput,
            EntryNameCreateInput,
            EntryNameUpdateInput>
    {


    }
}
