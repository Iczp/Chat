using IczpNet.Chat.SessionSections;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Management.SessionSections.SessionRoles.Dtos;

public class SessionRoleCreateInput : SessionRoleCreateBySessionUnitInput, ISessionId
{
    [Required]
    public virtual Guid? SessionId { get; set; }
}
