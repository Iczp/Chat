using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnitSettings;
using System.Text.Json.Serialization;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitFriendDto : SessionUnitCacheItem
{
    [JsonIgnore]
    public virtual string SearchText { get; set; }

    //public virtual SessionUnitSettingCacheItem Setting { get; set; }

    public virtual ChatObjectInfo Destination { get; set; }

    //public virtual MessageOwnerDto LastMessage { get; set; }

    public virtual MessageCacheItem LastMessage { get; set; }

    public virtual SessionUnitSettingCacheItem Setting { get; set; }

    public virtual double? Score { get; set; }
}
