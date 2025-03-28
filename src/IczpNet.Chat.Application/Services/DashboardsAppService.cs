﻿using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Dashboards;
using IczpNet.Chat.Dashboards.Dtos;
using IczpNet.Chat.DbTables;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.MessageReminders;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionRequests;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnitOrganizations;
using IczpNet.Chat.SessionSections.SessionUnitRoles;
using IczpNet.Chat.SessionSections.SessionUnitTags;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Services;

/// <summary>
/// 仪表板
/// </summary>
public class DashboardsAppService(
    IChatObjectRepository chatObjectRepository,
    IMessageRepository messageRepository,
    ISessionRepository sessionRepository,
    ISessionUnitRepository sessionUnitRepository,
    IRepository<ReadedRecorder> readedRecorderRepository,
    IRepository<OpenedRecorder> openedRecorderRepository,
    IRepository<Follow> followRepository,
    IRepository<SessionRequest, Guid> sessionRequestRepository,
    IRepository<SessionOrganization, long> sessionOrganizationRepository,
    IRepository<SessionRole, Guid> sessionRoleRepository,
    IRepository<SessionTag, Guid> sessionTagRepository,
    IRepository<SessionUnitTag> sessionUnitTagRepository,
    IRepository<SessionUnitRole> sessionUnitRoleRepository,
    IRepository<SessionUnitOrganization> sessionUnitOrganizationRepository,
    IRepository<MessageReminder> messageReminderRepository,
    IRepository<FavoritedRecorder> favoriteRepository,
    IDbTableRepository dbTableRepository) : ChatAppService, IDashboardsAppService
{
    protected IChatObjectRepository ChatObjectRepository { get; } = chatObjectRepository;
    protected ISessionRepository SessionRepository { get; } = sessionRepository;
    protected ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;
    protected IMessageRepository MessageRepository { get; } = messageRepository;
    protected IRepository<ReadedRecorder> ReadedRecorderRepository { get; } = readedRecorderRepository;
    protected IRepository<OpenedRecorder> OpenedRecorderRepository { get; } = openedRecorderRepository;
    protected IRepository<Follow> FollowRepository { get; } = followRepository;
    protected IRepository<SessionRequest, Guid> SessionRequestRepository { get; } = sessionRequestRepository;
    protected IRepository<SessionOrganization, long> SessionOrganizationRepository { get; } = sessionOrganizationRepository;
    protected IRepository<SessionRole, Guid> SessionRoleRepository { get; } = sessionRoleRepository;
    protected IRepository<SessionTag, Guid> SessionTagRepository { get; } = sessionTagRepository;
    protected IRepository<SessionUnitTag> SessionUnitTagRepository { get; } = sessionUnitTagRepository;
    protected IRepository<SessionUnitRole> SessionUnitRoleRepository { get; } = sessionUnitRoleRepository;
    protected IRepository<SessionUnitOrganization> SessionUnitOrganizationRepository { get; } = sessionUnitOrganizationRepository;
    protected IRepository<MessageReminder> MessageReminderRepository { get; } = messageReminderRepository;
    protected IRepository<FavoritedRecorder> FavoriteRepository { get; } = favoriteRepository;
    protected IDbTableRepository DbTableRepository { get; } = dbTableRepository;

    /// <summary>
    /// 获取各表的数据数量
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [RemoteService(false)]
    [UnitOfWork(true, IsolationLevel.ReadUncommitted)]
    public async Task<DashboardsDto> GetProfileAsync()
    {
        return new DashboardsDto()
        {
            Now = Clock.Now,
            ChatObjectCount = await ChatObjectRepository.GetCountAsync(),
            MessageCount = await MessageRepository.GetCountAsync(),
            SessionCount = await SessionRepository.GetCountAsync(),
            SessionUnitCount = await SessionUnitRepository.GetCountAsync(),
            ReadedRecorderCount = await ReadedRecorderRepository.GetCountAsync(),
            OpenedRecorderCount = await OpenedRecorderRepository.GetCountAsync(),
            FollowCount = await FollowRepository.GetCountAsync(),
            SessionRequestCount = await SessionRequestRepository.GetCountAsync(),
            SessionOrganizationCount = await SessionOrganizationRepository.GetCountAsync(),
            SessionRoleCount = await SessionRoleRepository.GetCountAsync(),
            SessionTagCount = await SessionTagRepository.GetCountAsync(),
            SessionUnitTagCount = await SessionUnitTagRepository.GetCountAsync(),
            SessionUnitRoleCount = await SessionUnitRoleRepository.GetCountAsync(),
            SessionUnitOrganizationCount = await SessionUnitOrganizationRepository.GetCountAsync(),
            MessageReminderCount = await MessageReminderRepository.GetCountAsync(),
            FavoriteCount = await FavoriteRepository.GetCountAsync(),
        };
    }

    //[HttpGet]
    //public virtual async Task<PagedResultDto<DbTableDto>> GetListTableRowAsync(GetListInput input)
    //{
    //    var allList = await (await DbTableRepository.GetQueryableAsync()).ToListAsync();

    //    var query = allList.AsQueryable()
    //        .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.TableName.Contains(input.Keyword));

    //    return await ToPagedListAsync<DbTable, DbTableDto>(query, input);
    //}

    [HttpGet]
    public virtual async Task<PagedResultDto<DbTableDto>> GetListDbTablesAsync(GetListInput input)
    {
        var query =  (await DbTableRepository.GetQueryableAsync())
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.TableName.Contains(input.Keyword));

        return await GetPagedListAsync<DbTable, DbTableDto>(query, input);
    }
}
