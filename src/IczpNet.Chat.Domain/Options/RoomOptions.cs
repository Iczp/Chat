using IczpNet.Chat.Enums;
using System.Collections.Generic;

namespace IczpNet.Chat.Options
{
    public class RoomOptions
    {
        public List<ChatObjectTypes> AllowJoinRoomObjectTypes { get; set; } = new List<ChatObjectTypes>() {
            ChatObjectTypes.Personal,
            ChatObjectTypes.ShopKeeper,
            ChatObjectTypes.ShopWaiter,
            ChatObjectTypes.Customer,
            ChatObjectTypes.Robot,
        };

        public List<ChatObjectTypes> AllowCreateRoomObjectTypes { get; set; } = new List<ChatObjectTypes>() {
            ChatObjectTypes.Personal,
            ChatObjectTypes.ShopKeeper,
            ChatObjectTypes.Customer,
        };
    }
}
