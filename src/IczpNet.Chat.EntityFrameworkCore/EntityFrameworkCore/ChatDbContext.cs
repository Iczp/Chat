using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSettings;
using IczpNet.Chat.Messages;
using IczpNet.Chat.Messages.Templates;
using IczpNet.Chat.Officials;
using IczpNet.Chat.Robots;
using IczpNet.Chat.Rooms;
using IczpNet.Chat.Sessions;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using IczpNet.Chat.OfficialExcludedMembers;
using IczpNet.Chat.OfficialGroupMembers;
using IczpNet.Chat.OfficialGroups;
using IczpNet.Chat.RoomMembers;
using IczpNet.Chat.RoomRoles;
using IczpNet.Chat.RoomRoleRoomMembers;
using IczpNet.Chat.RoomPermissionDefines;
using IczpNet.Chat.RoomPermissionGrants;
using IczpNet.Chat.RoomForbiddenMembers;

namespace IczpNet.Chat.EntityFrameworkCore;

[ConnectionStringName(ChatDbProperties.ConnectionStringName)]
public class ChatDbContext : AbpDbContext<ChatDbContext>, IChatDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public DbSet<Session> Session { get; }
    public DbSet<SessionSetting> SessionSetting { get; }

    public DbSet<SessionSetting> ChatSetting { get; }
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
    

    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureChat();
    }
}
