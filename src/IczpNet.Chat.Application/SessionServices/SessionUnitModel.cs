using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionServices
{
    public class SessionUnitModel
    {
        public virtual Guid Id { get; set; }

        public virtual Guid? SessionId { get; set; }

        public virtual SessionUnitSettingDto Setting { get; set; }

        public virtual long OwnerId { get; set; }

        public virtual string MemberName { get; set; }

        public virtual string MemberNameSpellingAbbreviation { get; set; }

        public virtual string Rename { get; set; }

        public virtual string RenameSpellingAbbreviation { get; set; }

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
        /// 特别关注 for destination
        /// </summary>
        public virtual bool IsImportant { get; set; }

        /// <summary>
        /// 聊天背景，默认为 null
        /// </summary>
        [StringLength(500)]
        public virtual string BackgroundImage { get; set; }

        /// <summary>
        /// 加入方式
        /// </summary>
        public virtual JoinWays? JoinWay { get; set; }

        /// <summary>
        /// 邀请人
        /// </summary>
        public virtual long? InviterId { get; set; }

        public virtual bool IsKilled { get; set; }

        public virtual bool IsCreator { get; set; }

        public virtual bool IsInputEnabled { get; set; }

        public virtual bool IsEnabled { get; set; }

        public virtual ChatObject Destination { get; set; }

        public virtual Message LastMessage { get; set; }

        public virtual long? LastMessageId { get; set; }

        public virtual int Badge { get; set; }

        public virtual long ReadedMessageId { get; set; }

        public virtual int? PublicBadge { get; set; }

        public virtual int? PrivateBadge { get; set; }

        public virtual int? RemindAllCount { get; set; }

        public virtual int? RemindMeCount { get; set; }

        public virtual int? FollowingCount { get; set; }

        public virtual double Sorting { get; set; }
    }
}
