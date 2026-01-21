using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionBoxes;

public interface IBoxManager
{
    Task<IEnumerable<Box>> GetListByOwnerAsync(long ownerId);
}
