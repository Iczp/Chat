using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Messages;
using IczpNet.Chat.Officials;
using IczpNet.Chat.Robots;
using IczpNet.Chat.Rooms;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.EntityFrameworkCore;

[ConnectionStringName(ChatDbProperties.ConnectionStringName)]
public class ChatDbContext : AbpDbContext<ChatDbContext>, IChatDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */
    public DbSet<ChatObject> ChatObject { get; }
    public DbSet<Message> Message { get; }
    public DbSet<Official> Official { get; }
    public DbSet<Room> Room { get; }
    public DbSet<Robot> Robot { get; }

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
