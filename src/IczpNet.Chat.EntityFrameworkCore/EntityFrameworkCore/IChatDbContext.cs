using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Messages;
using IczpNet.Chat.Robots;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.RoomSections.RoomRoles;
using IczpNet.Chat.RoomSections.RoomPermissionGrants;
using IczpNet.Chat.RoomSections.RoomPermissionDefines;
using IczpNet.Chat.RoomSections.RoomRoleRoomMembers;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.OfficialSections.Officials;
using IczpNet.Chat.OfficialSections.OfficialGroups;
using IczpNet.Chat.OfficialSections.OfficialGroupMembers;
using IczpNet.Chat.OfficialSections.OfficialExcludedMembers;
using IczpNet.Chat.RoomSections.RoomForbiddenMembers;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionSettings;

namespace IczpNet.Chat.EntityFrameworkCore;

[ConnectionStringName(ChatDbProperties.ConnectionStringName)]
public interface IChatDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */

    DbSet<Session> Session { get; }
    DbSet<SessionSetting> SessionSetting { get; }


    DbSet<SessionSetting> ChatSetting { get; }
    DbSet<ChatObject> ChatObject { get; }
    DbSet<Message> Message { get; }
    DbSet<Robot> Robot { get; }

    DbSet<CmdContent> CmdMessage { get; }
    DbSet<TextContent> TextMessage { get; }
    DbSet<HtmlContent> HtmlContent { get; }
    DbSet<ArticleContent> ArticleContent { get; }
    DbSet<LinkContent> LinkContent { get; }
    DbSet<ImageContent> ImageMessage { get; }
    DbSet<SoundContent> SoundContent { get; }
    DbSet<VideoContent> VideoContent { get; }
    DbSet<FileContent> FileContent { get; }
    DbSet<LocationContent> LocationContent { get; }
    DbSet<ContactsContent> ContactsContent { get; }
    DbSet<RedEnvelopeContent> RedEnvelopeContent { get; }
    DbSet<RedEnvelopeUnit> RedEnvelopeUnit { get; }
    DbSet<HistoryContent> HistoryContent { get; }
    DbSet<HistoryMessage> HistoryMessage { get; }


    DbSet<Official> Official { get; }
    DbSet<OfficialGroupMember> OfficialGroupMember { get; }
    DbSet<OfficialGroup> OfficialGroup { get; }
    DbSet<OfficalExcludedMember> OfficalExcludedMember { get; }

    DbSet<Room> Room { get; }
    DbSet<RoomMember> RoomMember { get; }
    DbSet<RoomRole> RoomRole { get; }
    DbSet<RoomRoleRoomMember> RoomRoleRoomMember { get; }
    DbSet<RoomPermissionDefine> RoomPermissionDefine { get; }
    DbSet<RoomPermissionGrant> RoomPermissionGrant { get; }
    DbSet<RoomForbiddenMember> RoomForbiddenMember { get; }

}
