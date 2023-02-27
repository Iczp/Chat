using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjectCategorys.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.ChatObjectCategorys
{
    public interface IChatObjectCategoryAppService :
        ICrudAppService<
            ChatObjectCategoryDetailDto,
            ChatObjectCategoryDto,
            Guid,
            ChatObjectCategoryGetListInput,
            ChatObjectCategoryCreateInput,
            ChatObjectCategoryUpdateInput>
        ,
        ITreeAppService<Guid, ChatObjectCategoryInfo>
    {
    }
}
