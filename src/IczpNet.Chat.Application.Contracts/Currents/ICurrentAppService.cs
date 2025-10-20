using IczpNet.Chat.Currents.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.Currents;

public interface ICurrentAppService
{
    Task<List<TabDto>> GetTabsAsync(long chatObjectId);
}
