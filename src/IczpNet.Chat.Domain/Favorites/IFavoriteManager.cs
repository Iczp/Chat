using IczpNet.Chat.Bases;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Favorites
{
    public interface IFavoriteManager : IRecorderManager<Favorite>
    {
        Task DeleteAsync(Guid sessionUnitId, long messageId);
    }
}
