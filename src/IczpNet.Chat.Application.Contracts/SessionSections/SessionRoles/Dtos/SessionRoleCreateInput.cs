using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.SessionRoles.Dtos;

public class SessionRoleCreateInput : SessionRoleCreateBySessionUnitInput, ISessionId
{
    [Required]
    public virtual Guid? SessionId { get; set; }
}
