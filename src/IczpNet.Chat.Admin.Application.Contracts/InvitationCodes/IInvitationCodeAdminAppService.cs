using System;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.InvitationCodes.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.InvitationCodes;

public interface IInvitationCodeAdminAppService : IApplicationService
{
    Task<PagedResultDto<InvitationCodeAdminListItemDto>> GetListAsync(
        InvitationCodeAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<InvitationCodeAdminDetailDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<InvitationCodeAdminDetailDto> CreateAsync(
        InvitationCodeAdminCreateInput input,
        CancellationToken cancellationToken = default);

    Task<InvitationCodeAdminDetailDto> UpdateAsync(
        Guid id,
        InvitationCodeAdminUpdateInput input,
        CancellationToken cancellationToken = default);

    Task<InvitationCodeAdminDetailDto> SetIsEnabledAsync(
        Guid id,
        InvitationCodeAdminSetIsEnabledInput input,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
