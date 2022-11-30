using IczpNet.AbpCommons;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.BaseAppServices;

public abstract class CrudChatAppService<
    TEntity,
    TGetOutputDto,
    TGetListOutputDto,
    TKey,
    TGetListInput,
    TCreateInput,
    TUpdateInput>
    :
    CrudCommonAppService<
        TEntity,
        TGetOutputDto,
        TGetListOutputDto,
        TKey,
        TGetListInput,
        TCreateInput,
        TUpdateInput>
    ,
    ICrudChatAppService<
        TGetOutputDto,
        TGetListOutputDto,
        TKey,
        TGetListInput,
        TCreateInput,
        TUpdateInput>

    where TEntity : BaseEntity<TKey>, IEntity<TKey>
    where TGetOutputDto : IEntityDto<TKey>
    where TGetListOutputDto : IEntityDto<TKey>
    //where TCreateInput : IName
    //where TUpdateInput : IName
{
    protected CrudChatAppService(IRepository<TEntity, TKey> repository) : base(repository)
    {
        LocalizationResource = typeof(ChatResource);
        ObjectMapperContext = typeof(ChatApplicationModule);
    }

    [HttpGet]
    public override Task<TGetOutputDto> GetAsync(TKey id)
    {
        return base.GetAsync(id);
    }

    [RemoteService(false)]
    public override Task<TGetOutputDto> CreateAsync(TCreateInput input)
    {
        return base.CreateAsync(input);
    }

    [RemoteService(false)]
    public override Task<TGetOutputDto> UpdateAsync(TKey id, TUpdateInput input)
    {
        return base.UpdateAsync(id, input);
    }

    [RemoteService(false)]
    public override Task DeleteAsync(TKey id)
    {
        return base.DeleteAsync(id);
    }

    [RemoteService(false)]
    public override Task DeleteManyAsync(List<TKey> idList)
    {
        return base.DeleteManyAsync(idList);
    }
}
