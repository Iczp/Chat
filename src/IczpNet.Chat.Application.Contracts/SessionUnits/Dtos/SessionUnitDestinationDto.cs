using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionUnits.Dtos
{
    public class SessionUnitDestinationDto
    {
        public virtual Guid Id { get; set; }

        ///// <summary>
        ///// 会话Id
        ///// </summary>
        //public virtual Guid SessionId { get; set; }

        /// <summary>
        /// 聊天对象Id
        /// </summary>
        public virtual long OwnerId { get; set; }

        /// <summary>
        /// 对象类型
        /// </summary>
        public virtual ChatObjectTypeEnums? OwnerObjectType { get; set; }

        /// <summary>
        /// 聊天对象类型:个人|群|服务号等
        /// </summary>
        public virtual ChatObjectTypeEnums? DestinationObjectType { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [StringLength(50)]
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// 会话内名称（如：群内名称）
        /// </summary>
        public virtual string MemberName { get; set; }

        /// <summary>
        /// 是否创建人(群主|版主等)
        /// </summary>
        public virtual bool IsCreator { get; set; }

        //public virtual long? InviterId { get; set; }

        //public virtual JoinWays? JoinWay { get; set; }

        //public virtual bool IsKilled { get; set; }

        //public virtual bool IsPublic { get; set; }

        //public virtual bool IsStatic { get; set; }

        public virtual IChatObject Owner { get; set; }

    }
}
