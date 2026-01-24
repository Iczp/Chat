using System;
using Volo.Abp.ObjectExtending;

namespace IczpNet.Chat.SessionBoxes;

public class BoxInfo : ExtensibleObject //ExtensibleCreationAuditedEntityDto<Guid>
{
    public Guid Id { get; set; }

    public long? OwnerId { get; set; }

    public string Name { get; set; }

}
