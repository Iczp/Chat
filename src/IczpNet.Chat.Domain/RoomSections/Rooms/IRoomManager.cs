using System.Threading.Tasks;

namespace IczpNet.Chat.RoomSections.Rooms
{
    public interface IRoomManager
    {
        Task<Room> CreateRoomAsync(Room room);
    }
}
