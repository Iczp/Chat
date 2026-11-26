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
        });
    }
}
