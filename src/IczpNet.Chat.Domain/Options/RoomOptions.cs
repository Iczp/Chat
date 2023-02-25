using IczpNet.Chat.Enums;
using System.Collections.Generic;

namespace IczpNet.Chat.Options
{
    public class RoomOptions
    {
        public List<ChatObjectTypeEnums> AllowJoinRoomObjectTypes { get; set; } = new List<ChatObjectTypeEnums>() {
            ChatObjectTypeEnums.Personal,
            ChatObjectTypeEnums.ShopKeeper,
            ChatObjectTypeEnums.ShopWaiter,
            ChatObjectTypeEnums.Customer,
            ChatObjectTypeEnums.Robot,
        };

        public List<ChatObjectTypeEnums> AllowCreateRoomObjectTypes { get; set; } = new List<ChatObjectTypeEnums>() {
            ChatObjectTypeEnums.Personal,
            ChatObjectTypeEnums.ShopKeeper,
            ChatObjectTypeEnums.Customer,
        };
    }
}
