using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System.ComponentModel;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitGetListInput : BaseGetListInput
    {
        /// <summary>
        /// 所属聊天对象Id
        /// </summary>
        public virtual long? OwnerId { get; set; }

        /// <summary>
        /// 目标聊天对象Id
        /// </summary>
        public virtual long? DestinationId { get; set; }

        /// <summary>
        /// 聊天对象类型:个人|群|服务号等
        /// </summary>
        public virtual ChatObjectTypeEnums? DestinationObjectType { get; set; }

        /// <summary>
        /// 是否创建人(群主/场主等)
        /// </summary>
        public virtual bool? IsCreator { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        [DefaultValue(null)]
        public virtual bool? IsTopping { get; set; }

        /// <summary>
        /// 是否保存通讯录(群)
        /// </summary>
        public virtual bool? IsContacts { get; set; }

        /// <summary>
        /// 消息免打扰，默认为 false
        /// </summary>
        public virtual bool? IsImmersed { get; set; }

        

        //public virtual bool IsOrderByBadge { get; set; }
        /// <summary>
        /// 是否被移除会话
        /// </summary>
        public virtual bool? IsKilled { get; set; }

        /// <summary>
        /// 最小消息Id
        /// </summary>
        public virtual long? MinMessageId { get; set; }

        /// <summary>
        /// 最大消息Id
        /// </summary>
        public virtual long? MaxMessageId { get; set; }

        /// <summary>
        /// 是否有角标（新消息）
        /// </summary>
        [DefaultValue(null)]
        public virtual bool? IsBadge { get; set; }

        /// <summary>
        /// 是否有提醒
        /// </summary>
        [DefaultValue(null)]
        public virtual bool? IsRemind { get; set; }

        /// <summary>
        /// 是否有关注的人
        /// </summary>
        public virtual bool? IsFollowing { get; set; }


        //public virtual JoinWays? JoinWay { get; set; }

        //public virtual Guid? InviterId { get; set; }
    }
}