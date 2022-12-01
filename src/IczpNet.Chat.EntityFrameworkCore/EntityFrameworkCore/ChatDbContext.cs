using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.OfficialSections.OfficialExcludedMembers;
using IczpNet.Chat.OfficialSections.OfficialGroupMembers;
using IczpNet.Chat.OfficialSections.OfficialGroups;
using IczpNet.Chat.OfficialSections.Officials;
using IczpNet.Chat.RobotSections.Robots;
using IczpNet.Chat.RoomSections.RoomForbiddenMembers;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.RoomSections.RoomPermissionDefines;
using IczpNet.Chat.RoomSections.RoomPermissionGrants;
using IczpNet.Chat.RoomSections.RoomRoleRoomMembers;
using IczpNet.Chat.RoomSections.RoomRoles;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.SessionSections.Friends;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionSettings;
using IczpNet.Chat.SquareSections.SquareCategorys;
using IczpNet.Chat.SquareSections.SquareMembers;
using IczpNet.Chat.SquareSections.Squares;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;

namespace IczpNet.Chat.EntityFrameworkCore;

[ConnectionStringName(ChatDbProperties.ConnectionStringName)]
public class ChatDbContext : AbpDbContext<ChatDbContext>, IChatDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public DbSet<Session> Session { get; }
    //public DbSet<SessionSetting> SessionSetting { get; }

    public DbSet<Friendship> Friendship { get; }

    public DbSet<ChatObject> ChatObject { get; }
    public DbSet<Message> Message { get; }
    
    public DbSet<Robot> Robot { get; }

    public DbSet<CmdContent> CmdMessage { get; }
    public DbSet<TextContent> TextMessage { get; }
    public DbSet<HtmlContent> HtmlContent { get; }
    public DbSet<ArticleContent> ArticleContent { get; }
    public DbSet<LinkContent> LinkContent { get; }
    public DbSet<ImageContent> ImageMessage { get; }
    public DbSet<SoundContent> SoundContent { get; }
    public DbSet<VideoContent> VideoContent { get; }
    public DbSet<FileContent> FileContent { get; }
    public DbSet<LocationContent> LocationContent { get; }
    public DbSet<ContactsContent> ContactsContent { get; }
    public DbSet<RedEnvelopeContent> RedEnvelopeContent { get; }
    public DbSet<RedEnvelopeUnit> RedEnvelopeUnit { get; }
    public DbSet<HistoryContent> HistoryContent { get; }
    public DbSet<HistoryMessage> HistoryMessage { get; }

    public DbSet<Official> Official { get; }
    public DbSet<OfficialGroupMember> OfficialGroupMember { get; }
    public DbSet<OfficialGroup> OfficialGroup { get; }
    public DbSet<OfficalExcludedMember> OfficalExcludedMember { get; }


    public DbSet<Room> Room { get; }
    public DbSet<RoomMember> RoomMember { get; }
    public DbSet<RoomRole> RoomRole { get; }
    public DbSet<RoomRoleRoomMember> RoomRoleRoomMember { get; }
    public DbSet<RoomPermissionDefine> RoomPermissionDefine { get; }
    public DbSet<RoomPermissionGrant> RoomPermissionGrant { get; }
    public DbSet<RoomForbiddenMember> RoomForbiddenMember { get; }

    public DbSet<Square> Square { get; }
    public DbSet<SquareCategory> SquareCategory { get; }
    public DbSet<SquareMember> SquareMember { get; }


    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureChat();
        builder.ConfigureIdentity();
        }
}
