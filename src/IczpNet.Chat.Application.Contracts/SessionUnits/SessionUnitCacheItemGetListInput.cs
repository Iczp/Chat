using IczpNet.AbpCommons.Dtos;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitCacheItemGetListInput : GetListInput
{
    [Required]
    public long OwnerId { get; set; }
}
