using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.MessageSections.Counters;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.Scopeds;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionUnits;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class Message
{
    /// <summary>
    /// 找开记录
    /// </summary>
    [InverseProperty(nameof(OpenedRecorder.Message))]
    public virtual IList<OpenedRecorder> OpenedRecorderList { get; set; }

    /// <summary>
    /// 已读记录
    /// </summary>
    [InverseProperty(nameof(ReadedRecorder.Message))]
    public virtual IList<ReadedRecorder> ReadedRecorderList { get; set; } = [];

    /// <summary>
    /// 收藏记录
    /// </summary>
    [InverseProperty(nameof(FavoritedRecorder.Message))]
    public virtual IList<FavoritedRecorder> FavoriteList { get; set; }

    /// <summary>
    /// 指定范围消息
    /// </summary>
    [InverseProperty(nameof(Scoped.Message))]
    public virtual IList<Scoped> ScopedList { get; set; }

    /// <summary>
    /// 会话
    /// </summary>
    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; protected set; }

    /// <summary>
    /// sender session unit
    /// </summary>
    [ForeignKey(nameof(SenderSessionUnitId))]
    public virtual SessionUnit SenderSessionUnit { get; protected set; }

    /// <summary>
    /// receiver session unit
    /// </summary>
    [ForeignKey(nameof(ReceiverSessionUnitId))]
    public virtual SessionUnit ReceiverSessionUnit { get; protected set; }

    /// <summary>
    /// 会话列表
    /// </summary>
    [InverseProperty(nameof(SessionSections.Sessions.Session.LastMessage))]
    public virtual IList<Session> SessionList { get; set; }

    /// <summary>
    /// last message list
    /// </summary>
    [InverseProperty(nameof(SessionUnit.LastMessage))]
    public virtual List<SessionUnit> LastMessageSessionUnitList { get; protected set; }

    /// <summary>
    /// last message list
    /// </summary>
    [InverseProperty(nameof(SessionUnitSetting.ReadedMessage))]
    public virtual List<SessionUnitSetting> ReadedMessageSessionUnitList { get; protected set; }

    /// <summary>
    /// 已读计数器
    /// </summary>
    public virtual ReadedCounter ReadedCounter { get; protected set; } = new ReadedCounter();

    /// <summary>
    /// 打开计数器
    /// </summary>
    public virtual OpenedCounter OpenedCounter { get; protected set; } = new OpenedCounter();

    /// <summary>
    /// 收藏计数器
    /// </summary>
    public virtual FavoritedCounter FavoritedCounter { get; protected set; } = new FavoritedCounter();

    /// <summary>
    /// 设置会话数量
    /// </summary>
    internal virtual void SetSessionUnitCount(int sessionUnitCount)
    {
        SessionUnitCount = sessionUnitCount;
    }
}
