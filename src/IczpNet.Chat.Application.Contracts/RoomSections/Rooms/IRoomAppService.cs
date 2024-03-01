using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Enums.Dtos;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.RoomSections.Rooms
{
    public interface IRoomAppService
    {
        Task<ChatObjectDto> CreateAsync(RoomCreateInput input);

        Task<List<SessionUnitSenderInfo>> InviteAsync(InviteInput input);

        Task<PagedResultDto<SessionUnitDto>> GetSameAsync(SameGetListInput input);

        Task<int> GetSameCountAsync(long sourceChatObjectId, long targetChatObjectId);

        Task<ChatObjectDto> UpdateNameAsync(Guid sessionUnitId, string name);

        Task<ChatObjectDto> UpdatePortraitAsync(Guid sessionUnitId, string thumbnail, string portrait);

        Task TransferCreatorAsync(Guid sessionUnitId, Guid targetSessionUnitId);

        /// <summary>
        /// 解散群
        /// </summary>
        /// <param name="sessionUnitId"></param>
        /// <returns></returns>
        Task DissolveAsync(Guid sessionUnitId);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<PagedResultDto<EnumDto>> GetListAllowJoinTypes();
    }
}
