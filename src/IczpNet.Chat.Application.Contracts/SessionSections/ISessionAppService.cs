using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.SessionSections.OpenedRecordes.Dtos;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections
{
    public interface ISessionAppService
    {
        Task<PagedResultDto<ChatObjectDto>> GetFriendsAsync(Guid ownerId, bool? isCantacts, int maxResultCount = 10, int skipCount = 0, string sorting = null);

        Task<OpenedRecorderDto> SetOpenedAsync(OpenedRecorderInput input);

        Task<PagedResultDto<SessionDto>> GetSessionsAsync(Guid ownerId, int maxResultCount = 10, int skipCount = 0, string sorting = null);
    }
}
