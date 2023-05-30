using IczpNet.Chat.Articles;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Connections;
using IczpNet.Chat.FavoritedRecorders;
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
using IczpNet.Chat.SessionSections.SessionUnitOrganizations;
using IczpNet.Chat.SessionSections.SessionUnitRoles;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnitTags;
using IczpNet.Chat.Scopeds;
using IczpNet.Chat.Wallets;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using IczpNet.Chat.TextContentWords;
using IczpNet.Chat.Words;
using IczpNet.Chat.SessionSections.SessionUnitCounters;
using IczpNet.Chat.SessionSections.SessionUnitSettings;
using IczpNet.Chat.MessageSections.Counters;
using IczpNet.Chat.ActionMenus;

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
    DbSet<Motto> Motto { get; }

    DbSet<Article> Article { get; }
    DbSet<ArticleMessage> ArticleMessage { get; }


    DbSet<Session> Session { get; }
    DbSet<SessionUnit> SessionUnit { get; }
    DbSet<SessionUnitSetting> SessionUnitSetting { get; }
    DbSet<SessionUnitCounter> SessionUnitCounter { get; }
    DbSet<SessionTag> SessionTag { get; }
    DbSet<SessionRole> SessionRole { get; }
    DbSet<SessionOrganization> SessionOrganization { get; }
    DbSet<SessionUnitTag> SessionUnitTag { get; }
    DbSet<SessionUnitRole> SessionUnitRole { get; }
    DbSet<SessionUnitOrganization> SessionUnitOrganization { get; }
    DbSet<SessionPermissionDefinition> SessionPermissionDefinition { get; }
    DbSet<SessionPermissionGroup> SessionPermissionGroup { get; }
    DbSet<SessionRequest> SessionRequest { get; }
    DbSet<SessionPermissionRoleGrant> SessionPermissionRoleGrant { get; }
    DbSet<SessionPermissionUnitGrant> SessionPermissionUnitGrant { get; }

    DbSet<Follow> Follow { get; }

    DbSet<Friendship> Friendship { get; }
    DbSet<FriendshipTag> FriendshipTag { get; }
    DbSet<FriendshipTagUnit> FriendshipTagUnit { get; }
    DbSet<FriendshipRequest> FriendshipRequest { get; }

    DbSet<OpenedRecorder> OpenedRecorder { get; }
    DbSet<ReadedRecorder> ReadedRecorder { get; }
    DbSet<FavoritedRecorder> FavoritedRecorder { get; }
    DbSet<OpenedCounter> OpenedCounter { get; }
    DbSet<ReadedCounter> ReadedCounter { get; }
    DbSet<FavoritedCounter> FavoritedCounter { get; }

    DbSet<MessageReminder> MessageReminder { get; }

    DbSet<Scoped> Scoped { get; }

    DbSet<ActionMenu> ActionMenu { get; }

    DbSet<Connection> Connection { get; }

    DbSet<Message> Message { get; }
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

    DbSet<Wallet> Wallet { get; }
    DbSet<WalletRecorder> WalletRecorder { get; }
    DbSet<WalletBusiness> WalletBusiness { get; }
    DbSet<PaymentPlatform> PaymentPlatform { get; }
    DbSet<WalletRequest> RechargeRequest { get; }


    DbSet<Word> Word { get; }
    DbSet<TextContentWord> TextContentWord { get; }




}
