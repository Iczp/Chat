using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Auditing;

namespace IczpNet.Chat.SessionUnitSettings;

/// <summary>
/// 会话设置
/// </summary>
[Index(nameof(MuteExpireTime))]
[Index(nameof(LastSendMessageId), AllDescending = true)]
[Index(nameof(LastSendTime), AllDescending = true)]
[Comment("会话设置")]
public class SessionUnitSetting : BaseEntity, IHasCreationTime, IHasModificationTime, IIsStatic, IIsPublic//, ISoftDelete
{
    /// <summary>
    /// 会话单元Id
    /// </summary>
    [Required]
    public virtual Guid SessionUnitId { get; set; }

    /// <summary>
    /// 会话单元
    /// </summary>
    [ForeignKey(nameof(SessionUnitId))]
    public virtual SessionUnit SessionUnit { get; set; }

    /// <summary>
    /// 会话Id
    /// </summary>
    public virtual Guid? SessionId { get; set; }

    /// <summary>
    /// 会话Id
    /// </summary>
    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; set; }

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
    /// 最后发言的消息
    /// </summary>
    [Comment("最后发言的消息")]
    public virtual long? LastSendMessageId { get; set; }

    /// <summary>
    /// 最后发言的消息
    /// </summary>
    [Comment("最后发言的消息")]
    [ForeignKey(nameof(LastSendMessageId))]
    public virtual Message LastSendMessage { get; protected set; }

    /// <summary>
    /// 最后发言时间
    /// </summary>
    [Comment("最后发言时间")]
    public virtual DateTime? LastSendTime { get; protected set; }

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

    /// <summary>
    /// 清除历史消息最后时间,为null时则不限
    /// </summary>
    [Comment("清除历史消息最后时间,为null时则不限")]
    public virtual DateTime? ClearTime { get; protected set; }

    /// <summary>
    /// 不显示消息会话(不退群,不删除消息
    /// </summary>
    [Comment("不显示消息会话(不退群,不删除消息)")]
    public virtual DateTime? RemoveTime { get; protected set; }

    /// <summary>
    /// 会话内的名称
    /// </summary>
    [MaxLength(50)]
    [Comment("会话内的名称")]
    public virtual string MemberName { get; protected set; }

    /// <summary>
    /// 备注名称
    /// </summary>
    [MaxLength(50)]
    [Comment("备注名称")]
    public virtual string Rename { get; protected set; }

    /// <summary>
    /// 会话内的名称拼音
    /// </summary>
    [MaxLength(300)]
    [Comment("会话内的名称拼音")]
    public virtual string MemberNameSpelling { get; protected set; }

    /// <summary>
    /// 会话内的名称拼音简写
    /// </summary>
    [MaxLength(50)]
    [Comment("会话内的名称拼音简写")]
    public virtual string MemberNameSpellingAbbreviation { get; protected set; }

    /// <summary>
    /// 备注拼音简写
    /// </summary>
    [MaxLength(300)]
    [Comment("备注拼音简写")]
    public virtual string RenameSpelling { get; protected set; }

    /// <summary>
    /// 备注拼音简写
    /// </summary>
    [MaxLength(50)]
    [Comment("备注拼音简写")]
    public virtual string RenameSpellingAbbreviation { get; protected set; }

    [MaxLength(500)]
    [Comment("备注其他")]
    public virtual string Remarks { get; protected set; }

    /// <summary>
    /// 是否保存通讯录
    /// </summary>
    [Comment("是否保存通讯录")]
    public virtual bool IsContacts { get; protected set; }

    /// <summary>
    /// 消息免打扰，默认为 false
    /// </summary>
    [Comment("消息免打扰，默认为 false")]
    public virtual bool IsImmersed { get; protected set; } = false;

    /// <summary>
    /// 是否显示成员名称
    /// </summary>
    [Comment("是否显示成员名称")]
    public virtual bool IsShowMemberName { get; protected set; }

    /// <summary>
    /// 是否显示已读
    /// </summary>
    [Comment("是否显示已读")]
    public virtual bool IsShowReaded { get; protected set; }

    /// <summary>
    /// 是否固定成员
    /// </summary>
    [Comment("是否固定成员")]
    public virtual bool IsStatic { get; set; } = false;

    /// <summary>
    /// 是否公有成员
    /// </summary>
    [Comment("是否公有成员")]
    public virtual bool IsPublic { get; set; } = true;

    /// <summary>
    /// 是否可见的
    /// </summary>
    [Comment("是否可见的")]
    public virtual bool IsVisible { get; set; } = true;

    /// <summary>
    /// 是否显示
    /// </summary>
    [Comment("是否显示")]
    public virtual bool IsDisplay { get; set; } = true;

    /// <summary>
    /// 是否启用输入框
    /// </summary>
    [Comment("是否启用输入框")]
    public virtual bool IsInputEnabled { get; set; } = true;

    /// <summary>
    /// 是否隐藏角标
    /// </summary>
    [Comment("是否隐藏角标")]
    public virtual bool IsHideBadge { get; set; } = false;

    /// <summary>
    /// 是否可用
    /// </summary>
    [Comment("是否可用")]
    public virtual bool IsEnabled { get; protected set; } = true;

    /// <summary>
    /// 聊天背景，默认为 null
    /// </summary>
    [StringLength(500)]
    [Comment("聊天背景，默认为 null")]
    public virtual string BackgroundImage { get; set; }

    /// <summary>
    /// 加入方式
    /// </summary>
    [Comment("加入方式")]
    public virtual JoinWays? JoinWay { get; set; }

    /// <summary>
    /// 是否创建者（群主等）
    /// </summary>
    [Comment("是否创建者（群主等）")]
    public virtual bool IsCreator { get; protected set; } = false;

    #region 邀请人 SessionUnit
    /// <summary>
    /// 邀请人Id
    /// </summary>
    [Comment("邀请人Id")]
    public virtual Guid? InviterId { get; set; }

    /// <summary>
    /// 邀请人
    /// </summary>
    [ForeignKey(nameof(InviterId))]
    [Comment("邀请人")]
    public virtual SessionUnit Inviter { get; set; }

    #endregion

    #region 删除人 SessionUnit

    /// <summary>
    /// 删除会话(退出群等)，但是不删除会话(用于查看历史消息)
    /// </summary>
    [Comment("删除会话(退出群等)，但是不删除会话(用于查看历史消息)")]
    public virtual bool IsKilled { get; set; }

    /// <summary>
    /// 删除渠道
    /// </summary>
    [Comment("删除渠道")]
    public virtual KillTypes? KillType { get; set; }

    /// <summary>
    /// 删除会话时间
    /// </summary>
    [Comment("删除会话时间")]
    public virtual DateTime? KillTime { get; set; }

    /// <summary>
    /// 删除人Id
    /// </summary>
    [Comment("删除人Id")]
    public virtual Guid? KillerId { get; set; }

    /// <summary>
    /// 删除人
    /// </summary>
    [ForeignKey(nameof(KillerId))]
    public virtual SessionUnit Killer { get; set; }

    #endregion

    /// <summary>
    /// 创建时间
    /// </summary>
    [Comment("创建时间")]
    public override DateTime CreationTime { get; protected set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    [Comment("修改时间")]
    public override DateTime? LastModificationTime { get; set; }

    /// <summary>
    /// 禁言过期时间，为空则不禁言
    /// </summary>
    [Comment("禁言过期时间，为空则不禁言")]
    public virtual DateTime? MuteExpireTime { get; protected set; }

    //public virtual bool IsDeleted { get; protected set; }

    public SessionUnitSetting()
    {

    }

    public SessionUnitSetting(Session session)
    {
        Session = session;
        SessionId = Session.Id;
    }

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
    /// <param name="killTime"></param>
    /// <param name="killType"></param>
    /// <param name="killerId"></param>
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

    internal virtual void SetIsContacts(bool isContacts) => IsContacts = isContacts;

    internal virtual void SetIsShowMemberName(bool isShowMemberName) => IsShowMemberName = isShowMemberName;

    internal virtual void SetIsEnabled(bool v) => IsEnabled = v;

    internal virtual void SetLastSendMessage(Message message)
    {
        LastSendMessageId = message.Id;
        LastSendMessage = message;
        LastSendTime = message.CreationTime;
    }
    internal virtual void SetLastSendMessage(long messageId, DateTime lastSendTime)
    {
        LastSendMessageId = messageId;
        LastSendTime = lastSendTime;
    }
    internal virtual void SetIsCreator(bool v)
    {
        IsCreator = v;
        IsStatic = v;
    }

    public virtual void SetMuteExpireTime(DateTime? muteExpireTime)
    {
        MuteExpireTime = muteExpireTime;
    }

}
