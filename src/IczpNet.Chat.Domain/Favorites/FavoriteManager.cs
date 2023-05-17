using IczpNet.Chat.Bases;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Favorites
{
    public class FavoriteManager : RecorderManager<Favorite>, IFavoriteManager
    {
        public FavoriteManager(IRepository<Favorite> repository) : base(repository)
        {


        }

        public virtual Task DeleteAsync(Guid sessionUnitId, long messageId)
        {
           return  Repository.DeleteAsync(x => x.SessionUnitId == sessionUnitId && x.MessageId == messageId);
        }

        protected override Favorite CreateEntity(SessionUnit sessionUnit, Message message, string deviceId)
        {
            return new Favorite(sessionUnit, message, deviceId);
        }
    }
}
