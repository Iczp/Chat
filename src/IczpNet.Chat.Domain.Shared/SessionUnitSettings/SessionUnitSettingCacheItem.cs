using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionUnitSettings;

public class SessionUnitSettingCacheItem
{
    /// <summary>
    /// 会话单元Id
    /// </summary>
    public virtual Guid SessionUnitId { get; set; }

    ///// <summary>
    ///// 会话Id
    ///// </summary>
    //public virtual Guid? SessionId { get; set; }


    /// <summary>
    /// 已读的消息
    /// </summary>
    public virtual long? ReadedMessageId { get; set; }

    /// <summary>
    /// 最后发言的消息
    /// </summary>
    public virtual long? LastSendMessageId { get; set; }

    /// <summary>
    /// 最后发言时间
    /// </summary>
    public virtual DateTime? LastSendTime { get; set; }

    /// <summary>
    /// 查看历史消息起始时间,为null时则不限
    /// </summary>
    public virtual DateTime? HistoryFristTime { get; set; }

    /// <summary>
    /// 查看历史消息截止时间,为null时则不限
    /// </summary>
    public virtual DateTime? HistoryLastTime { get; set; }

    /// <summary>
    /// 清除历史消息最后时间,为null时则不限
    /// </summary>
    public virtual DateTime? ClearTime { get; set; }

    /// <summary>
    /// 不显示消息会话(不退群,不删除消息
    /// </summary>
    public virtual DateTime? RemoveTime { get; set; }

    /// <summary>
    /// 会话内的名称
    /// </summary>
    public virtual string MemberName { get; set; }

    /// <summary>
    /// 备注名称
    /// </summary>
    public virtual string Rename { get; set; }

    /// <summary>
    /// 会话内的名称拼音
    /// </summary>
    public virtual string MemberNameSpelling { get; set; }

    /// <summary>
    /// 会话内的名称拼音简写
    /// </summary>
    public virtual string MemberNameSpellingAbbreviation { get; set; }

    /// <summary>
    /// 备注拼音简写
    /// </summary>
    public virtual string RenameSpelling { get; set; }

    /// <summary>
    /// 备注拼音简写
    /// </summary>
    public virtual string RenameSpellingAbbreviation { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual string Remarks { get; set; }

    /// <summary>
    /// 是否保存通讯录
    /// </summary>
    public virtual bool IsContacts { get; set; }

    /// <summary>
    /// 消息免打扰，默认为 false
    /// </summary>
    public virtual bool IsImmersed { get; set; } = false;

    /// <summary>
    /// 是否显示成员名称
    /// </summary>
    public virtual bool IsShowMemberName { get; set; }

    /// <summary>
    /// 是否显示已读
    /// </summary>
    public virtual bool IsShowReaded { get; set; }

    /// <summary>
    /// 是否固定成员
    /// </summary>
    public virtual bool IsStatic { get; set; } = false;

    /// <summary>
    /// 是否公有成员
    /// </summary>
    public virtual bool IsPublic { get; set; } = true;

    /// <summary>
    /// 是否可见的
    /// </summary>
    public virtual bool IsVisible { get; set; } = true;

    /// <summary>
    /// 是否显示
    /// </summary>
    public virtual bool IsDisplay { get; set; } = true;

    /// <summary>
    /// 是否启用输入框
    /// </summary>
    public virtual bool IsInputEnabled { get; set; } = true;

    /// <summary>
    /// 是否隐藏角标
    /// </summary>
    public virtual bool IsHideBadge { get; set; } = false;

    /// <summary>
    /// 是否可用
    /// </summary>
    public virtual bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 聊天背景，默认为 null
    /// </summary>
    public virtual string BackgroundImage { get; set; }

    /// <summary>
    /// 加入方式
    /// </summary>
    public virtual JoinWays? JoinWay { get; set; }

    /// <summary>
    /// 是否创建者（群主等）
    /// </summary>
    public virtual bool IsCreator { get; set; } = false;

    /// <summary>
    /// 禁言过期时间
    /// </summary>
    public virtual DateTime? MuteExpireTime { get; set; }

    /// <summary>
    /// 邀请人
    /// </summary>
    public virtual Guid? InviterId { get; set; }

    /// <summary>
    /// 删除渠道
    /// </summary>
    public virtual KillTypes? KillType { get; set; }

    /// <summary>
    /// 删除会话时间
    /// </summary>
    public virtual DateTime? KillTime { get; set; }

    /// <summary>
    /// 删除人Id
    /// </summary>
    public virtual Guid? KillerId { get; set; }
}
