using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitContactDto //: ContactValue
{
    public virtual Guid Id { get; set; }
    public long OwnerId { get; set; }
    public long DestinationId { get; set; }
    public virtual string Name { get; set; }
    public string Rename { get; set; }
    public string Mobile { get; set; }
    public virtual string Abbr { get; set; }
    public string NameSpelling { get; set; }
    public bool? IsFollowing { get; set; }
    public ChatObjectTypeEnums? ObjectType { get; set; }
    public string Portrait { get; set; }
    public string Thumbnail { get; set; }
}
