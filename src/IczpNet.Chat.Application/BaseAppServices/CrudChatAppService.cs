using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.AspNetCore.Authorization;

namespace IczpNet.Chat.BaseAppServices;

[ApiExplorerSettings(GroupName = ChatRemoteServiceConsts.ModuleName)]
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

    where TEntity : class, IEntity<TKey>
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

    protected virtual async Task CheckPolicyAsync(string policyName, ChatObject owner)
    {

        if (string.IsNullOrEmpty(policyName))
        {
            return;
        }

        await AuthorizationService.CheckAsync(owner, policyName);
    }

    protected virtual async Task CheckPolicyAsync(string policyName, SessionUnit sessionUnit)
    {
        if (string.IsNullOrEmpty(policyName))
        {
            return;
        }

        await AuthorizationService.CheckAsync(sessionUnit, policyName);
    }
}
