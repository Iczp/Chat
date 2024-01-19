using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace IczpNet.Chat.SessionUnits.Dtos
{
    public class SessionUnitGetListDestinationInput : GetListInput
    {
        /// <summary>
        /// 是否被禁言
        /// </summary>
        public virtual bool? IsMuted { get; set; }

        /// <summary>
        /// 是否已删除的
        /// </summary>
        public virtual bool? IsKilled { get; set; }

        /// <summary>
        /// 是否公开的
        /// </summary>
        public virtual bool? IsPublic { get; set; } = true;

        /// <summary>
        /// 是否固定成员
        /// </summary>
        public virtual bool? IsStatic { get; set; }

        /// <summary>
        /// 所属聊天对象Id
        /// </summary>
        public virtual List<long> OwnerIdList { get; set; }

        /// <summary>
        /// 所属聊天对象类型
        /// </summary>
        [DefaultValue(null)]
        public virtual List<ChatObjectTypeEnums> OwnerTypeList { get; set; }

        /// <summary>
        /// 会话标签Id
        /// </summary>
        public virtual Guid? TagId { get; set; }

        /// <summary>
        /// 会话角色Id
        /// </summary>
        public virtual Guid? RoleId { get; set; }

        /// <summary>
        /// 加入方式
        /// </summary>
        public virtual JoinWays? JoinWay { get; set; }

        /// <summary>
        /// 邀请人Id
        /// </summary>
        public virtual Guid? InviterId { get; set; }


    }
}