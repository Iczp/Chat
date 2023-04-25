using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Localization;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using IczpNet.AbpCommons.DataFilters;

namespace IczpNet.Chat.BaseAppServices;

public abstract class CrudChatAdminAppService<
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
    ICrudChatAdminAppService<
        TGetOutputDto,
        TGetListOutputDto,
        TKey,
        TGetListInput,
        TCreateInput,
        TUpdateInput>

    where TEntity : class, IEntity<TKey>
    where TGetOutputDto : IEntityDto<TKey>
    where TGetListOutputDto : IEntityDto<TKey>
    //where TCreateInput : IName
    //where TUpdateInput : IName
{
    protected ICurrentChatObject CurrentChatObject => LazyServiceProvider.LazyGetRequiredService<ICurrentChatObject>();
    protected CrudChatAdminAppService(IRepository<TEntity, TKey> repository) : base(repository)
    {
        LocalizationResource = typeof(ChatResource);
        ObjectMapperContext = typeof(ChatAdminApplicationModule);
    }

    protected virtual bool HasNameProperty(TEntity entity)
    {
        return entity.GetType().GetProperty(nameof(IName.Name)) != null;
    }

    protected virtual void TryToSetName(TEntity entity, string name)
    {
        if (entity is IName && HasNameProperty(entity))
        {
            if (!name.IsNullOrWhiteSpace())
            {
                return;
            }

            var methodInfo = entity.GetType().GetMethod(nameof(IName.Name));

            if (methodInfo == null || !methodInfo.IsPublic)
            {
                return;
            }

            methodInfo.Invoke(entity, new object[] { name });
        }
    }


}
