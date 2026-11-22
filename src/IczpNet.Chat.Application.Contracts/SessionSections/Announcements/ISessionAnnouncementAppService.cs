using IczpNet.Chat.SessionSections.Announcements.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.Announcements;

public interface ISessionAnnouncementAppService : IApplicationService
{
    Task<SessionAnnouncementDetailDto> GetPinnedAsync(Guid sessionId);

    Task<SessionAnnouncementDetailDto> GetAsync(Guid id);

    Task<PagedResultDto<SessionAnnouncementDto>> GetListAsync(GetSessionAnnouncementListInput input);

    Task<SessionAnnouncementDetailDto> CreateAsync(CreateSessionAnnouncementInput input);

    Task<SessionAnnouncementDetailDto> UpdateAsync(Guid id, UpdateSessionAnnouncementInput input);

    Task SetPinnedAsync(Guid id, SetPinnedInput input);

    Task DeleteAsync(Guid id, Guid sessionUnitId);

    Task ConfirmReceiptAsync(Guid id, ConfirmSessionAnnouncementInput input);

    Task RecordViewAsync(Guid id, RecordAnnouncementViewInput input);

    Task<SessionAnnouncementStatDto> GetStatAsync(Guid id, Guid sessionUnitId);

    Task<PagedResultDto<SessionAnnouncementMemberDto>> GetMembersAsync(Guid id, GetAnnouncementMemberListInput input);

    Task<PagedResultDto<SessionAnnouncementReceiptDto>> GetReceiptsAsync(Guid id, PagedAndSortedResultRequestDto input);
}
