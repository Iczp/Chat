using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.SessionSections.Sessions.Dtos
{
    public class SessionGetListInput : GetListInput
    {
        /// <summary>
        /// 聊天对象Id
        /// </summary>
        public virtual long? OwnerId { get; set; }
    }
}