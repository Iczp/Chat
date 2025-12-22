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
using IczpNet.Chat.MessageSections.Counters;
using IczpNet.Chat.MessageSections.MessageContents;
using IczpNet.Chat.MessageSections.MessageFollowers;
using IczpNet.Chat.MessageSections.MessageReminders;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageStats;
using IczpNet.Chat.MessageWords;
using IczpNet.Chat.Mottos;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.ScanCodes;
using IczpNet.Chat.Scopeds;
using IczpNet.Chat.ServerHosts;
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

namespace IczpNet.Chat.EntityFrameworkCore;

[ConnectionStringName(ChatDbProperties.ConnectionStringName)]
public interface IChatDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */

    DbSet<ChatObject> ChatObject { get; }
    DbSet<ChatObjectCategory> ChatObjectCategory { get; }
    DbSet<ChatObjectType> ChatObjectType { get; }
    DbSet<ChatObjectEntryValue> ChatObjectEntryValue { get; }

    DbSet<ContactTag> ContactTag { get; set; }

    DbSet<Motto> Motto { get; }

    DbSet<EntryName> EntryName { get; }
    DbSet<EntryValue> EntryValue { get; }

    DbSet<Article> Article { get; }
    DbSet<ArticleMessage> ArticleMessage { get; }

    DbSet<Session> Session { get; }
    DbSet<SessionUnit> SessionUnit { get; }

    DbSet<SessionUnitMessage> SessionUnitMessage { get; }

    DbSet<SessionUnitEntryValue> SessionUnitEntryValue { get; }
    DbSet<SessionUnitSetting> SessionUnitSetting { get; }
    DbSet<SessionUnitCounter> SessionUnitCounter { get; }
    DbSet<SessionTag> SessionTag { get; }
    DbSet<SessionRole> SessionRole { get; }
    DbSet<SessionOrganization> SessionOrganization { get; }
    DbSet<SessionUnitTag> SessionUnitTag { get; }
    DbSet<SessionUnitContactTag> SessionUnitContactTag { get; }
    DbSet<SessionUnitRole> SessionUnitRole { get; }
    DbSet<SessionUnitOrganization> SessionUnitOrganization { get; }
    DbSet<SessionPermissionDefinition> SessionPermissionDefinition { get; }
    DbSet<SessionPermissionGroup> SessionPermissionGroup { get; }
    DbSet<SessionRequest> SessionRequest { get; }
    DbSet<SessionPermissionRoleGrant> SessionPermissionRoleGrant { get; }
    DbSet<SessionPermissionUnitGrant> SessionPermissionUnitGrant { get; }

    DbSet<Follow> Follow { get; }

    DbSet<OpenedRecorder> OpenedRecorder { get; }
    DbSet<ReadedRecorder> ReadedRecorder { get; }
    DbSet<FavoritedRecorder> FavoritedRecorder { get; }
    DbSet<DeletedRecorder> DeletedRecorder { get; }

    DbSet<OpenedCounter> OpenedCounter { get; }
    DbSet<ReadedCounter> ReadedCounter { get; }
    DbSet<FavoritedCounter> FavoritedCounter { get; }
    DbSet<DeletedCounter> DeletedCounter { get; }

    DbSet<MessageReminder> MessageReminder { get; }
    DbSet<MessageFollower> MessageFollower { get; }

    DbSet<Scoped> Scoped { get; }

    DbSet<Menu> Menu { get; }

    DbSet<Developer> Developer { get; }

    DbSet<Connection> Connection { get; }

    DbSet<ServerHost> ServerHost { get; }

    DbSet<Message> Message { get; }
    DbSet<MessageWord> MessageWord { get; }
    DbSet<MessageContent> MessageContent { get; }

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
    DbSet<MessageStat> MessageStat { get; }

    DbSet<Wallet> Wallet { get; }
    DbSet<WalletOrder> WalletOrder { get; }
    DbSet<WalletRecorder> WalletRecorder { get; }
    DbSet<WalletBusiness> WalletBusiness { get; }
    DbSet<PaymentPlatform> PaymentPlatform { get; }
    DbSet<WalletRequest> RechargeRequest { get; }

    DbSet<Word> Word { get; }
    DbSet<TextContentWord> TextContentWord { get; }

    DbSet<HttpRequest> HttpRequest { get; }

    DbSet<Blob> Blob { get; }
    DbSet<BlobContent> BlobContent { get; }

    DbSet<InvitationCode> InvitationCode { get; }

    DbSet<ClientConfig> ClientConfig { get; }

    DbSet<Device> Device { get; set; }

    DbSet<DeviceGroup> DeviceGroup { get; }

    DbSet<DeviceGroupMap> DeviceGroupMap { get; }

    DbSet<UserDevice> UserDevice { get; }

    DbSet<AppVersion> AppVersion { get; }

    DbSet<AppVersionDevice> AppVersionDevice { get; }

    DbSet<AppVersionDeviceGroup> AppVersionDeviceGroup { get; }

    DbSet<ScanCode> ScanCode { get; }

    DbSet<ScanHandler> ScanHandler { get; }


}
