using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.RoomSections.Rooms.Dtos;

public class RoomManagementSettingsDto
{
    public virtual Guid SessionId { get; set; }
    public virtual long RoomId { get; set; }
    public virtual string Name { get; set; }
    public virtual string Portrait { get; set; }
    public virtual string Description { get; set; }
    public virtual VerificationMethods VerificationMethod { get; set; }
    public virtual bool IsAllMuted { get; set; }
    public virtual RoomInvitePolicies InvitePolicy { get; set; }
    public virtual RoomJoinChannels AllowJoinChannels { get; set; }
    public virtual int PendingJoinRequestCount { get; set; }
    public virtual int PendingAnnouncementCount { get; set; }
    public virtual bool IsCreator { get; set; }
    public virtual bool IsAdmin { get; set; }
    public virtual bool CanEnterManagement { get; set; }
    public virtual bool CanManageSettings { get; set; }
    public virtual bool CanManageJoinRequests { get; set; }
    public virtual bool CanManageAnnouncements { get; set; }
    public virtual bool CanManageMembers { get; set; }
    public virtual bool CanManageOrgs { get; set; }
    public virtual bool CanManageRoles { get; set; }
    public virtual bool CanManagePermissions { get; set; }
    public virtual bool CanTransferOrDissolve { get; set; }
}
