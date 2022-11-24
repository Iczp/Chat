using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IczpNet.Chat.EntityFrameworkCore;

public class ChatHttpApiHostMigrationsDbContextFactory : IDesignTimeDbContextFactory<ChatHttpApiHostMigrationsDbContext>
{
    public ChatHttpApiHostMigrationsDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<ChatHttpApiHostMigrationsDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Chat"));

        return new ChatHttpApiHostMigrationsDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
