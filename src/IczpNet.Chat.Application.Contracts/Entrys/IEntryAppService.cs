using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.EntryValues.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.Entrys
{
    public interface IEntryAppService
    {
        Task<ChatObjectDetailDto> SetForChatObjectAsync(long ownerId, Dictionary<Guid, List<EntryValueInput>> input);

        Task<SessionUnitDestinationDetailDto> SetForSessionUnitAsync(Guid sessionUnitId, Dictionary<Guid, List<EntryValueInput>> input);
    }
}
