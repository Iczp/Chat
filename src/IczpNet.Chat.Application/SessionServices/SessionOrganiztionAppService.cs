using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionOrganiztions;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionSections.Sessions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices;

/// <summary>
/// 会话内组织架构
/// </summary>
public class SessionOrganizationAppService
    : CrudTreeChatAppService<
        SessionOrganization,
        long,
        SessionOrganizationDetailDto,
        SessionOrganizationDto,
        SessionOrganizationGetListInput,
        SessionOrganizationCreateInput,
        SessionOrganizationUpdateInput,
        SessionOrganizationInfo>,
    ISessionOrganizationAppService
{
    //protected override string GetPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionOrganizationPermission.Default;
    //protected override string GetListPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionOrganizationPermission.Default;
    //protected override string CreatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionOrganizationPermission.Create;
    //protected override string UpdatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionOrganizationPermission.Update;
    //protected override string DeletePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionOrganizationPermission.Delete;

    protected IRepository<Session, Guid> SessionRepository { get; set; }
    public SessionOrganizationAppService(
        IRepository<SessionOrganization, long> repository,
        ISessionOrganizationManager sessionOrganizationManager,
        IRepository<Session, Guid> sessionRepository)
        : base(repository, sessionOrganizationManager)
    {
        SessionRepository = sessionRepository;
    }

    protected override async Task<IQueryable<SessionOrganization>> CreateFilteredQueryAsync(SessionOrganizationGetListInput input)
    {
        Assert.If(!input.IsEnabledParentId && input.ParentId.HasValue, "When [IsEnabledParentId]=false,then [ParentId] != null");

        return (await Repository.GetQueryableAsync())
            .WhereIf(input.DepthList.IsAny(), (x) => input.DepthList.Contains(x.Depth))
            .WhereIf(input.SessionId.HasValue, (x) => x.SessionId == input.SessionId)
            //.WhereIf(input.SessionUnitId.HasValue, (x) => x.session == input.SessionId)
            .WhereIf(input.IsEnabledParentId, (x) => x.ParentId == input.ParentId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword));
    }

    protected override async Task CheckGetListPolicyAsync(SessionOrganizationGetListInput input)
    {
        if (input.SessionUnitId.HasValue)
        {
            await CheckPolicyAsync(GetListPolicyName, input.SessionUnitId.Value);
        }

        await base.CheckGetListPolicyAsync(input);
    }

    protected override async Task<SessionOrganization> MapToEntityAsync(SessionOrganizationCreateInput createInput)
    {
        await Task.Yield();

        return new SessionOrganization(createInput.Name, createInput.SessionId.Value, createInput.ParentId);
    }

    /// <summary>
    /// 添加组织
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public override async Task<SessionOrganizationDetailDto> CreateAsync(SessionOrganizationCreateInput input)
    {
        Assert.If(!await SessionRepository.AnyAsync(x => x.Id == input.SessionId), $"No such entity of sessionId:{input.SessionId}");

        Assert.If(input.ParentId.HasValue && !await Repository.AnyAsync(x => x.Id == input.ParentId && x.SessionId == input.SessionId), $"No such entity of ParentId:{input.ParentId}");

        return await base.CreateAsync(input);
    }

    /// <summary>
    /// 修改组织
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public override async Task<SessionOrganizationDetailDto> UpdateAsync(long id, SessionOrganizationUpdateInput input)
    {
        if (input.ParentId.HasValue)
        {
            var perent = await Repository.GetAsync(input.ParentId.Value);

            var entity = await Repository.GetAsync(id);

            Assert.If(perent.SessionId != entity.SessionId, $"Parent session is different,ParentId:{input.ParentId}");
        }
        return await base.UpdateAsync(id, input);
    }
}
