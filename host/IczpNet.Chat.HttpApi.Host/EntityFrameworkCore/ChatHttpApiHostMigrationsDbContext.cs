using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.EntityFrameworkCore;

public class ChatHttpApiHostMigrationsDbContext : AbpDbContext<ChatHttpApiHostMigrationsDbContext>
{
    public ChatHttpApiHostMigrationsDbContext(DbContextOptions<ChatHttpApiHostMigrationsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureChat();
    }
}
