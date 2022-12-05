using IczpNet.AbpCommons;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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
    CrudAbpCommonsAppService<
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
    protected ICurrentChatObject CurrentChatObject => LazyServiceProvider.LazyGetRequiredService<ICurrentChatObject>();
    protected CrudChatAppService(IRepository<TEntity, TKey> repository) : base(repository)
    {
        LocalizationResource = typeof(ChatResource);
        ObjectMapperContext = typeof(ChatApplicationModule);
    }

    [HttpGet]
    public override Task<TGetOutputDto> GetAsync(TKey id)
    {
        //CurrentUser.PhoneNumber = id;
        return base.GetAsync(id);
    }

    
}
