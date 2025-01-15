using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionSections.SessionPermissionGroups.Dtos;

/// <summary>
/// SessionPermissionGroup GetListInput
/// </summary>
public class SessionPermissionGroupGetListInput : BaseTreeGetListInput<long>
{
    public long? Depth { get; set; }
}
