using IczpNet.Chat.Articles;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatUsers;
using IczpNet.Chat.Connections;
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
using IczpNet.Chat.SessionSections;
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

namespace IczpNet.Chat.EntityFrameworkCore;

[ConnectionStringName(ChatDbProperties.ConnectionStringName)]
public interface IChatDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */

    DbSet<ChatObject> ChatObject { get; }
    DbSet<ChatUser> ChatUser { get; }

    DbSet<Article> Article { get; }
    DbSet<ArticleMessage> ArticleMessage { get; }


    DbSet<Session> Session { get; }
    DbSet<SessionUnit> SessionUnit { get; }
    DbSet<SessionTag> SessionTag { get; }
    DbSet<SessionRole> SessionRole { get; }


    DbSet<ReadedRecorder> ReadedRecorder { get; }
    DbSet<Friendship> Friendship { get; }
    DbSet<FriendshipTag> FriendshipTag { get; }
    DbSet<FriendshipTagUnit> FriendshipTagUnit { get; }
    DbSet<FriendshipRequest> FriendshipRequest { get; }
    DbSet<OpenedRecorder> OpenedRecorder { get; }
    DbSet<MessageReminder> MessageReminder { get; }


    DbSet<Connection> Connection { get; }



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
    DbSet<Subscription> Subscription { get; }
    DbSet<OfficialMember> OfficialMember { get; }
    DbSet<OfficialMemberTagUnit> OfficialMemberTagUnit { get; }
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

    DbSet<Square> Square { get; }
    DbSet<SquareCategory> SquareCategory { get; }
    DbSet<SquareMember> SquareMember { get; }


    DbSet<Wallet> Wallet { get; }
    DbSet<WalletRecorder> WalletRecorder { get; }
    DbSet<WalletBusiness> WalletBusiness { get; }
    DbSet<PaymentPlatform> PaymentPlatform { get; }
    DbSet<WalletRequest> RechargeRequest { get; }
}
