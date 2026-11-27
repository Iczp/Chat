using System;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.ChatObjectCategories.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.ChatObjectCategories;

public interface IChatObjectCategoryAdminAppService : IApplicationService
{
    Task<PagedResultDto<ChatObjectCategoryAdminDto>> GetListAsync(
        ChatObjectCategoryAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<ChatObjectCategoryAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ChatObjectCategoryAdminDto> CreateAsync(
        ChatObjectCategoryAdminCreateInput input,
        CancellationToken cancellationToken = default);

    Task<ChatObjectCategoryAdminDto> UpdateAsync(
        Guid id,
        ChatObjectCategoryAdminUpdateInput input,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
