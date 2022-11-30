using IczpNet.AbpTrees;
using IczpNet.Chat.SquareSections.SquareCategorys.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SquareSections.SquareCategorys
{
    public interface ISquareCategoryAppService :
        ICrudAppService<
            SquareCategoryDetailDto,
            SquareCategoryDto,
            Guid,
            SquareCategoryGetListInput,
            SquareCategoryCreateInput,
            SquareCategoryUpdateInput>
        ,
        ITreeAppService<Guid, SquareCategoryInfo>
    {
    }
}
