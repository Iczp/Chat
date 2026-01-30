using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.SessionUnitContactTags;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.ContactTags;

/// <summary>
/// 联系人标签
/// </summary>
[Description("联系人标签")]
public class ContactTag : BaseEntity<Guid>, IChatOwner<long?>
{
    [MaxLength(20)]
    public virtual string Name { get; set; }

    [MaxLength(1)]
    public virtual string Index { get; set; }

    public virtual long? OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; set; }

    public virtual IList<SessionUnitContactTag> SessionUnitContactTagList { get; set; }

    protected ContactTag() { }

    public ContactTag(Guid id, [NotNull] string name, [NotNull] long ownerId) : base(id)
    {
        Name = name;
        OwnerId = ownerId;
    }
}
