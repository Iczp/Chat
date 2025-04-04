﻿using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Contacts.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Contacts;

/// <summary>
/// 通讯录
/// </summary>
public class ContactsAppService(
    ISessionUnitRepository repository) : ChatAppService, IContactsAppService
{
    protected override string GetListPolicyName { get; set; } = ChatPermissions.ContactPermission.GetList;
    protected override string GetPolicyName { get; set; } = ChatPermissions.ContactPermission.GetItem;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.ContactPermission.Delete;

    protected ISessionUnitRepository Repository { get; } = repository;

    /// <inheritdoc/>
    protected virtual async Task<IQueryable<SessionUnit>> CreateQueryAsync(ContactsGetListInput input)
    {
        return (await Repository.GetQueryableAsync())
            //.Where(x => x.Setting.IsContacts)
            .Where(x => x.OwnerId == input.OwnerId)
            .WhereIf(input.ObjectTypes.IsAny(), x => input.ObjectTypes.Contains((ChatObjectTypeEnums)x.DestinationObjectType))
            .WhereIf(input.DestinationObjectType.HasValue, x => x.DestinationObjectType == input.DestinationObjectType)
            .WhereIf(input.TagId.HasValue, x => x.SessionUnitContactTagList.Any(d => d.TagId == input.TagId))
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordDestinationSessionUnitSpecification(input.Keyword, await ChatObjectManager.SearchKeywordByCacheAsync(input.Keyword)))
            ;
    }

    /// <summary>
    /// 通讯录列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<ContactsDto>> GetListAsync(ContactsGetListInput input)
    {

        await CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(GetListPolicyName));

        var query = await CreateQueryAsync(input);

        return await GetPagedListAsync<SessionUnit, ContactsDto>(
            query,
            input,
            x => x.OrderByDescending(x => x.Sorting).ThenByDescending(x => x.LastMessageId),
            async entities =>
            {
                await Task.Yield();

                return entities;
            });
    }
}
