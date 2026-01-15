using IczpNet.Chat.AppVersionDeviceGroups;
using IczpNet.Chat.AppVersionDevices;
using IczpNet.Chat.AppVersions;
using IczpNet.Chat.Articles;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.ChatObjectCategories;
using IczpNet.Chat.ChatObjectEntryValues;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.ClientConfigs;
using IczpNet.Chat.Connections;
using IczpNet.Chat.ContactTags;
using IczpNet.Chat.DbTables;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.Developers;
using IczpNet.Chat.DeviceGroupMaps;
using IczpNet.Chat.DeviceGroups;
using IczpNet.Chat.Devices;
using IczpNet.Chat.EntryNames;
using IczpNet.Chat.EntryValues;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.Follows;
using IczpNet.Chat.HttpRequests;
using IczpNet.Chat.InvitationCodes;
using IczpNet.Chat.Menus;
using IczpNet.Chat.MessageReports;
using IczpNet.Chat.MessageSections.Counters;
using IczpNet.Chat.MessageSections.MessageContents;
using IczpNet.Chat.MessageSections.MessageFollowers;
using IczpNet.Chat.MessageSections.MessageReminders;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageWords;
using IczpNet.Chat.Mottos;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.ScanCodes;
using IczpNet.Chat.Scopeds;
using IczpNet.Chat.ServerHosts;
using IczpNet.Chat.SessionBoxs;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionGroups;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionPermissionUnitGrants;
using IczpNet.Chat.SessionSections.SessionRequests;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnitContactTags;
using IczpNet.Chat.SessionSections.SessionUnitCounters;
using IczpNet.Chat.SessionSections.SessionUnitEntryValues;
using IczpNet.Chat.SessionSections.SessionUnitOrganizations;
using IczpNet.Chat.SessionSections.SessionUnitRoles;
using IczpNet.Chat.SessionSections.SessionUnitTags;
using IczpNet.Chat.SessionUnitMessages;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.TextContentWords;
using IczpNet.Chat.WalletBusinesses;
using IczpNet.Chat.WalletOrders;
using IczpNet.Chat.WalletRecorders;
using IczpNet.Chat.Wallets;
using IczpNet.Chat.Words;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;

namespace IczpNet.Chat.EntityFrameworkCore;

[ConnectionStringName(ChatDbProperties.ConnectionStringName)]
public class ChatDbContext(DbContextOptions<ChatDbContext> options) : AbpDbContext<ChatDbContext>(options), IChatDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */


    public DbSet<DbTable> DbTable { get; set; }

    public DbSet<ChatObject> ChatObject { get; set; }
    public DbSet<ChatObjectCategory> ChatObjectCategory { get; set; }
    public DbSet<ChatObjectType> ChatObjectType { get; set; }
    public DbSet<ChatObjectEntryValue> ChatObjectEntryValue { get; set; }

    public DbSet<ContactTag> ContactTag { get; set; }

    public DbSet<Motto> Motto { get; set; }

    public DbSet<EntryName> EntryName { get; set; }
    public DbSet<EntryValue> EntryValue { get; set; }

    public DbSet<Article> Article { get; set; }
    public DbSet<ArticleMessage> ArticleMessage { get; set; }

    public DbSet<Session> Session { get; set; }
    public DbSet<SessionUnit> SessionUnit { get; set; }

    public DbSet<Box> SessionBox { get; set; }

    public DbSet<SessionUnitMessage> SessionUnitMessage { get; set; }

    public DbSet<SessionUnitEntryValue> SessionUnitEntryValue { get; set; }
    public DbSet<SessionUnitSetting> SessionUnitSetting { get; set; }
    public DbSet<SessionUnitCounter> SessionUnitCounter { get; set; }

    public DbSet<SessionTag> SessionTag { get; set; }
    public DbSet<SessionRole> SessionRole { get; set; }
    public DbSet<SessionOrganization> SessionOrganization { get; set; }
    public DbSet<SessionUnitTag> SessionUnitTag { get; set; }
    public DbSet<SessionUnitContactTag> SessionUnitContactTag { get; set; }
    public DbSet<SessionUnitRole> SessionUnitRole { get; set; }
    public DbSet<SessionUnitOrganization> SessionUnitOrganization { get; set; }
    public DbSet<SessionPermissionDefinition> SessionPermissionDefinition { get; set; }
    public DbSet<SessionPermissionGroup> SessionPermissionGroup { get; set; }
    public DbSet<SessionRequest> SessionRequest { get; set; }
    public DbSet<SessionPermissionRoleGrant> SessionPermissionRoleGrant { get; set; }
    public DbSet<SessionPermissionUnitGrant> SessionPermissionUnitGrant { get; set; }


    public DbSet<Follow> Follow { get; set; }

    public DbSet<OpenedRecorder> OpenedRecorder { get; set; }
    public DbSet<ReadedRecorder> ReadedRecorder { get; set; }
    public DbSet<FavoritedRecorder> FavoritedRecorder { get; set; }
    public DbSet<DeletedRecorder> DeletedRecorder { get; set; }


    public DbSet<OpenedCounter> OpenedCounter { get; set; }
    public DbSet<ReadedCounter> ReadedCounter { get; set; }
    public DbSet<FavoritedCounter> FavoritedCounter { get; set; }
    public DbSet<DeletedCounter> DeletedCounter { get; set; }

    public DbSet<MessageReminder> MessageReminder { get; set; }

    public DbSet<MessageFollower> MessageFollower { get; set; }

    public DbSet<Scoped> Scoped { get; set; }

    public DbSet<Menu> Menu { get; set; }
    public DbSet<Developer> Developer { get; set; }

    public DbSet<Connection> Connection { get; set; }
    public DbSet<ServerHost> ServerHost { get; }


    public DbSet<Message> Message { get; set; }

    public DbSet<MessageWord> MessageWord { get; set; }

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
    public DbSet<MessageReportDay> MessageReportDay { get; set; }
    public DbSet<MessageReportMonth> MessageReportMonth { get; set; }
    public DbSet<MessageReportHour> MessageReportHour { get; set; }

    public DbSet<Wallet> Wallet { get; set; }

    public DbSet<WalletOrder> WalletOrder { get; set; }

    public DbSet<WalletRecorder> WalletRecorder { get; set; }
    
    public DbSet<WalletBusiness> WalletBusiness { get; set; }
    public DbSet<PaymentPlatform> PaymentPlatform { get; set; }
    public DbSet<WalletRequest> RechargeRequest { get; set; }

    public DbSet<Word> Word { get; set; }
    public DbSet<TextContentWord> TextContentWord { get; set; }


    public DbSet<HttpRequest> HttpRequest { get; set; }


    public DbSet<Blob> Blob { get; set; }
    public DbSet<BlobContent> BlobContent { get; set; }

    public DbSet<InvitationCode> InvitationCode { get; set; }

    public DbSet<ClientConfig> ClientConfig { get; set; }

    public DbSet<Device> Device { get; set; }

    public DbSet<DeviceGroup> DeviceGroup { get; set; }

    public DbSet<DeviceGroupMap> DeviceGroupMap { get; set; }

    public DbSet<UserDevice> UserDevice { get; set; }

    public DbSet<AppVersion> AppVersion { get; set; }

    public DbSet<AppVersionDevice> AppVersionDevice { get; set; }

    public DbSet<AppVersionDeviceGroup> AppVersionDeviceGroup { get; set; }


    public DbSet<ScanCode> ScanCode { get; set; }

    public DbSet<ScanHandler> ScanHandler { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureChat();
        builder.ConfigureIdentity();
    }
}
