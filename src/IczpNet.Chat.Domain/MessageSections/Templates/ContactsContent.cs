using IczpNet.Chat.Attributes;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections.Templates;

[MessageTemplate(MessageTypes.Contacts)]
[ContentOuput(typeof(ContactsContentInfo))]
public class ContactsContent : MessageContentEntityBase, IChatOwner<long?>
{
    public override long GetSize() =>  3;

    public virtual long DestinationId { get;  set; }

    [ForeignKey(nameof(DestinationId))]
    public virtual ChatObject Destination { get; protected set; }

    /// <summary>
    /// 联系人名称
    /// </summary>
    //[Index]
    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(50)]
    public virtual string Code { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    [StringLength(300)]
    [MaxLength(300)]
    public virtual string Portrait { get; set; }

    public virtual ChatObjectTypeEnums? ObjectType { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    [StringLength(200)]
    public string Description { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(200)]
    public string Remark { get; set; }
}
