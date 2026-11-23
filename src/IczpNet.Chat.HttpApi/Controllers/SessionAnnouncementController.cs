using IczpNet.Chat.SessionSections.Announcements;
using IczpNet.Chat.SessionSections.Announcements.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ChatRemoteServiceConsts.RemoteServiceName)]
[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/session-announcement")]
public class SessionAnnouncementController(ISessionAnnouncementAppService announcementAppService)
    : ChatController, ISessionAnnouncementAppService
{
    protected ISessionAnnouncementAppService AnnouncementAppService { get; } = announcementAppService;

    [HttpGet]
    [Route("pinned/{sessionId}")]
    public virtual Task<SessionAnnouncementDetailDto> GetPinnedAsync(Guid sessionId)
    {
        return AnnouncementAppService.GetPinnedAsync(sessionId);
    }

    [HttpGet]
    [Route("pinned")]
    public virtual Task<SessionAnnouncementDetailDto> GetPinnedByQueryAsync([FromQuery] Guid sessionId)
    {
        return AnnouncementAppService.GetPinnedAsync(sessionId);
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<SessionAnnouncementDetailDto> GetAsync(Guid id)
    {
        return AnnouncementAppService.GetAsync(id);
    }

    [HttpGet]
    public virtual Task<PagedResultDto<SessionAnnouncementDto>> GetListAsync(GetSessionAnnouncementListInput input)
    {
        return AnnouncementAppService.GetListAsync(input);
    }

    [HttpPost]
    public virtual Task<SessionAnnouncementDetailDto> CreateAsync(CreateSessionAnnouncementInput input)
    {
        return AnnouncementAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    public virtual Task<SessionAnnouncementDetailDto> UpdateAsync(Guid id, UpdateSessionAnnouncementInput input)
    {
        return AnnouncementAppService.UpdateAsync(id, input);
    }

    [HttpPost]
    [Route("{id}/update")]
    public virtual Task<SessionAnnouncementDetailDto> UpdateAliasAsync(Guid id, UpdateSessionAnnouncementInput input)
    {
        return AnnouncementAppService.UpdateAsync(id, input);
    }

    [HttpPost]
    [Route("{id}/set-pinned")]
    public virtual Task SetPinnedAsync(Guid id, SetPinnedInput input)
    {
        return AnnouncementAppService.SetPinnedAsync(id, input);
    }

    [HttpPost]
    [Route("{id}/pinned")]
    public virtual Task SetPinnedAliasAsync(Guid id, SetPinnedInput input)
    {
        return AnnouncementAppService.SetPinnedAsync(id, input);
    }

    [HttpPost]
    [Route("{id}/delete/{sessionUnitId}")]
    public virtual Task DeleteAsync(Guid id, Guid sessionUnitId)
    {
        return AnnouncementAppService.DeleteAsync(id, sessionUnitId);
    }

    [HttpDelete]
    [Route("{id}")]
    [HttpPost]
    [Route("{id}/delete")]
    public virtual Task DeleteDefaultAsync(Guid id, [FromQuery] Guid? sessionUnitId)
    {
        return AnnouncementAppService.DeleteAsync(id, sessionUnitId ?? Guid.Empty);
    }

    [HttpPost]
    [Route("{id}/confirm")]
    public virtual Task ConfirmReceiptAsync(Guid id, ConfirmSessionAnnouncementInput input)
    {
        return AnnouncementAppService.ConfirmReceiptAsync(id, input);
    }

    [HttpPost]
    [Route("{id}/record-view")]
    public virtual Task RecordViewAsync(Guid id, RecordAnnouncementViewInput input)
    {
        return AnnouncementAppService.RecordViewAsync(id, input);
    }

    [HttpPost]
    [Route("{id}/view")]
    public virtual Task RecordViewAliasAsync(Guid id, RecordAnnouncementViewInput input)
    {
        return AnnouncementAppService.RecordViewAsync(id, input);
    }

    [HttpGet]
    [Route("{id}/stat")]
    public virtual Task<SessionAnnouncementStatDto> GetStatAsync(Guid id, [FromQuery] Guid sessionUnitId)
    {
        return AnnouncementAppService.GetStatAsync(id, sessionUnitId);
    }

    [HttpGet]
    [Route("{id}/stat-summary")]
    public virtual Task<SessionAnnouncementStatDto> GetStatSummaryAsync(Guid id, [FromQuery] Guid? sessionUnitId)
    {
        return AnnouncementAppService.GetStatAsync(id, sessionUnitId ?? Guid.Empty);
    }

    [HttpGet]
    [Route("{id}/members")]
    public virtual Task<PagedResultDto<SessionAnnouncementMemberDto>> GetMembersAsync(Guid id, GetAnnouncementMemberListInput input)
    {
        return AnnouncementAppService.GetMembersAsync(id, input);
    }

    [HttpGet]
    [Route("{id}/receipts")]
    public virtual Task<PagedResultDto<SessionAnnouncementReceiptDto>> GetReceiptsAsync(Guid id, PagedAndSortedResultRequestDto input)
    {
        return AnnouncementAppService.GetReceiptsAsync(id, input);
    }
}
