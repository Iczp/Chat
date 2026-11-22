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

    [HttpPost]
    [Route("{id}")]
    public virtual Task<SessionAnnouncementDetailDto> UpdateAsync(Guid id, UpdateSessionAnnouncementInput input)
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
    [Route("{id}/delete/{sessionUnitId}")]
    public virtual Task DeleteAsync(Guid id, Guid sessionUnitId)
    {
        return AnnouncementAppService.DeleteAsync(id, sessionUnitId);
    }

    [HttpPost]
    [Route("{id}/confirm")]
    public virtual Task ConfirmReceiptAsync(Guid id, ConfirmSessionAnnouncementInput input)
    {
        return AnnouncementAppService.ConfirmReceiptAsync(id, input);
    }

    [HttpGet]
    [Route("{id}/receipts")]
    public virtual Task<PagedResultDto<SessionAnnouncementReceiptDto>> GetReceiptsAsync(Guid id, PagedAndSortedResultRequestDto input)
    {
        return AnnouncementAppService.GetReceiptsAsync(id, input);
    }
}
