using IczpNet.Chat.Articles;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Connections;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.MessageContents;
using IczpNet.Chat.MessageSections.MessageReminders;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Mottos;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.SessionSections.FriendshipRequests;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.FriendshipTagUnits;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionGroups;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionRequests;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnits;
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
    public DbSet<ChatObject> ChatObject { get; set; }
    public DbSet<ChatObjectCategory> ChatObjectCategory { get; set; }
    public DbSet<ChatObjectType> ChatObjectType { get; set; }
    public DbSet<Motto> Motto { get; set; }

    public DbSet<Article> Article { get; set; }
    public DbSet<ArticleMessage> ArticleMessage { get; set; }

    public DbSet<Session> Session { get; set; }
    public DbSet<SessionUnit> SessionUnit { get; set; }
    public DbSet<SessionTag> SessionTag { get; set; }
    public DbSet<SessionRole> SessionRole { get; set; }
    public DbSet<SessionOrganization> SessionOrganization { get; set; }
    public DbSet<SessionPermissionDefinition> SessionPermissionDefinition { get; set; }
    public DbSet<SessionPermissionGroup> SessionPermissionGroup { get; set; }
    public DbSet<SessionRequest> SessionRequest { get; set; }
    public DbSet<SessionPermissionRoleGrant> SessionPermissionRoleGrant { get; set; }
    public DbSet<SessionPermissionUnitGrant> SessionPermissionUnitGrant { get; set; }


    public DbSet<Follow> Follow { get; set; }

    public DbSet<ReadedRecorder> ReadedRecorder { get; set; }

    public DbSet<Friendship> Friendship { get; set; }
    public DbSet<FriendshipTag> FriendshipTag { get; set; }
    public DbSet<FriendshipTagUnit> FriendshipTagUnit { get; set; }
    public DbSet<FriendshipRequest> FriendshipRequest { get; set; }
    public DbSet<OpenedRecorder> OpenedRecorder { get; set; }
    public DbSet<MessageReminder> MessageReminder { get; set; }

    public DbSet<Connection> Connection { get; set; }


    public DbSet<Message> Message { get; set; }
    public DbSet<MessageContent> MessageContent { get; set; }


    public DbSet<CmdContent> CmdMessage { get; set; }
    public DbSet<TextContent> TextMessage { get; set; }
    public DbSet<HtmlContent> HtmlContent { get; set; }
    public DbSet<ArticleContent> ArticleContent { get; set; }
    public DbSet<LinkContent> LinkContent { get; set; }
    public DbSet<ImageContent> ImageMessage { get; set; }
    public DbSet<SoundContent> SoundContent { get; set; }
    public DbSet<VideoContent> VideoContent { get; set; }
    public DbSet<FileContent> FileContent { get; set; }
    public DbSet<LocationContent> LocationContent { get; set; }
    public DbSet<ContactsContent> ContactsContent { get; set; }
    public DbSet<RedEnvelopeContent> RedEnvelopeContent { get; set; }
    public DbSet<RedEnvelopeUnit> RedEnvelopeUnit { get; set; }
    public DbSet<HistoryContent> HistoryContent { get; set; }
    public DbSet<HistoryMessage> HistoryMessage { get; set; }


    public DbSet<Wallet> Wallet { get; set; }
    public DbSet<WalletRecorder> WalletRecorder { get; set; }
    public DbSet<WalletBusiness> WalletBusiness { get; set; }
    public DbSet<PaymentPlatform> PaymentPlatform { get; set; }
    public DbSet<WalletRequest> RechargeRequest { get; set; }



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
