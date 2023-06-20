using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Menus.Dtos;

public class MenuCreateInput : MenuUpdateInput
{

    /// <summary>
    /// 所属聊天对象
    /// </summary>
    [Required]
    public virtual long OwnerId { get; set; }
}
