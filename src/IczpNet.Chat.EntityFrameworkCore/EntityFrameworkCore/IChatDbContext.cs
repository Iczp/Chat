using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Messages;
using IczpNet.Chat.Messages.Templates;
using IczpNet.Chat.Officials;
using IczpNet.Chat.Robots;
using IczpNet.Chat.Rooms;
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
    DbSet<Message> Message { get; }
    DbSet<Official> Official { get; }
    DbSet<Room> Room { get; }
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
}
