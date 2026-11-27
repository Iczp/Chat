using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.ChatObjectTypes.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.ChatObjectTypes;

public interface IChatObjectTypeAdminAppService : IApplicationService
{
    Task<PagedResultDto<ChatObjectTypeAdminDto>> GetListAsync(
        ChatObjectTypeAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<ChatObjectTypeAdminDto> GetAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<ChatObjectTypeAdminDto> CreateAsync(
        ChatObjectTypeAdminCreateInput input,
        CancellationToken cancellationToken = default);

    Task<ChatObjectTypeAdminDto> UpdateAsync(
        string id,
        ChatObjectTypeAdminUpdateInput input,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);
}
