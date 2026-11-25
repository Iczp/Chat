using IczpNet.Chat.SessionSections.Announcements.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.Announcements;

public interface ISessionAnnouncementAppService : IApplicationService
{
    Task<SessionAnnouncementDetailDto> GetPinnedAsync(Guid sessionId, Guid? sessionUnitId = null);

    Task<List<SessionAnnouncementDetailDto>> GetPinnedListAsync(Guid sessionId, Guid? sessionUnitId = null);

    Task<SessionAnnouncementDetailDto> GetAsync(Guid id, Guid? sessionUnitId = null);

    Task<PagedResultDto<SessionAnnouncementDto>> GetListAsync(GetSessionAnnouncementListInput input);

    Task<PagedResultDto<SessionAnnouncementDto>> SearchAsync(SearchSessionAnnouncementInput input);

    Task<SessionAnnouncementPermissionsDto> GetPermissionsAsync(Guid sessionId, Guid? sessionUnitId = null);

    Task<SessionAnnouncementDetailDto> CreateAsync(CreateSessionAnnouncementInput input);

    Task<SessionAnnouncementDetailDto> UpdateAsync(Guid id, UpdateSessionAnnouncementInput input);

    Task<SessionAnnouncementDetailDto> SubmitReviewAsync(Guid id, SubmitAnnouncementReviewInput input);

    Task<SessionAnnouncementDetailDto> ReviewAsync(Guid id, ReviewSessionAnnouncementInput input);

    Task<List<SessionAnnouncementRevisionDto>> GetRevisionsAsync(Guid id, Guid? sessionUnitId = null);

    Task<SessionAnnouncementDetailDto> PublishAsync(Guid id, Guid sessionUnitId);

    Task RevokeAsync(Guid id, Guid sessionUnitId);

    Task<int> RemindUnconfirmedAsync(Guid id, RemindAnnouncementInput input);

    Task SetPinnedAsync(Guid id, SetPinnedInput input);

    Task DeleteAsync(Guid id, Guid sessionUnitId);

    Task ConfirmReceiptAsync(Guid id, ConfirmSessionAnnouncementInput input);

    Task RecordViewAsync(Guid id, RecordAnnouncementViewInput input);

    Task<SessionAnnouncementStatDto> GetStatAsync(Guid id, Guid sessionUnitId);

    Task<PagedResultDto<SessionAnnouncementMemberDto>> GetMembersAsync(Guid id, GetAnnouncementMemberListInput input);

    Task<PagedResultDto<SessionAnnouncementReceiptDto>> GetReceiptsAsync(Guid id, PagedAndSortedResultRequestDto input, Guid? sessionUnitId = null);
}
