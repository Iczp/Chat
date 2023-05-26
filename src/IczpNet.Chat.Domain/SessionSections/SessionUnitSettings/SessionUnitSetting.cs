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

namespace IczpNet.Chat.SessionSections.SessionUnitSettings
{

    public class SessionUnitSetting : Entity, IHasCreationTime, IHasModificationTime
    {
        public virtual Guid SessionUnitId { get; set; }

        [ForeignKey(nameof(SessionUnitId))]
        public virtual SessionUnit SessionUnit { get; set; }

        /// <summary>
        /// 已读的消息
        /// </summary>
        [Comment("已读的消息")] 
        public virtual long? ReadedMessageId { get; protected set; }

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
        public virtual DateTime? HistoryLastTime { get; protected set; }

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
        public virtual bool IsInputEnabled { get; protected set; } = true;

        [Comment("是否可用")]
        public virtual bool IsEnabled { get; protected set; } = true;

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
        public virtual bool IsKilled { get; protected set; }

        public virtual KillTypes? KillType { get; protected set; }

        [Comment("删除会话时间")]
        public virtual DateTime? KillTime { get; protected set; }

        public virtual Guid? KillerId { get; set; }

        [ForeignKey(nameof(KillerId))]
        public virtual SessionUnit Killer { get; set; }

       
        #endregion

        [Comment("创建时间")]
        public virtual DateTime CreationTime { get; set; }

        [Comment("修改时间")]
        public virtual DateTime? LastModificationTime { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { SessionUnitId };
        }
    }
}
