using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.ActionMenus.Dtos;

public class ActionMenuCreateInput : ActionMenuUpdateInput
{
    [Required]
    public virtual long OwnerId { get; set; }
}
