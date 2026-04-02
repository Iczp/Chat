using System;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitContactDto
{
    public virtual Guid Id { get; set; }
    public virtual string Name { get; set; }
    public virtual string Abbreviation { get; set; }
}
