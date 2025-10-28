using System;
using System.Collections.Generic;
using Volo.Abp.Data;

namespace IczpNet.Chat.Friends;

/// <summary>
/// 好友在线状态
/// </summary>
public class FriendStatus : IHasExtraProperties
{
    public Guid? UserId { get; set; }

    public DateTime? LastActiveTime { get; set; }

    public List<long> ChatObjectIdList { get; set; }

    public List<string> DeviceTypes { get; set; } = [];

    public ExtraPropertyDictionary ExtraProperties { get; set; } = new ExtraPropertyDictionary();
}
