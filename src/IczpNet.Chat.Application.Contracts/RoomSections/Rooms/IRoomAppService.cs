using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.RoomSections.Rooms
{
    public interface IRoomAppService : IApplicationService
    {
        Task<ChatObjectDto> CreateAsync(RoomCreateInput input);

        Task<ChatObjectDto> CreateByAllUsersAsync(string name);

        Task<List<SessionUnitSenderInfo>> InviteAsync(InviteInput input);
    }
}
