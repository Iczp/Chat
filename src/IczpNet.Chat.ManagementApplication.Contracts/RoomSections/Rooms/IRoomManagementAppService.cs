using IczpNet.Chat.Management.ChatObjects.Dtos;
using IczpNet.Chat.Management.RoomSections.Rooms.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionUnits.Dtos;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Management.RoomSections.Rooms
{
    public interface IRoomManagementAppService 
    {
        Task<ChatObjectDto> CreateAsync(RoomCreateInput input);

        Task<ChatObjectDto> CreateByAllUsersAsync(string name);

        Task<ChatObjectDto> CreateByAllUsersWithManyAsync(string name);

        Task<List<SessionUnitSenderInfo>> InviteAsync(InviteInput input);

        //Task<PagedResultDto<SessionUnitDto>> GetSameAsync(long sourceChatObjectId, long targetChatObjectId, int maxResultCount = 10, int skipCount = 0, string sorting = null);

        Task<int> GetSameCountAsync(long sourceChatObjectId, long targetChatObjectId);

        Task<ChatObjectDto> UpdateNameAsync(Guid sessionUnitId, string name);

        Task<ChatObjectDto> UpdatePortraitAsync(Guid sessionUnitId, string portrait);

        Task TransferCreatorAsync(Guid sessionUnitId, Guid targetSessionUnitId);

        /// <summary>
        /// 解散群
        /// </summary>
        /// <param name="sessionUnitId"></param>
        /// <param name="targetSessionUnitId"></param>
        /// <returns></returns>
        Task DissolveAsync(Guid sessionUnitId);
        
    }
}
