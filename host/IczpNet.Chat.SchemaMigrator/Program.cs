using IczpNet.Chat.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IczpNet.Chat.SchemaMigrator;

public static class Program
{
    public static async Task<int> Main()
    {
        try
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Chat")
                ?? Environment.GetEnvironmentVariable("CHAT_CONNECTION_STRING");
            var expectedDatabase = Environment.GetEnvironmentVariable("ChatMigration__ExpectedDatabase")
                ?? Environment.GetEnvironmentVariable("CHAT_MIGRATION_EXPECTED_DATABASE")
                ?? "Chat_Module_v3";

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(expectedDatabase))
            {
                Console.Error.WriteLine("Both ConnectionStrings__Chat and ChatMigration__ExpectedDatabase are required.");
                return 1;
            }

            var target = new SqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(target.InitialCatalog)
                || !string.Equals(target.InitialCatalog, expectedDatabase, StringComparison.OrdinalIgnoreCase)
                || IsSystemDatabase(target.InitialCatalog))
            {
                Console.Error.WriteLine($"Chat migration target database '{target.InitialCatalog}' does not match ChatMigration__ExpectedDatabase '{expectedDatabase}', or is a SQL Server system database.");
                return 1;
            }

            var options = new DbContextOptionsBuilder<ChatHttpApiHostMigrationsDbContext>()
                .UseSqlServer(connectionString, sql =>
                    sql.MigrationsAssembly(typeof(ChatHttpApiHostMigrationsDbContext).Assembly.GetName().Name))
                .Options;

            await using var context = new ChatHttpApiHostMigrationsDbContext(options);
            if (!context.Database.GetMigrations().Any())
            {
                Console.Error.WriteLine("No Chat migrations were found in the published image.");
                return 1;
            }

            var pending = (await context.Database.GetPendingMigrationsAsync()).ToArray();
            Console.WriteLine($"Chat database '{target.InitialCatalog}': {pending.Length} pending migration(s).");

            foreach (var migration in pending)
            {
                Console.WriteLine($"  {migration}");
            }

            if (pending.Length > 0)
            {
                await context.Database.MigrateAsync();
            }

            Console.WriteLine("Chat schema migration completed successfully.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Chat schema migration failed: {exception.GetType().Name}: {exception.Message}. Check SQL Server and migration logs.");
            return 1;
        }
    }

    private static bool IsSystemDatabase(string database) =>
        database.Equals("master", StringComparison.OrdinalIgnoreCase)
        || database.Equals("model", StringComparison.OrdinalIgnoreCase)
        || database.Equals("msdb", StringComparison.OrdinalIgnoreCase)
        || database.Equals("tempdb", StringComparison.OrdinalIgnoreCase);
}
