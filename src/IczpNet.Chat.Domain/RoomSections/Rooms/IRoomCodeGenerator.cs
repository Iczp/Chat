using IczpNet.Chat.ChatObjects;
using System.Threading.Tasks;

namespace IczpNet.Chat.RoomSections.Rooms;

public interface IRoomCodeGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    string Make();
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<string> MakeAsync();
}
