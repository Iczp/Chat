using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitSettingDto
    {
        public virtual long? ReadedMessageId { get; set; }

        public virtual string MemberName { get; set; }

        public virtual string Rename { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public virtual string Remarks { get; set; }

        /// <summary>
        /// 是否保存通讯录(群)
        /// </summary>
        public virtual bool IsCantacts { get; set; }

        /// <summary>
        /// 消息免打扰，默认为 false
        /// </summary>
        public virtual bool IsImmersed { get; set; }

        /// <summary>
        /// 是否显示成员名称
        /// </summary>
        public virtual bool IsShowMemberName { get; set; }

        /// <summary>
        /// 是否显示已读
        /// </summary>
        public virtual bool IsShowReaded { get; set; }

        /// <summary>
        /// 聊天背景，默认为 null
        /// </summary>
        //public virtual string BackgroundImage { get; set; }

        /// <summary>
        /// 加入方式
        /// </summary>
        //public virtual JoinWays? JoinWay { get; set; }

        public virtual bool IsKilled { get; set; }

        public virtual bool IsInputEnabled { get; set; }

        public virtual bool IsEnabled { get; set; }

        /// <summary>
        /// 邀请人
        /// </summary>
        public virtual Guid? InviterId { get; set; }
    }
}
