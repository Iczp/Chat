using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;

public class SessionPermissionDefinitionTreeDto
{
    public virtual string Id { get; set; }

    public virtual bool IsGroup { get; set; }

    public virtual string Title { get; set; }

    public virtual string Value { get; set; }

    public virtual string Description { get; set; }

    //public virtual List<SessionPermissionDefinitionDto> Permissions { get; set; }

    public virtual List<SessionPermissionDefinitionTreeDto> Children { get; set; } = [];
}
