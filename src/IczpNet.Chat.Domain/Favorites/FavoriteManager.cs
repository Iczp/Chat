using IczpNet.Chat.Bases;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Favorites
{
    public class FavoriteManager : RecorderManager<Favorite>, IFavoriteManager
    {
        public FavoriteManager(IRepository<Favorite> repository) : base(repository)
        {


        }
        protected override Favorite CreateEntity(SessionUnit sessionUnit, Message message, string deviceId)
        {
            return new Favorite(sessionUnit, message, deviceId);
        }

        public virtual Task DeleteAsync(Guid sessionUnitId, long messageId)
        {
            return Repository.DeleteAsync(x => x.SessionUnitId == sessionUnitId && x.MessageId == messageId);
        }

        public async Task<long> GetSizeAsync(long ownerId)
        {
            return (await Repository.GetQueryableAsync())
                .Where(x => x.OwnerId == ownerId)
                .Sum(x => x.Size);
        }
    }
}
