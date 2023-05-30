using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using IczpNet.AbpCommons.PinYin;
using IczpNet.AbpCommons.Extensions;
using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using Volo.Abp.MultiTenancy;
using Volo.Abp;

namespace IczpNet.Chat.SessionSections.SessionUnitSettings
{

    public class SessionUnitSetting : BaseEntity, IHasCreationTime, IHasModificationTime, IIsStatic, IIsPublic//, ISoftDelete
    {
        public virtual Guid SessionUnitId { get; set; }

        [ForeignKey(nameof(SessionUnitId))]
        public virtual SessionUnit SessionUnit { get; set; }

        /// <summary>
        /// 已读的消息
        /// </summary>
        [Comment("已读的消息")]
        public virtual long? ReadedMessageId { get; set; }

        /// <summary>
        /// 已读的消息
        /// </summary>
        [Comment("已读的消息")]
        [ForeignKey(nameof(ReadedMessageId))]
        public virtual Message ReadedMessage { get; protected set; }

        /// <summary>
        /// 查看历史消息起始时间,为null时则不限
        /// </summary>
        [Comment("查看历史消息起始时间,为null时则不限")]
        public virtual DateTime? HistoryFristTime { get; protected set; }

        /// <summary>
        /// 查看历史消息截止时间,为null时则不限
        /// </summary>
        [Comment("查看历史消息截止时间,为null时则不限")]
        public virtual DateTime? HistoryLastTime { get; set; }

        [Comment("清除历史消息最后时间,为null时则不限")]
        public virtual DateTime? ClearTime { get; protected set; }

        [Comment("不显示消息会话(不退群,不删除消息)")]
        public virtual DateTime? RemoveTime { get; protected set; }

        [MaxLength(50)]
        [Comment("会话内的名称")]
        public virtual string MemberName { get; protected set; }


        [MaxLength(50)]
        [Comment("备注名称")]
        public virtual string Rename { get; protected set; }

        [MaxLength(300)]
        public virtual string MemberNameSpelling { get; protected set; }

        [MaxLength(50)]
        public virtual string MemberNameSpellingAbbreviation { get; protected set; }

        [MaxLength(300)]
        public virtual string RenameSpelling { get; protected set; }

        [MaxLength(50)]
        public virtual string RenameSpellingAbbreviation { get; protected set; }

        [MaxLength(500)]
        [Comment("备注其他")]
        public virtual string Remarks { get; protected set; }

        [Comment("是否保存通讯录")]
        public virtual bool IsCantacts { get; protected set; }

        [Comment("消息免打扰，默认为 false")]
        public virtual bool IsImmersed { get; protected set; } = false;

        [Comment("是否显示成员名称")]
        public virtual bool IsShowMemberName { get; protected set; }

        [Comment("是否显示已读")]
        public virtual bool IsShowReaded { get; protected set; }

        [Comment("是否固定成员")]
        public virtual bool IsStatic { get; set; }

        [Comment("是否公有成员")]
        public virtual bool IsPublic { get; set; }

        [Comment("是否启用输入框")]
        public virtual bool IsInputEnabled { get; set; } = true;

        [Comment("是否可用")]
        public virtual bool IsEnabled { get; protected set; } = true;

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
        /// 是否创建者（群主等）
        /// </summary>
        [Comment("是否创建者（群主等）")]
        public virtual bool IsCreator { get; protected set; } = false;

        #region 邀请人 SessionUnit
        public virtual Guid? InviterId { get; set; }

        [ForeignKey(nameof(InviterId))]
        public virtual SessionUnit Inviter { get; set; }

        #endregion

        #region 删除人 SessionUnit

        /// <summary>
        /// 删除会话(退出群等)，但是不删除会话(用于查看历史消息)
        /// </summary>
        [Comment("删除会话(退出群等)，但是不删除会话(用于查看历史消息)")]
        public virtual bool IsKilled { get; set; }

        public virtual KillTypes? KillType { get; set; }

        [Comment("删除会话时间")]
        public virtual DateTime? KillTime { get; set; }

        public virtual Guid? KillerId { get; set; }

        [ForeignKey(nameof(KillerId))]
        public virtual SessionUnit Killer { get; set; }


        #endregion

        [Comment("创建时间")]
        public override DateTime CreationTime { get; protected set; }

        [Comment("修改时间")]
        public override DateTime? LastModificationTime { get; set; }

        //public virtual bool IsDeleted { get; protected set; }

        public override object[] GetKeys()
        {
            return new object[] { SessionUnitId };
        }

        internal virtual void SetRename(string rename)
        {
            Rename = rename;
            //RenameSpelling = rename.ConvertToPinyin().MaxLength(300);
            //RenameSpellingAbbreviation = rename.ConvertToPY().MaxLength(50);
        }

        internal virtual void SetMemberName(string memberName)
        {
            MemberName = memberName;
            //MemberNameSpelling = memberName.ConvertToPinyin().MaxLength(300);
            //MemberNameSpellingAbbreviation = memberName.ConvertToPY().MaxLength(50);
        }

        /// <summary>
        /// removeSession 删除消息会话,不退群
        /// </summary>
        /// <param name="removeTime"></param>
        internal virtual void Remove(DateTime removeTime) => RemoveTime = removeTime;

        //internal virtual void SetReadedMessageId(long lastMessageId, bool isForce = false)
        //{
        //    if (isForce || lastMessageId > ReadedMessageId.GetValueOrDefault())
        //    {
        //        ReadedMessageId = lastMessageId;
        //    }
        //}

        /// <summary>
        /// 退群，但不删除会话（用于查看历史I）
        /// </summary>
        /// <param name="removeTime"></param>
        internal virtual void Kill(DateTime killTime, KillTypes? killType = null, Guid? killerId = null)
        {
            IsKilled = true;
            KillTime = killTime;
            HistoryLastTime = killTime;
            KillType = killType;
            if (killerId != null)
            {
                KillerId = killerId;
            }
        }

        /// <summary>
        /// 清空消息，不退群 
        /// </summary>
        /// <param name="clearTime"></param>
        internal virtual void ClearMessage(DateTime? clearTime) => ClearTime = clearTime;


        internal virtual void SetImmersed(bool isImmersed) => IsImmersed = isImmersed;

        internal virtual void SetIsEnabled(bool v) => IsEnabled = v;

        internal virtual void SetIsCreator(bool v)
        {
            IsCreator = v;
            IsStatic = v;
        }
    }
}
