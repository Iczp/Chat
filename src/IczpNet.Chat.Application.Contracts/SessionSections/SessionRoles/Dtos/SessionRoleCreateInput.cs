using IczpNet.Chat.Sessions;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.SessionRoles.Dtos;

public class SessionRoleCreateInput : SessionRoleCreateBySessionUnitInput, ISessionId
{
    public virtual Guid? SessionId { get; set; }
}
