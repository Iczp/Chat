using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitIndexDto
{
    public virtual string Index { get; set; }
    public virtual int Count { get; set; }
    public virtual List<SessionUnitContactDto> List { get; set; }
}
