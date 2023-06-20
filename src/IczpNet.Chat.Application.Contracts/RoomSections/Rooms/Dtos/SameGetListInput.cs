using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.RoomSections.Rooms.Dtos
{
    public class SameGetListInput : GetListInput
    {

        /// <summary>
        /// 原聊天对象Id
        /// </summary>
        public virtual long SourceChatObjectId { get; set; }

        /// <summary>
        /// 目标对象Id
        /// </summary>
        public virtual long TargetChatObjectId { get; set; }
    }
}
