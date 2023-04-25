using IczpNet.Chat.Management.BaseDtos;
using System;

namespace IczpNet.Chat.Management.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionPermissionGroup GetListInput
/// </summary>
public class SessionPermissionGroupGetListInput : BaseTreeGetListInput<long>
{
    public long? Depth { get; set; }
}
