using IczpNet.Chat.Bases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.DeletedRecorders;

public interface IDeletedRecorderManager : IRecorderManager<DeletedRecorder>
{
    Task<List<long>> GetDeletedMessageIdListAsync(Guid sessionUnitId);

    Task RemoveCacheAsync(Guid sessionUnitId);
}
