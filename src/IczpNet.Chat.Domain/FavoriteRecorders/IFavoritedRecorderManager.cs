using IczpNet.Chat.Bases;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.FavoriteRecorders
{
    public interface IFavoritedRecorderManager : IRecorderManager<FavoritedRecorder>
    {
        Task DeleteAsync(Guid sessionUnitId, long messageId);
        Task<int> GetCountByOwnerIdAsync(long ownerId);
        Task<long> GetSizeByOwnerIdAsync(long ownerId);
    }
}
