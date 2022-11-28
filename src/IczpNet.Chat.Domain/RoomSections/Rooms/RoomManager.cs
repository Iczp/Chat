using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.RoomSections.Rooms
{
    public class RoomManager : DomainService, IRoomManager
    {

        //public virtual IdentityUserManager IdentityUserManager { get; }

        public Task<Room> CreateRoomAsync(Room room)
        {
            throw new System.NotImplementedException();
        }
    }
}
