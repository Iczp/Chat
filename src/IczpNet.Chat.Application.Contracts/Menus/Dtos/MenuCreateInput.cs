using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Menus.Dtos;

public class MenuCreateInput : MenuUpdateInput
{
    [Required]
    public virtual long OwnerId { get; set; }
}
