using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.Entries
{
    public interface IEntryAppService
    {
        Task<ChatObjectDetailDto> SetForChatObjectAsync(long ownerId, Dictionary<Guid, List<string>> entries);

        Task<SessionUnitDestinationDetailDto> SetForSessionUnitAsync(Guid sessionUnitId, Dictionary<Guid, List<string>> entries);
    }
}
