using IczpNet.Chat.Articles;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatUsers;
using IczpNet.Chat.Connections;
using IczpNet.Chat.MessageSections.MessageContents;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.OfficialSections.OfficialExcludedMembers;
using IczpNet.Chat.OfficialSections.OfficialGroupMembers;
using IczpNet.Chat.OfficialSections.OfficialGroups;
using IczpNet.Chat.OfficialSections.OfficialMembers;
using IczpNet.Chat.OfficialSections.OfficialMemberTagUnits;
using IczpNet.Chat.OfficialSections.Officials;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.RobotSections.Robots;
using IczpNet.Chat.RoomSections.RoomForbiddenMembers;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.RoomSections.RoomPermissionDefines;
using IczpNet.Chat.RoomSections.RoomPermissionGrants;
using IczpNet.Chat.RoomSections.RoomRoleRoomMembers;
using IczpNet.Chat.RoomSections.RoomRoles;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.SessionSections.FriendshipRequests;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.FriendshipTagUnits;
using IczpNet.Chat.SessionSections.MessageReminders;
using IczpNet.Chat.SessionSections.OpenedRecorders;
using IczpNet.Chat.SessionSections.ReadedRecorders;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SquareSections.SquareCategorys;
using IczpNet.Chat.SquareSections.SquareMembers;
using IczpNet.Chat.SquareSections.Squares;
using IczpNet.Chat.Subscriptions;
using IczpNet.Chat.Wallets;
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
    public DbSet<ChatObject> ChatObject { get; }
    public DbSet<ChatUser> ChatUser { get; }


    public DbSet<Article> Article { get; }
    public DbSet<ArticleMessage> ArticleMessage { get; }

    public DbSet<Session> Session { get; }
    public DbSet<SessionUnit> SessionUnit { get; set; }
    public DbSet<SessionTag> SessionTag { get; }
    public DbSet<SessionRole> SessionRole { get; }
    
    public DbSet<ReadedRecorder> ReadedRecorder { get; }

    public DbSet<Friendship> Friendship { get; }
    public DbSet<FriendshipTag> FriendshipTag { get; }
    public DbSet<FriendshipTagUnit> FriendshipTagUnit { get; }
    public DbSet<FriendshipRequest> FriendshipRequest { get; }
    public DbSet<OpenedRecorder> OpenedRecorder { get; }
    public DbSet<MessageReminder> MessageReminder { get; }

    public DbSet<Connection> Connection { get; }




    public DbSet<Message> Message { get; }
    public DbSet<MessageContent> MessageContent { get; }
    

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
    public DbSet<Subscription> Subscription { get; }
    
    public DbSet<OfficialMember> OfficialMember { get; }
    public DbSet<OfficialMemberTagUnit> OfficialMemberTagUnit { get; }
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


    public DbSet<Wallet> Wallet { get; }
    public DbSet<WalletRecorder> WalletRecorder { get; }
    public DbSet<WalletBusiness> WalletBusiness { get; }
    public DbSet<PaymentPlatform> PaymentPlatform { get; }
    public DbSet<WalletRequest> RechargeRequest { get; }

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
