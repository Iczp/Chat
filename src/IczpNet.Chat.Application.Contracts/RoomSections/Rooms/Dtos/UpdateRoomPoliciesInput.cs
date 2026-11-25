using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.RoomSections.Rooms.Dtos;

public class UpdateRoomPoliciesInput
{
    [Required]
    public virtual Guid SessionUnitId { get; set; }

    public virtual VerificationMethods? VerificationMethod { get; set; }

    public virtual bool? IsAllMuted { get; set; }

    public virtual RoomInvitePolicies? InvitePolicy { get; set; }

    public virtual RoomJoinChannels? AllowJoinChannels { get; set; }

    [StringLength(500)]
    public virtual string Description { get; set; }
}
