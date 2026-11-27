using IczpNet.Chat.Blobs;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Options;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Modularity;
using Volo.Abp.Timing;

namespace IczpNet.Chat.SchemaMigrator;

[DependsOn(
    typeof(ChatDomainModule),
    typeof(ChatEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule)
)]
internal sealed class LegacyPublicBlobAuditModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var config = context.Services.GetConfiguration();
        var conn = config.GetConnectionString("Chat")!;

        Configure<AbpClockOptions>(options =>
        {
            options.Kind = DateTimeKind.Utc;
        });

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = conn;
            options.ConnectionStrings["Chat"] = conn;
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(ctx => ctx.UseSqlServer());
        });

        Configure<AttachmentSignatureOptions>(options =>
        {
            options.SigningKey = config["AttachmentSignature:SigningKey"]
                ?? "SchemaMigrator-Audit-Only-Key-Do-Not-Use-For-Tokens-001";
            options.DefaultExpireSeconds = 900;
        });
    }
}

public static class LegacyPublicBlobAuditCommand
{
    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var valid = args.All(x =>
                x is "--audit-legacy-public-blobs" or "--dry-run" or "--apply"
                || x.StartsWith("--batch-size=", StringComparison.Ordinal)
                || x.StartsWith("--after-blob-id=", StringComparison.Ordinal));
            if (!valid || args.Distinct().Count() != args.Length)
            {
                throw new InvalidOperationException(
                    "Invalid arguments. Usage: --audit-legacy-public-blobs [--dry-run | --apply] [--batch-size=200] [--after-blob-id=<guid>]");
            }

            var isApply = args.Contains("--apply");
            var isDryRunFlag = args.Contains("--dry-run");
            if (isApply && isDryRunFlag)
            {
                throw new InvalidOperationException("Specify at most one of --dry-run or --apply.");
            }

            var dryRun = !isApply;

            var batchSize = 200;
            var batchArg = args.FirstOrDefault(x => x.StartsWith("--batch-size=", StringComparison.Ordinal));
            if (batchArg != null)
            {
                if (!int.TryParse(batchArg["--batch-size=".Length..], out batchSize) || batchSize < 1 || batchSize > 1000)
                {
                    throw new InvalidOperationException("--batch-size must be between 1 and 1000.");
                }
            }

            Guid? afterBlobId = null;
            var cursorArg = args.FirstOrDefault(x => x.StartsWith("--after-blob-id=", StringComparison.Ordinal));
            if (cursorArg != null)
            {
                if (!Guid.TryParse(cursorArg["--after-blob-id=".Length..], out var parsedCursor))
                {
                    throw new InvalidOperationException("--after-blob-id must be a valid GUID.");
                }
                afterBlobId = parsedCursor;
            }

            var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Chat")
                ?? Environment.GetEnvironmentVariable("CHAT_CONNECTION_STRING");
            var expectedDb = Environment.GetEnvironmentVariable("ChatMigration__ExpectedDatabase")
                ?? Environment.GetEnvironmentVariable("CHAT_MIGRATION_EXPECTED_DATABASE")
                ?? "Chat_Module_v3";
            if (string.IsNullOrWhiteSpace(cs))
            {
                throw new InvalidOperationException("ConnectionStrings__Chat or CHAT_CONNECTION_STRING is required.");
            }

            var target = new SqlConnectionStringBuilder(cs);
            if (string.IsNullOrWhiteSpace(target.InitialCatalog)
                || !target.InitialCatalog.Equals(expectedDb, StringComparison.OrdinalIgnoreCase)
                || new[] { "master", "model", "msdb", "tempdb" }.Contains(target.InitialCatalog, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Database '{target.InitialCatalog}' does not match expected '{expectedDb}'.");
            }

            await using var lockConn = new SqlConnection(cs);
            await lockConn.OpenAsync();
            await using (var appLock = new SqlCommand(
                "DECLARE @r int; EXEC @r = sp_getapplock @Resource=N'Chat.LegacyPublicBlob.Audit', @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=0; SELECT @r;",
                lockConn))
            {
                var lockResult = Convert.ToInt32(await appLock.ExecuteScalarAsync());
                if (lockResult < 0)
                {
                    throw new InvalidOperationException("Another legacy public blob audit runner holds the exclusive session lock.");
                }
            }

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = cs,
                    ["ConnectionStrings:Chat"] = cs,
                    ["AttachmentSignature:SigningKey"] =
                        Environment.GetEnvironmentVariable("AttachmentSignature__SigningKey")
                        ?? "SchemaMigrator-Audit-Only-Key-Do-Not-Use-For-Tokens-001",
                })
                .Build();

            using var app = await AbpApplicationFactory.CreateAsync<LegacyPublicBlobAuditModule>(options =>
            {
                options.UseAutofac();
                options.Services.ReplaceConfiguration(config);
            });
            await app.InitializeAsync();

            var auditService = app.ServiceProvider.GetRequiredService<ILegacyPublicBlobAuditService>();

            long totalCandidates = 0;
            long totalConfirmed = 0;
            long totalMissingRefs = 0;
            long totalUnclassified = 0;
            long totalBackfilled = 0;
            long totalPrivatized = 0;
            var batchIndex = 0;
            var cursor = afterBlobId;

            Console.WriteLine(
                $"[LegacyPublicBlobAudit] Server={target.DataSource}; Database={target.InitialCatalog}; Mode={(dryRun ? "DRY-RUN" : "APPLY")}; BatchSize={batchSize}; StartAfter={cursor}");

            while (true)
            {
                var batch = await auditService.AuditAndMigrateBatchAsync(cursor, batchSize, dryRun);
                if (batch.CandidateCount == 0)
                {
                    break;
                }

                batchIndex++;
                totalCandidates += batch.CandidateCount;
                totalConfirmed += batch.ConfirmedChatCount;
                totalMissingRefs += batch.MissingRefCount;
                totalUnclassified += batch.UnclassifiedCount;
                totalBackfilled += batch.BackfilledRefCount;
                totalPrivatized += batch.PrivatizedCount;
                cursor = batch.LastProcessedBlobId;

                Console.WriteLine(
                    $"[Batch {batchIndex}] Candidates={batch.CandidateCount}, ConfirmedChat={batch.ConfirmedChatCount}, MissingRefs={batch.MissingRefCount}, Unclassified={batch.UnclassifiedCount}, Backfilled={batch.BackfilledRefCount}, Privatized={batch.PrivatizedCount}, LastId={cursor}, HasMore={batch.HasMore}");

                if (!batch.HasMore || !cursor.HasValue)
                {
                    break;
                }
            }

            Console.WriteLine(
                $"[Summary] Mode={(dryRun ? "DRY-RUN" : "APPLY")}; Batches={batchIndex}; Candidates={totalCandidates}; ConfirmedChat={totalConfirmed}; MissingRefs={totalMissingRefs}; Unclassified={totalUnclassified}; Backfilled={totalBackfilled}; Privatized={totalPrivatized}");

            await app.ShutdownAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Legacy public blob audit failed: {ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }
}
