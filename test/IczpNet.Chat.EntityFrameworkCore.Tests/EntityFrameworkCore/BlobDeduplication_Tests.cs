using IczpNet.Chat.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Xunit;

namespace IczpNet.Chat.EntityFrameworkCore;

public class BlobDeduplication_Tests : ChatEntityFrameworkCoreTestBase
{
    private sealed class InMemoryBlobContainer : IBlobContainer
    {
        public ConcurrentDictionary<string, byte[]> Store { get; } = new(StringComparer.Ordinal);
        public List<string> SavedKeys { get; } = [];
        public List<string> DeletedKeys { get; } = [];

        public Task SaveAsync(string name, Stream stream, bool overrideExisting = false, CancellationToken cancellationToken = default)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            Store[name] = ms.ToArray();
            SavedKeys.Add(name);
            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(string name, CancellationToken cancellationToken = default)
        {
            DeletedKeys.Add(name);
            return Task.FromResult(Store.TryRemove(name, out _));
        }

        public Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
            => Task.FromResult(Store.ContainsKey(name));

        public Task<Stream> GetAsync(string name, CancellationToken cancellationToken = default)
            => Task.FromResult<Stream>(new MemoryStream(Store[name]));

        public Task<Stream> GetOrNullAsync(string name, CancellationToken cancellationToken = default)
            => Task.FromResult<Stream>(Store.TryGetValue(name, out var bytes) ? new MemoryStream(bytes) : null);
    }

    private (BlobManager Manager, TempBlobCleanupService Cleanup, InMemoryBlobContainer Container) CreateServices()
    {
        var inMemory = new InMemoryBlobContainer();
        var factory = Substitute.For<IBlobContainerFactory>();
        factory.Create(Arg.Any<string>()).Returns(inMemory);

        var lazy = ServiceProvider.GetRequiredService<IAbpLazyServiceProvider>();
        var blobRepo = ServiceProvider.GetRequiredService<IRepository<Blob, Guid>>();
        var objRepo = ServiceProvider.GetRequiredService<IRepository<BlobObject, Guid>>();
        var distLock = ServiceProvider.GetRequiredService<IAbpDistributedLock>();

        var manager = new BlobManager(factory, blobRepo, objRepo, distLock)
        {
            LazyServiceProvider = lazy
        };
        var cleanup = new TempBlobCleanupService(blobRepo, objRepo, factory, distLock)
        {
            LazyServiceProvider = lazy
        };
        return (manager, cleanup, inMemory);
    }

    [Fact]
    public async Task Duplicate_uploads_share_one_physical_BlobObject_with_isolated_logical_Blobs()
    {
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var clock = ServiceProvider.GetRequiredService<IClock>();
        var payload = Encoding.UTF8.GetBytes("shared-attachment-payload-bytes-001");
        var blobIdA = guidGen.Create();
        var blobIdB = guidGen.Create();

        var (_, _, container) = await WithUnitOfWorkAsync(async () =>
        {
            var (manager, _, mem) = CreateServices();
            var db = await ServiceProvider.GetRequiredService<Volo.Abp.EntityFrameworkCore.IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            var blobA = await manager.CreateAsync(new Blob(blobIdA, "chat-files")
            {
                Name = "2026/11/alice-file.txt",
                FileName = "alice-doc.txt",
                MimeType = "text/plain",
                Suffix = ".txt",
                FileSize = payload.Length,
                Bytes = payload,
                UploaderChatObjectId = 1001L,
                Status = BlobStatus.Temp,
                ExpireTime = clock.Now.AddHours(2),
            });

            var blobB = await manager.CreateAsync(new Blob(blobIdB, "chat-files")
            {
                Name = "2026/11/bob-file.txt",
                FileName = "bob-copy.txt",
                MimeType = "text/plain",
                Suffix = ".txt",
                FileSize = payload.Length,
                Bytes = payload,
                UploaderChatObjectId = 1002L,
                Status = BlobStatus.Temp,
                ExpireTime = clock.Now.AddHours(2),
            });

            Assert.NotNull(blobA.StorageObjectId);
            Assert.Equal(blobA.StorageObjectId, blobB.StorageObjectId);
            Assert.NotEqual(blobA.Id, blobB.Id);
            Assert.Equal("alice-doc.txt", blobA.FileName);
            Assert.Equal("bob-copy.txt", blobB.FileName);
            Assert.Equal(1001L, blobA.UploaderChatObjectId);
            Assert.Equal(1002L, blobB.UploaderChatObjectId);
            Assert.Single(mem.SavedKeys);

            var storageObj = await db.BlobObject.AsNoTracking().SingleAsync(x => x.Id == blobA.StorageObjectId.Value);
            Assert.Equal(BlobObjectStatus.Ready, storageObj.Status);
            Assert.Equal(2L, storageObj.RefCount);
            Assert.Equal(payload.LongLength, storageObj.FileSize);

            var bytesA = await manager.GetBytesAsync(blobIdA);
            var bytesB = await manager.GetBytesAsync(blobIdB);
            Assert.Equal(payload, bytesA);
            Assert.Equal(payload, bytesB);

            return (manager, (TempBlobCleanupService)null!, mem);
        });

        Assert.Single(container.SavedKeys);
    }

    [Fact]
    public async Task Legacy_blob_without_StorageObjectId_still_reads_from_container_and_name()
    {
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var legacyBlobId = guidGen.Create();
        var legacyBytes = Encoding.UTF8.GetBytes("legacy-blob-content");

        await WithUnitOfWorkAsync(async () =>
        {
            var (manager, _, mem) = CreateServices();
            var db = await ServiceProvider.GetRequiredService<Volo.Abp.EntityFrameworkCore.IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = legacyBlobId,
                ["Container"] = "chat-files",
                ["Name"] = "2024/01/old.bin",
                ["FileName"] = "old.bin",
                ["MimeType"] = "application/octet-stream",
                ["Status"] = BlobStatus.Bound,
                ["RefCount"] = 1
            });

            mem.Store["2024/01/old.bin"] = legacyBytes;

            var blob = await manager.GetAsync(legacyBlobId);
            var location = await manager.ResolvePhysicalLocationAsync(blob);
            Assert.Equal("chat-files", location.Container);
            Assert.Equal("2024/01/old.bin", location.StorageName);
            Assert.Equal(legacyBytes, await manager.GetBytesAsync(legacyBlobId));
        });
    }

    [Fact]
    public async Task Cleanup_decrements_shared_BlobObject_and_deletes_physical_file_only_when_last_ref_expires()
    {
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var clock = ServiceProvider.GetRequiredService<IClock>();
        var payload = Encoding.UTF8.GetBytes("cleanup-dedup-bytes-999");
        var expiredId = guidGen.Create();
        var activeId = guidGen.Create();
        var stalePendingObjId = guidGen.Create();
        Guid storageObjectId = Guid.Empty;
        InMemoryBlobContainer sharedMem = null!;

        await WithUnitOfWorkAsync(async () =>
        {
            var (manager, cleanup, mem) = CreateServices();
            sharedMem = mem;
            var db = await ServiceProvider.GetRequiredService<Volo.Abp.EntityFrameworkCore.IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            var expiredBlob = await manager.CreateAsync(new Blob(expiredId, "chat-files")
            {
                Name = "2026/11/t1.bin",
                FileName = "t1.bin",
                MimeType = "application/octet-stream",
                Suffix = ".bin",
                Bytes = payload,
                Status = BlobStatus.Temp,
                ExpireTime = clock.Now.AddMinutes(-10),
                RefCount = 0,
            });
            var activeBlob = await manager.CreateAsync(new Blob(activeId, "chat-files")
            {
                Name = "2026/11/t2.bin",
                FileName = "t2.bin",
                MimeType = "application/octet-stream",
                Suffix = ".bin",
                Bytes = payload,
                Status = BlobStatus.Temp,
                ExpireTime = clock.Now.AddHours(4),
                RefCount = 0,
            });
            storageObjectId = expiredBlob.StorageObjectId!.Value;

            // Insert a stale Pending orphan (>2h old) to confirm orphan recovery.
            await MessageVisibilityQuery_Tests.InsertAsync<BlobObject>(db, new()
            {
                ["Id"] = stalePendingObjId,
                ["StorageScope"] = "host",
                ["Container"] = "chat-files",
                ["StorageName"] = "objects/ff/00/stale_pending.bin",
                ["Sha256"] = new string('f', 64),
                ["FileSize"] = 123L,
                ["Status"] = BlobObjectStatus.Pending,
                ["RefCount"] = 0L,
                ["CreationTime"] = clock.Now.AddHours(-3),
            });
            sharedMem.Store["objects/ff/00/stale_pending.bin"] = [1, 2, 3];

            var removedCount = await cleanup.CleanupExpiredTempBlobsAsync();
            Assert.Equal(1, removedCount);
            Assert.Null(await db.Blob.AsNoTracking().FirstOrDefaultAsync(x => x.Id == expiredId));
            Assert.NotNull(await db.Blob.AsNoTracking().FirstOrDefaultAsync(x => x.Id == activeId));
            Assert.Null(await db.BlobObject.AsNoTracking().FirstOrDefaultAsync(x => x.Id == stalePendingObjId));

            var remainingObj = await db.BlobObject.AsNoTracking().SingleAsync(x => x.Id == storageObjectId);
            Assert.Equal(1L, remainingObj.RefCount);
            Assert.Equal(BlobObjectStatus.Ready, remainingObj.Status);
            Assert.True(sharedMem.Store.ContainsKey(remainingObj.StorageName));
            Assert.Equal(payload, await manager.GetBytesAsync(activeId));

            // Now expire the last Temp blob and run cleanup again using the same in-memory container.
            var trackedActive = await db.Blob.SingleAsync(x => x.Id == activeId);
            trackedActive.ExpireTime = clock.Now.AddMinutes(-1);
            await db.SaveChangesAsync();

            var secondRemoved = await cleanup.CleanupExpiredTempBlobsAsync();
            Assert.Equal(1, secondRemoved);
            Assert.Null(await db.Blob.AsNoTracking().FirstOrDefaultAsync(x => x.Id == activeId));
            Assert.Null(await db.BlobObject.AsNoTracking().FirstOrDefaultAsync(x => x.Id == storageObjectId));
            Assert.Empty(sharedMem.Store);

            // Re-uploading the exact same payload after soft-deleting the prior BlobObject must succeed.
            var reuploadId = guidGen.Create();
            var reuploaded = await manager.CreateAsync(new Blob(reuploadId, "chat-files")
            {
                Name = "2026/11/reupload.bin",
                FileName = "reupload.bin",
                MimeType = "application/octet-stream",
                Suffix = ".bin",
                Bytes = payload,
                Status = BlobStatus.Temp,
                ExpireTime = clock.Now.AddHours(4),
                RefCount = 0,
            });
            Assert.NotNull(reuploaded.StorageObjectId);
            Assert.NotEqual(storageObjectId, reuploaded.StorageObjectId!.Value);
            Assert.Equal(payload, await manager.GetBytesAsync(reuploadId));
            var newObj = await db.BlobObject.AsNoTracking().SingleAsync(x => x.Id == reuploaded.StorageObjectId.Value);
            Assert.Equal(BlobObjectStatus.Ready, newObj.Status);
            Assert.Equal(1L, newObj.RefCount);
        });
    }

    [Fact]
    public async Task Conditional_claim_bumps_ConcurrencyStamp_and_never_deletes_concurrently_bound_blob()
    {
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var clock = ServiceProvider.GetRequiredService<IClock>();
        var msgRepo = ServiceProvider.GetRequiredService<IczpNet.Chat.MessageSections.Messages.IMessageRepository>();
        var blobId = guidGen.Create();
        var sessionId = guidGen.Create();
        var initialStamp = Guid.NewGuid().ToString("N");

        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<Volo.Abp.EntityFrameworkCore.IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = blobId,
                ["Container"] = "chat-files",
                ["Name"] = "2026/11/race-bind.bin",
                ["FileName"] = "race-bind.bin",
                ["MimeType"] = "application/octet-stream",
                ["Suffix"] = ".bin",
                ["Status"] = BlobStatus.Temp,
                ["ExpireTime"] = clock.Now.AddMinutes(30),
                ["RefCount"] = 0,
                ["ConcurrencyStamp"] = initialStamp,
            });
            await db.SaveChangesAsync();

            var boundCount = await msgRepo.BindBlobsToMessageAsync(
                91001L,
                sessionId,
                [blobId],
                BlobRefSourceTypes.Send,
                now: clock.Now,
                markAsBound: true);
            Assert.Equal(1, boundCount);

            var boundBlob = await db.Blob.AsNoTracking().SingleAsync(x => x.Id == blobId);
            Assert.Equal(BlobStatus.Bound, boundBlob.Status);
            Assert.Null(boundBlob.ExpireTime);
            Assert.NotEqual(initialStamp, boundBlob.ConcurrencyStamp);

            // Even with a cleanup timestamp in the far future, the bound blob must NOT be claimed for deletion.
            var claimed = await msgRepo.ClaimExpiredTempBlobsAsync(clock.Now.AddDays(30), 100);
            Assert.DoesNotContain(claimed, c => c.Id == blobId);
            Assert.NotNull(await db.Blob.AsNoTracking().FirstOrDefaultAsync(x => x.Id == blobId));
        });
    }

    [Fact]
    public async Task Rolled_back_caller_UOW_leaves_zero_RefCount_and_cleanup_reconciles_positive_RefCount_orphans()
    {
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var clock = ServiceProvider.GetRequiredService<IClock>();
        var uowManager = ServiceProvider.GetRequiredService<Volo.Abp.Uow.IUnitOfWorkManager>();
        var payload = Encoding.UTF8.GetBytes("rollback-orphan-reconcile-bytes-002");

        var blobIdA = guidGen.Create();
        var rolledBackBlobId = guidGen.Create();
        Guid? sharedObjectId = null;

        // 1. First upload commits normally (RefCount == 1)
        await WithUnitOfWorkAsync(async () =>
        {
            var (manager, _, _) = CreateServices();
            var db = await ServiceProvider.GetRequiredService<Volo.Abp.EntityFrameworkCore.IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            var blobA = await manager.CreateAsync(new Blob(blobIdA, "chat-files")
            {
                Name = "2026/11/commit-a.bin",
                FileName = "commit-a.bin",
                MimeType = "application/octet-stream",
                Suffix = ".bin",
                Bytes = payload,
                Status = BlobStatus.Temp,
                ExpireTime = clock.Now.AddHours(2),
            });
            sharedObjectId = blobA.StorageObjectId;
        });

        Assert.NotNull(sharedObjectId);

        // 2. Second CreateAsync inside an aborted UOW must NOT leak RefCount increment (stays 1).
        using (var abortedUow = uowManager.Begin(requiresNew: true, isTransactional: true))
        {
            var (manager, _, _) = CreateServices();
            var db = await ServiceProvider.GetRequiredService<Volo.Abp.EntityFrameworkCore.IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await manager.CreateAsync(new Blob(rolledBackBlobId, "chat-files")
            {
                Name = "2026/11/aborted-b.bin",
                FileName = "aborted-b.bin",
                MimeType = "application/octet-stream",
                Suffix = ".bin",
                StorageObjectId = sharedObjectId,
                Status = BlobStatus.Temp,
                ExpireTime = clock.Now.AddHours(2),
            });
            // 故意不调用 CompleteAsync，让逻辑 Blob 写入与 RefCount+1 一同回滚
        }

        using (var checkUow = uowManager.Begin(requiresNew: true, isTransactional: false))
        {
            var db = await ServiceProvider.GetRequiredService<Volo.Abp.EntityFrameworkCore.IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            Assert.Null(await db.Blob.AsNoTracking().FirstOrDefaultAsync(x => x.Id == rolledBackBlobId));
            var obj = await db.BlobObject.AsNoTracking().SingleAsync(x => x.Id == sharedObjectId.Value);
            Assert.Equal(1L, obj.RefCount);
            await checkUow.CompleteAsync();
        }

        // 3. Pre-existing legacy positive-RefCount Ready orphan (>2h old with 0 logical Blobs) must be cleaned up.
        var orphanObjId = guidGen.Create();
        await WithUnitOfWorkAsync(async () =>
        {
            var (_, cleanup, mem) = CreateServices();
            var db = await ServiceProvider.GetRequiredService<Volo.Abp.EntityFrameworkCore.IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await MessageVisibilityQuery_Tests.InsertAsync<BlobObject>(db, new()
            {
                ["Id"] = orphanObjId,
                ["StorageScope"] = "host",
                ["Container"] = "chat-files",
                ["StorageName"] = "objects/aa/bb/orphan_ready_positive.bin",
                ["Sha256"] = new string('e', 64),
                ["FileSize"] = 456L,
                ["Status"] = BlobObjectStatus.Ready,
                ["RefCount"] = 3L,
                ["CreationTime"] = clock.Now.AddHours(-3),
                ["LastVerifiedTime"] = clock.Now.AddHours(-3),
            });
            await db.SaveChangesAsync();
            mem.Store["objects/aa/bb/orphan_ready_positive.bin"] = [9, 8, 7];

            await cleanup.CleanupExpiredTempBlobsAsync();
            Assert.Null(await db.BlobObject.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orphanObjId));
            Assert.False(mem.Store.ContainsKey("objects/aa/bb/orphan_ready_positive.bin"));
        });
    }
}
