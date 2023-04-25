using IczpNet.AbpTrees;
using IczpNet.Chat.Management.ChatObjectCategorys.Dtos;

using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.ChatObjectCategorys
{
    public interface IChatObjectCategoryManagementAppService :
        ICrudAppService<
            ChatObjectCategoryDetailDto,
            ChatObjectCategoryDto,
            Guid,
            ChatObjectCategoryGetListInput,
            ChatObjectCategoryCreateInput,
            ChatObjectCategoryUpdateInput>
        ,
        ITreeAppService<ChatObjectCategoryDetailDto,
            ChatObjectCategoryDto,
            Guid,
            ChatObjectCategoryGetListInput,
            ChatObjectCategoryCreateInput,
            ChatObjectCategoryUpdateInput>
    {
    }
}
