using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Sessions;
using System;

namespace IczpNet.Chat.SessionSections.SessionRoles.Dtos;

public class SessionRoleGetListInput : GetListInput, ISessionId
{
    /// <summary>
    /// 会话Id
    /// </summary>
    public virtual Guid? SessionId { get; set; }
}
