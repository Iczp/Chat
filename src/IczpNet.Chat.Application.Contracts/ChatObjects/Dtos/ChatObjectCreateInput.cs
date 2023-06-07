using IczpNet.Chat.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.ChatObjects.Dtos;

public class ChatObjectCreateInput : ChatObjectUpdateInput
{
    [MaxLength(20)]
    public override string Name { get; set; }

    [DefaultValue(nameof(ChatObjectTypeEnums.Personal))]
    public virtual string ChatObjectTypeId { get; set; }

    [DefaultValue(ChatObjectTypeEnums.Personal)]
    public virtual ChatObjectTypeEnums? ObjectType { get; set; }
}
