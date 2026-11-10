using System.Data;
using System.Diagnostics;
using System.Text.Json;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Sessions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace IczpNet.Chat.SchemaMigrator;

// Explicit maintenance command. All queries address physical rows, bypassing ABP filters.
internal static class MessageSessionSeqBackfill
{
    private sealed record Journal(string Server, string Database, long Messages, long Sessions, int CountsFingerprint, int MessageFingerprint);
    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var known = args.All(x => x is "--backfill-message-session-seq" or "--dry-run" or "--apply" or "--resume" or "--verify" or "--self-test" or "--source-frozen" || x.StartsWith("--batch-size=", StringComparison.Ordinal) || x.StartsWith("--journal=", StringComparison.Ordinal));
            if (!known || args.Distinct().Count() != args.Length) throw new InvalidOperationException("Unknown or duplicate argument.");
            var modes = new[] { "--dry-run", "--apply", "--verify", "--self-test" }.Count(args.Contains);
            if (modes != 1 || (args.Contains("--resume") && !args.Contains("--apply"))) throw new InvalidOperationException("Choose exactly one of --dry-run, --apply or --verify; --resume requires --apply.");
            var sizeArgs = args.Where(x => x.StartsWith("--batch-size=", StringComparison.Ordinal)).ToArray();
            if (sizeArgs.Length > 1 || (sizeArgs.Length == 1 && (!int.TryParse(sizeArgs[0][13..], out var parsed) || parsed < 1 || parsed > 50000))) throw new InvalidOperationException("Batch size must be 1..50000.");
            var batchSize = sizeArgs.Length == 0 ? 10000 : int.Parse(sizeArgs[0][13..]);
            var journalArgs = args.Where(x => x.StartsWith("--journal=", StringComparison.Ordinal)).ToArray();
            if (journalArgs.Length > 1 || (args.Contains("--apply") != (journalArgs.Length == 1))) throw new InvalidOperationException("--apply requires exactly one --journal=<absolute path>; other modes must not specify it.");
            var journalPath = journalArgs.Length == 0 ? null : journalArgs[0][10..];
            if (journalPath != null && !Path.IsPathFullyQualified(journalPath)) throw new InvalidOperationException("Journal path must be absolute.");
            if (args.Contains("--apply") && !args.Contains("--source-frozen")) throw new InvalidOperationException("--apply requires --source-frozen after stopping all writers.");
            var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Chat") ?? Environment.GetEnvironmentVariable("CHAT_CONNECTION_STRING") ?? throw new InvalidOperationException("Missing Chat connection string.");
            var expected = Environment.GetEnvironmentVariable("ChatMigration__ExpectedDatabase") ?? throw new InvalidOperationException("Set ChatMigration__ExpectedDatabase explicitly.");
            var target = new SqlConnectionStringBuilder(cs);
            if (!target.InitialCatalog.Equals(expected, StringComparison.OrdinalIgnoreCase) || new[] { "master", "model", "msdb", "tempdb" }.Contains(expected, StringComparer.OrdinalIgnoreCase)) throw new InvalidOperationException("Unexpected database.");
            var options = new DbContextOptionsBuilder<ChatHttpApiHostMigrationsDbContext>().UseSqlServer(cs).Options;
            await using var db = new ChatHttpApiHostMigrationsDbContext(options);
            var message = db.Model.FindEntityType(typeof(Message)) ?? throw new InvalidOperationException("Message mapping missing.");
            var session = db.Model.FindEntityType(typeof(Session)) ?? throw new InvalidOperationException("Session mapping missing.");
            if (message.GetTableName() != "Chat_Message" || session.GetTableName() != "Chat_Session" || (message.GetSchema() ?? "dbo") != "dbo" || (session.GetSchema() ?? "dbo") != "dbo") throw new InvalidOperationException("Unexpected EF table mapping.");
            await db.Database.OpenConnectionAsync();
            var connection = (SqlConnection)db.Database.GetDbConnection();
            await using (var appLock = new SqlCommand("DECLARE @r int; EXEC @r = sp_getapplock @Resource=N'Chat.MessageSessionSeq.Backfill', @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=0; SELECT @r;", connection))
                if (Convert.ToInt32(await appLock.ExecuteScalarAsync()) < 0) throw new InvalidOperationException("Another backfill runner holds the application lock.");
            if (args.Contains("--self-test")) { await SelfTestAsync(connection); await ConcurrentSelfTestAsync(cs); return 0; }
            await using (var triggers = new SqlCommand("SELECT COUNT(*) FROM sys.triggers WHERE is_disabled=0 AND parent_id IN (OBJECT_ID(N'dbo.Chat_Message'), OBJECT_ID(N'dbo.Chat_Session'));", connection))
                if (Convert.ToInt32(await triggers.ExecuteScalarAsync()) != 0) throw new InvalidOperationException("Enabled Message or Session triggers require review before backfill.");
            Console.WriteLine($"Server={target.DataSource}; Database={target.InitialCatalog}; mode={(args.Contains("--apply") ? "apply" : args.Contains("--verify") ? "verify" : "dry-run")}; batch={batchSize}");
            var baseline = await CheckAsync(connection, args.Contains("--verify"), args.Contains("--resume"));
            if (!args.Contains("--apply")) return 0;
            var journal = new Journal(target.DataSource, target.InitialCatalog, baseline.Messages, baseline.Sessions, baseline.CountsFingerprint, baseline.MessageFingerprint);
            if (args.Contains("--resume"))
            {
                var previous = JsonSerializer.Deserialize<Journal>(await File.ReadAllTextAsync(journalPath!));
                if (previous != journal) throw new InvalidOperationException("Resume journal does not match this database and its original data fingerprints.");
            }
            else
            {
                await using var file = new FileStream(journalPath!, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                await JsonSerializer.SerializeAsync(file, journal);
                await file.FlushAsync();
                file.Flush(flushToDisk: true);
            }
            long cursor = 0, total = 0, batches = 0, retries = 0;
            var all = Stopwatch.StartNew();
            while (true)
            {
                var committed = false;
                for (var attempt = 0; attempt < 4 && !committed; attempt++)
                {
                    try
                    {
                        var sw = Stopwatch.StartNew();
                        await using var tx = (SqlTransaction)await connection.BeginTransactionAsync();
                        await using var command = new SqlCommand(BatchSql, connection, tx) { CommandTimeout = 300 };
                        command.Parameters.Add("@cursor", SqlDbType.BigInt).Value = cursor;
                        command.Parameters.Add("@batch", SqlDbType.Int).Value = batchSize;
                        await using var reader = await command.ExecuteReaderAsync();
                        await reader.ReadAsync();
                        var count = reader.GetInt32(0);
                        var next = reader.GetInt64(1);
                        await reader.DisposeAsync();
                        await tx.CommitAsync();
                        committed = true;
                        if (count == 0)
                        {
                            Console.WriteLine($"Completed batches={batches}; messages={total}; retries={retries}; elapsed={all.Elapsed}");
                            var final = await CheckAsync(connection, true, true);
                            if (baseline != final) throw new InvalidOperationException("Message/session counts or per-session count fingerprint changed during maintenance.");
                            Console.WriteLine("Baseline message/session counts and per-session count fingerprint match.");
                            return 0;
                        }
                        cursor = next; total += count; batches++;
                        Console.WriteLine($"batch={batches}; lastMessageId={cursor}; messages={count}; total={total}; elapsedMs={sw.ElapsedMilliseconds}; retries={retries}");
                    }
                    catch (SqlException ex) when ((ex.Number == 1205 || ex.Number == -2) && attempt < 3)
                    {
                        retries++;
                        Console.Error.WriteLine($"Batch after message {cursor} failed with SQL {ex.Number}; retry {attempt + 1}/3.");
                        await Task.Delay(TimeSpan.FromSeconds(attempt + 1));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"SessionSeq maintenance failed: {ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }

    private static async Task SelfTestAsync(SqlConnection connection)
    {
        var journal = new Journal("local", "test", 31, 3, 123, 456);
        if (JsonSerializer.Deserialize<Journal>(JsonSerializer.Serialize(journal)) != journal) throw new InvalidOperationException("Journal round-trip failed.");
        const string setup = """
            CREATE TABLE #Messages (Id bigint NOT NULL PRIMARY KEY, SessionId uniqueidentifier NOT NULL, SessionSeq bigint NOT NULL);
            CREATE TABLE #Sessions (Id uniqueidentifier NOT NULL PRIMARY KEY, LastMessageSeq bigint NOT NULL);
            DECLARE @a uniqueidentifier = NEWID(), @b uniqueidentifier = NEWID(), @empty uniqueidentifier = NEWID();
            INSERT #Sessions VALUES (@a,0),(@b,0),(@empty,0);
            ;WITH numbers AS (SELECT 1 AS n UNION ALL SELECT n+1 FROM numbers WHERE n<31)
            INSERT #Messages SELECT n, CASE WHEN n%3=0 THEN @b ELSE @a END, 0 FROM numbers;
            """;
        await using (var setupCommand = new SqlCommand(setup, connection)) await setupCommand.ExecuteNonQueryAsync();
        var testSql = BatchSql.Replace("dbo.Chat_Message", "#Messages", StringComparison.Ordinal).Replace("dbo.Chat_Session", "#Sessions", StringComparison.Ordinal);
        long cursor = 0;
        for (var step = 0; step < 20; step++)
        {
            await using var tx = (SqlTransaction)await connection.BeginTransactionAsync();
            await using var command = new SqlCommand(testSql, connection, tx);
            command.Parameters.Add("@cursor", SqlDbType.BigInt).Value = cursor;
            command.Parameters.Add("@batch", SqlDbType.Int).Value = 4;
            await using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync(); var count = reader.GetInt32(0); var next = reader.GetInt64(1);
            await reader.DisposeAsync();
            if (step == 0) { await tx.RollbackAsync(); continue; }
            await tx.CommitAsync();
            if (count == 0) break;
            cursor = next;
            if (step == 3) cursor = 0; // Simulate restarting after committed batches.
        }
        const string verify = """
            ;WITH ranked AS (SELECT SessionSeq, ROW_NUMBER() OVER (PARTITION BY SessionId ORDER BY Id) AS Expected FROM #Messages)
            SELECT (SELECT COUNT(*) FROM ranked WHERE SessionSeq<>Expected) +
                   (SELECT COUNT(*) FROM #Sessions s WHERE s.LastMessageSeq<>COALESCE((SELECT MAX(SessionSeq) FROM #Messages m WHERE m.SessionId=s.Id),0));
            """;
        await using var check = new SqlCommand(verify, connection);
        if (Convert.ToInt32(await check.ExecuteScalarAsync()) != 0) throw new InvalidOperationException("Batch, rollback, or empty-session self-test failed.");
        // Re-running from the beginning must find no unprocessed messages.
        await using var repeat = new SqlCommand(testSql, connection);
        repeat.Parameters.Add("@cursor", SqlDbType.BigInt).Value = 0;
        repeat.Parameters.Add("@batch", SqlDbType.Int).Value = 4;
        if (Convert.ToInt32(await repeat.ExecuteScalarAsync()) != 0) throw new InvalidOperationException("Repeat self-test failed.");
        Console.WriteLine("Self-test passed: interleaved sessions, empty session, four-message chunks, rollback, restart, and repeat.");
    }

    // Global temporary tables make this test independent of real Chat data and shared by separate connections.
    private static async Task ConcurrentSelfTestAsync(string connectionString)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var sessions = $"##SeqSessions_{suffix}";
        var messages = $"##SeqMessages_{suffix}";
        var a = Guid.NewGuid(); var b = Guid.NewGuid();
        await using var owner = new SqlConnection(connectionString);
        await owner.OpenAsync();
        await using (var setup = new SqlCommand($"CREATE TABLE {sessions}(Id uniqueidentifier PRIMARY KEY, Seq bigint NOT NULL); CREATE TABLE {messages}(Id bigint IDENTITY PRIMARY KEY, SessionId uniqueidentifier NOT NULL, Seq bigint NOT NULL, UNIQUE(SessionId, Seq)); INSERT {sessions} VALUES (@a,0),(@b,0);", owner))
        {
            setup.Parameters.Add("@a", SqlDbType.UniqueIdentifier).Value = a;
            setup.Parameters.Add("@b", SqlDbType.UniqueIdentifier).Value = b;
            await setup.ExecuteNonQueryAsync();
        }
        await Task.WhenAll(Enumerable.Range(0, 3).Select(async worker =>
        {
            var options = new DbContextOptionsBuilder<ChatHttpApiHostMigrationsDbContext>().UseSqlServer(connectionString).Options;
            await using var context = new ChatHttpApiHostMigrationsDbContext(options);
            await context.Database.OpenConnectionAsync();
            var conn = (SqlConnection)context.Database.GetDbConnection();
            for (var i = 0; i < 40; i++)
            {
                await using var transaction = await context.Database.BeginTransactionAsync();
                await using var cmd = new SqlCommand($"UPDATE {sessions} SET Seq=Seq+1 WHERE Id=@id; DECLARE @n bigint=(SELECT Seq FROM {sessions} WHERE Id=@id); INSERT {messages}(SessionId,Seq) VALUES (@id,@n);", conn, (SqlTransaction)transaction.GetDbTransaction());
                cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = (i + worker) % 2 == 0 ? a : b;
                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
        }));
        await using (var rollback = (SqlTransaction)await owner.BeginTransactionAsync())
        {
            await using var command = new SqlCommand($"UPDATE {sessions} SET Seq=Seq+1 WHERE Id=@a", owner, rollback);
            command.Parameters.Add("@a", SqlDbType.UniqueIdentifier).Value = a;
            await command.ExecuteNonQueryAsync();
            await rollback.RollbackAsync();
        }
        await using (var check = new SqlCommand($"SELECT (SELECT COUNT(*) FROM {messages}), (SELECT COUNT(*) FROM {sessions} s WHERE s.Seq<>(SELECT COALESCE(MAX(m.Seq),0) FROM {messages} m WHERE m.SessionId=s.Id)), (SELECT COUNT(*) FROM (SELECT SessionId,Seq FROM {messages} GROUP BY SessionId,Seq HAVING COUNT(*)>1) d), (SELECT COUNT(DISTINCT SessionId) FROM {messages});", owner))
        await using (var reader = await check.ExecuteReaderAsync())
        {
            await reader.ReadAsync();
            if (reader.GetInt32(0) != 120 || reader.GetInt32(1) != 0 || reader.GetInt32(2) != 0 || reader.GetInt32(3) != 2) throw new InvalidOperationException("Concurrent sequence or rollback self-test failed.");
        }
        try
        {
            await using var duplicate = new SqlCommand($"INSERT {messages}(SessionId,Seq) VALUES (@a,1)", owner);
            duplicate.Parameters.Add("@a", SqlDbType.UniqueIdentifier).Value = a;
            await duplicate.ExecuteNonQueryAsync();
            throw new InvalidOperationException("Unique index did not reject a duplicate.");
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627) { }
        Console.WriteLine("SQL temporary-table self-test passed: 120 commits across three independent EF contexts and two sessions, rollback, and duplicate rejection.");
    }

    private static async Task<(long Messages, long Sessions, int CountsFingerprint, int MessageFingerprint)> CheckAsync(SqlConnection connection, bool complete, bool allowResume)
    {
        await using var command = new SqlCommand(CheckSql, connection) { CommandTimeout = 900 };
        await using var r = await command.ExecuteReaderAsync();
        await r.ReadAsync();
        var migration = r.GetInt32(0); var messages = r.GetInt64(1); var zeros = r.GetInt64(2); var positive = r.GetInt64(3); var invalid = r.GetInt64(4); var messageFingerprint = r.GetInt32(5);
        await r.NextResultAsync(); await r.ReadAsync();
        var orphans = r.GetInt64(0);
        await r.NextResultAsync(); await r.ReadAsync();
        var orderErrors = r.GetInt64(0);
        await r.NextResultAsync(); await r.ReadAsync();
        var sessions = r.GetInt64(0); var counterErrors = r.GetInt64(1); var fingerprint = r.GetInt32(2);
        await r.NextResultAsync(); await r.ReadAsync(); var duplicates = r.GetInt64(0);
        Console.WriteLine($"migration={migration}; messages={messages}; zero={zeros}; positive={positive}; invalid={invalid}; orphans={orphans}; orderErrors={orderErrors}; sessions={sessions}; counterErrors={counterErrors}; duplicateKeys={duplicates}; countsFingerprint={fingerprint}; messageFingerprint={messageFingerprint}");
        if (migration != 1 || invalid != 0 || orphans != 0 || orderErrors != 0 || counterErrors != 0 || duplicates != 0 || (complete && zeros != 0) || (!complete && !allowResume && positive != 0))
        {
            await r.DisposeAsync();
            await ReportExamplesAsync(connection);
            throw new InvalidOperationException("Data precondition or validation failed; no further writes are permitted.");
        }
        return (messages, sessions, fingerprint, messageFingerprint);
    }

    private static async Task ReportExamplesAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT TOP (20) 'invalid_or_zero' Issue, CONVERT(nvarchar(36),SessionId) SessionId, CONVERT(nvarchar(30),Id) MessageId, CONVERT(nvarchar(30),SessionSeq) Seq FROM dbo.Chat_Message WHERE SessionSeq IS NULL OR SessionSeq<=0 OR SessionId IS NULL ORDER BY Id;
            SELECT TOP (20) 'orphan' Issue, CONVERT(nvarchar(36),m.SessionId) SessionId, CONVERT(nvarchar(30),m.Id) MessageId, CONVERT(nvarchar(30),m.SessionSeq) Seq FROM dbo.Chat_Message m LEFT JOIN dbo.Chat_Session s ON s.Id=m.SessionId WHERE s.Id IS NULL ORDER BY m.Id;
            ;WITH ranked AS (SELECT Id,SessionId,SessionSeq,ROW_NUMBER() OVER (PARTITION BY SessionId ORDER BY Id) Expected, SUM(CONVERT(bigint,CASE WHEN SessionSeq=0 THEN 1 ELSE 0 END)) OVER (PARTITION BY SessionId ORDER BY Id ROWS UNBOUNDED PRECEDING) EarlierZeros FROM dbo.Chat_Message)
            SELECT TOP (20) 'order' Issue, CONVERT(nvarchar(36),SessionId) SessionId, CONVERT(nvarchar(30),Id) MessageId, CONVERT(nvarchar(30),SessionSeq) Seq FROM ranked WHERE SessionSeq>0 AND (SessionSeq<>Expected OR EarlierZeros<>0) ORDER BY Id;
            SELECT TOP (20) 'duplicate' Issue, CONVERT(nvarchar(36),SessionId) SessionId, CONVERT(nvarchar(30),MIN(Id)) MessageId, CONVERT(nvarchar(30),SessionSeq) Seq FROM dbo.Chat_Message WHERE SessionSeq>0 GROUP BY SessionId,SessionSeq HAVING COUNT_BIG(*)>1 ORDER BY MIN(Id);
            ;WITH maxima AS (SELECT SessionId,MAX(SessionSeq) Seq FROM dbo.Chat_Message GROUP BY SessionId)
            SELECT TOP (20) 'counter' Issue, CONVERT(nvarchar(36),s.Id) SessionId, CAST(NULL AS nvarchar(30)) MessageId, CONVERT(nvarchar(30),s.LastMessageSeq) Seq FROM dbo.Chat_Session s LEFT JOIN maxima m ON m.SessionId=s.Id WHERE s.LastMessageSeq<>COALESCE(m.Seq,0) ORDER BY s.Id;
            """;
        await using var command = new SqlCommand(sql, connection) { CommandTimeout = 900 };
        await using var reader = await command.ExecuteReaderAsync();
        do { while (await reader.ReadAsync()) Console.Error.WriteLine($"example issue={reader.GetString(0)} session={(reader.IsDBNull(1) ? "NULL" : reader.GetString(1))} message={(reader.IsDBNull(2) ? "NULL" : reader.GetString(2))} seq={(reader.IsDBNull(3) ? "NULL" : reader.GetString(3))}"); } while (await reader.NextResultAsync());
    }

    private const string BatchSql = """
        CREATE TABLE #batch (Id bigint NOT NULL PRIMARY KEY, SessionId uniqueidentifier NOT NULL);
        INSERT #batch (Id, SessionId)
        SELECT TOP (@batch) Id, SessionId FROM dbo.Chat_Message
        WHERE Id > @cursor AND SessionSeq = 0 ORDER BY Id;
        ;WITH ranked AS (
            SELECT b.Id, ROW_NUMBER() OVER (PARTITION BY b.SessionId ORDER BY b.Id) + s.LastMessageSeq AS NewSeq
            FROM #batch b JOIN dbo.Chat_Session s ON s.Id = b.SessionId
        )
        UPDATE m SET SessionSeq = r.NewSeq FROM dbo.Chat_Message m JOIN ranked r ON r.Id = m.Id;
        ;WITH lastSeq AS (
            SELECT b.SessionId, MAX(m.SessionSeq) AS Seq
            FROM #batch b JOIN dbo.Chat_Message m ON m.Id=b.Id GROUP BY b.SessionId
        )
        UPDATE s SET LastMessageSeq=q.Seq FROM dbo.Chat_Session s JOIN lastSeq q ON q.SessionId=s.Id;
        DECLARE @count int, @next bigint;
        SELECT @count=COUNT(*), @next=COALESCE(MAX(Id), @cursor) FROM #batch;
        DROP TABLE #batch;
        SELECT @count, @next;
        """;

    private const string CheckSql = """
        SET NOCOUNT ON;
        SELECT (SELECT COUNT(*) FROM dbo.__EFMigrationsHistory WHERE MigrationId=N'20261109093244_MessageSessionSeq'),
               COUNT_BIG(*), COALESCE(SUM(CONVERT(bigint, CASE WHEN SessionSeq=0 THEN 1 ELSE 0 END)),0),
               COALESCE(SUM(CONVERT(bigint, CASE WHEN SessionSeq>0 THEN 1 ELSE 0 END)),0),
               COALESCE(SUM(CONVERT(bigint, CASE WHEN SessionSeq IS NULL OR SessionSeq<0 OR SessionId IS NULL THEN 1 ELSE 0 END)),0),
               COALESCE(CHECKSUM_AGG(BINARY_CHECKSUM(Id, SessionId, MessageType, MessageContentId, IsDeleted, IsRollbacked,
                   HASHBYTES('SHA2_256', CONVERT(varbinary(max), ContentJson)))),0)
        FROM dbo.Chat_Message;
        SELECT COUNT_BIG(*) FROM dbo.Chat_Message m LEFT JOIN dbo.Chat_Session s ON s.Id=m.SessionId WHERE s.Id IS NULL;
        ;WITH ranked AS (
          SELECT SessionSeq, ROW_NUMBER() OVER (PARTITION BY SessionId ORDER BY Id) AS Expected,
                 SUM(CONVERT(bigint, CASE WHEN SessionSeq=0 THEN 1 ELSE 0 END)) OVER (PARTITION BY SessionId ORDER BY Id ROWS UNBOUNDED PRECEDING) AS EarlierZeros
          FROM dbo.Chat_Message
        ) SELECT COUNT_BIG(*) FROM ranked WHERE SessionSeq>0 AND (SessionSeq<>Expected OR EarlierZeros<>0);
        ;WITH maxima AS (SELECT SessionId, MAX(SessionSeq) AS Seq, COUNT_BIG(*) AS N FROM dbo.Chat_Message GROUP BY SessionId)
        SELECT COUNT_BIG(*), COALESCE(SUM(CONVERT(bigint, CASE WHEN s.LastMessageSeq<>COALESCE(m.Seq,0) THEN 1 ELSE 0 END)),0),
               COALESCE(CHECKSUM_AGG(BINARY_CHECKSUM(s.Id, COALESCE(m.N,0))),0)
        FROM dbo.Chat_Session s LEFT JOIN maxima m ON s.Id=m.SessionId;
        SELECT COUNT_BIG(*) FROM (SELECT SessionId, SessionSeq FROM dbo.Chat_Message WHERE SessionSeq>0 GROUP BY SessionId, SessionSeq HAVING COUNT_BIG(*)>1) d;
        """;
}
