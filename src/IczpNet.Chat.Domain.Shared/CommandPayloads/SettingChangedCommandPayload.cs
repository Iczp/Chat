using IczpNet.Chat.Commands;
using IczpNet.Pusher.Commands;
using System;

namespace IczpNet.Chat.CommandPayloads;

[Command(CommandConsts.SettingChanged)]
[Serializable]
public class SettingChangedCommandPayload
{
    //public virtual string SessionUnitId { get; set; }
}
