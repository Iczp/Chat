using IczpNet.Chat.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IczpNet.Chat.SchemaMigrator;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Contains("--backfill-message-session-seq")) return await MessageSessionSeqBackfill.RunAsync(args);
        if(args.Contains("--import-devices") || args.Contains("--import-app-versions"))return await DeviceImportCommand.RunAsync(args);
        var indexOnly = args.Length == 2 && args.Contains("--apply-session-seq-index") && args.Contains("--source-frozen");
        if(args.Length>0 && !indexOnly){Console.Error.WriteLine("Unknown migration mode. Import: (--import-devices | --import-app-versions) [--apply] (--host | --tenant=<guid>).");return 2;}
        if (indexOnly && await MessageSessionSeqBackfill.RunAsync(["--backfill-message-session-seq", "--verify"]) != 0) return 1;
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
                    sql.MigrationsAssembly(typeof(ChatHttpApiHostMigrationsDbContext).Assembly.GetName().Name)
                        .CommandTimeout(indexOnly ? 1800 : 30))
                .Options;

            await using var context = new ChatHttpApiHostMigrationsDbContext(options);
            if (!context.Database.GetMigrations().Any())
            {
                Console.Error.WriteLine("No Chat migrations were found in the published image.");
                return 1;
            }

            var pending = (await context.Database.GetPendingMigrationsAsync()).ToArray();
            if (indexOnly && (pending.Length != 1 || pending[0] != "20261109112254_AddMessageSessionSeqUniqueIndex"))
                throw new InvalidOperationException("The expected SessionSeq index migration must be the only pending migration.");
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
