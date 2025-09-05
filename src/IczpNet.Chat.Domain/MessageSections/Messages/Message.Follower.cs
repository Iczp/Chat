using Castle.Core.Internal;
using IczpNet.Chat.MessageSections.MessageFollowers;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class Message
{

    /// <summary>
    /// 发送人会话单元Id
    /// </summary>
    [Comment("发送人会话单元Id")]
    public virtual Guid? SenderSessionUnitId { get; protected set; }

    /// <summary>
    /// sender session unit
    /// </summary>
    [ForeignKey(nameof(SenderSessionUnitId))]
    public virtual SessionUnit SenderSessionUnit { get; protected set; }

    /// <summary>
    /// 发送人的粉丝(关注发送人的)
    /// </summary>
    [Comment("发送人的关注者(粉丝)")]
    [StringLength(5000)]
    public virtual string SenderFollowerIds { get; protected set; }

    /// <summary>
    /// 关注(粉丝)数量
    /// </summary>
    [Comment("关注数量")]
    public virtual int FollowerCount { get; protected set; }

    /// <summary>
    /// 关注消息
    /// </summary>
    [InverseProperty(nameof(MessageFollower.Message))]
    public virtual IList<MessageFollower> MessageFollowerList { get; protected set; } = [];

    /// <summary>
    /// 设置发送人的粉丝
    /// </summary>
    /// <param name="followerSessionUnitIdList"></param>
    public virtual void SetFollowerIds(List<Guid> followerSessionUnitIdList)
    {
        FollowerCount = followerSessionUnitIdList.Count;

        MessageFollowerList = followerSessionUnitIdList.Select(x => new MessageFollower(Id, x)).ToList();

        SenderFollowerIds = followerSessionUnitIdList.JoinAsString(",");

        var maxLength = SenderFollowerIds.GetType().GetAttribute<StringLengthAttribute>()?.MaximumLength;
        if (maxLength != null && SenderFollowerIds.Length > maxLength.Value)
        {
            SenderFollowerIds = SenderFollowerIds.Substring(0, maxLength.Value);
        }
    }
}
