using IczpNet.AbpCommons.Dtos;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.ConnectionPools.Dtos;

public class OnlineFriendsGetListInput : GetListInput
{
    /// <summary>
    /// 所有者Id
    /// </summary>
    [Required]
    public long OwnerId { get; set; }

    public long? FriendId { get; set; }

    public Guid? SessionId { get; set; }

    public Guid? SessionUnitId { get; set; }
}
