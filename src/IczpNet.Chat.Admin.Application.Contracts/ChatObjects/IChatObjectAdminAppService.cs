using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.ChatObjects.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.ChatObjects;

public interface IChatObjectAdminAppService : IApplicationService
{
    Task<PagedResultDto<ChatObjectAdminListItemDto>> GetListAsync(
        ChatObjectAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<ChatObjectAdminDetailDto> GetAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<ChatObjectAdminDetailDto> UpdateAsync(
        long id,
        ChatObjectAdminUpdateInput input,
        CancellationToken cancellationToken = default);

    Task<ChatObjectAdminDetailDto> CreateAsync(
        ChatObjectAdminCreateInput input,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<ChatObjectAdminDetailDto> SetVerificationMethodAsync(
        long id,
        ChatObjectAdminSetVerificationMethodInput input,
        CancellationToken cancellationToken = default);

    Task<ChatObjectAdminDetailDto> SetIsEnabledAsync(
        long id,
        ChatObjectAdminSetIsEnabledInput input,
        CancellationToken cancellationToken = default);
}
