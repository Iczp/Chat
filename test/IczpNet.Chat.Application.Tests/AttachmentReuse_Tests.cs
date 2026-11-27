using IczpNet.Chat.Blobs;
using IczpNet.Chat.Blobs.Dtos;
using IczpNet.Chat.Blobs.Validation;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Controllers;
using IczpNet.Chat.Medias;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Json;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

public class AttachmentReuse_Tests
{
    private sealed class TestSetup
    {
        public Guid AccountUserId { get; init; } = Guid.NewGuid();
        public Guid SessionId { get; } = Guid.NewGuid();
        public Guid UnitAId { get; } = Guid.NewGuid();
        public Guid UnitBId { get; } = Guid.NewGuid();
        public Guid DisabledUnitId { get; } = Guid.NewGuid();
        public List<Blob> StoredBlobs { get; } = [];
        public List<BlobReuseRecord> ReuseRecords { get; } = [];
        public Dictionary<string, UploadAttachmentOutput> CacheStore { get; } = new(StringComparer.Ordinal);
        public int PhysicalSaveCount { get; set; }
        public long StorageObjectRefCount { get; set; }
        public DateTime Now { get; set; } = new(2026, 11, 26, 12, 0, 0, DateTimeKind.Utc);
        public AttachmentReuseTokenService TokenService { get; private set; } = null!;

        public AttachmentController CreateController(
            Guid currentUserId,
            long[] ownedChatObjectIds,
            bool enableAttachmentReuse = true,
            bool enablePhysicalDeduplication = true)
        {
            var clock = Substitute.For<IClock>();
            clock.Now.Returns(_ => Now);

            var sigOptions = Microsoft.Extensions.Options.Options.Create(new AttachmentSignatureOptions
            {
                SigningKey = "Reuse.Test.Signing.Key.2026",
                DefaultExpireSeconds = 900,
            });
            var uploadOptions = Microsoft.Extensions.Options.Options.Create(new AttachmentUploadOptions
            {
                TempBlobExpireHours = 24,
                EnableAttachmentReuse = enableAttachmentReuse,
                EnablePhysicalDeduplication = enablePhysicalDeduplication,
            });
            var msgOptions = Microsoft.Extensions.Options.Options.Create(new MessageOptions());

            TokenService = new AttachmentReuseTokenService(sigOptions, uploadOptions, clock);

            var chatObjectManager = Substitute.For<IChatObjectManager>();
            chatObjectManager.GetIdListByUserIdAsync(currentUserId).Returns(ownedChatObjectIds.ToList());

            var unitA = new SessionUnitCacheItem { Id = UnitAId, OwnerId = 101L, SessionId = SessionId, IsEnabled = true, IsInputEnabled = true };
            var unitB = new SessionUnitCacheItem { Id = UnitBId, OwnerId = 102L, SessionId = SessionId, IsEnabled = true, IsInputEnabled = true };
            var unitDisabled = new SessionUnitCacheItem { Id = DisabledUnitId, OwnerId = 102L, SessionId = SessionId, IsEnabled = true, IsInputEnabled = false };

            var sessionUnitManager = Substitute.For<ISessionUnitManager>();
            sessionUnitManager.GetCacheAsync(UnitAId).Returns(unitA);
            sessionUnitManager.GetCacheAsync(UnitBId).Returns(unitB);
            sessionUnitManager.GetCacheAsync(DisabledUnitId).Returns(unitDisabled);

            var validatorManager = Substitute.For<IAttachmentUploadValidatorManager>();
            validatorManager.ValidateAsync(Arg.Any<AttachmentUploadValidationContext>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            var storageObjId = Guid.NewGuid();
            var sharedObj = new BlobObject(storageObjId, "host", "chat-files", "objects/aa/bb/shared.pdf", new string('a', 64), 12, "application/pdf")
            {
                Status = BlobObjectStatus.Ready,
                RefCount = StorageObjectRefCount
            };

            var blobManager = Substitute.For<IBlobManager>();
            blobManager.CreateAsync(Arg.Any<Blob>()).Returns(call =>
            {
                var b = call.Arg<Blob>();
                if (!b.StorageObjectId.HasValue)
                {
                    PhysicalSaveCount++;
                    StorageObjectRefCount++;
                    sharedObj.RefCount = StorageObjectRefCount;
                    b.SetStorageObject(sharedObj);
                }
                else
                {
                    StorageObjectRefCount++;
                    sharedObj.RefCount = StorageObjectRefCount;
                    b.SetStorageObject(sharedObj);
                }
                StoredBlobs.Add(b);
                return Task.FromResult(b);
            });
            blobManager.ResolvePhysicalLocationAsync(Arg.Any<Blob>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    var b = call.Arg<Blob>();
                    return Task.FromResult((b.Container ?? "chat-files", b.StorageObject?.StorageName ?? b.Name));
                });
            blobManager.GetBytesAsync(Arg.Any<Guid>()).Returns([1, 2, 3, 4]);

            var blobRepository = Substitute.For<IRepository<Blob, Guid>>();
            blobRepository.FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call => StoredBlobs.FirstOrDefault(x => x.Id == call.Arg<Guid>()));
            blobRepository.GetQueryableAsync()
                .Returns(_ => Task.FromResult(StoredBlobs.AsQueryable()));

            var reuseCache = Substitute.For<IDistributedCache<UploadAttachmentOutput, string>>();
            reuseCache.GetAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call => CacheStore.TryGetValue(call.Arg<string>(), out var v) ? v : null);
            reuseCache.SetAsync(Arg.Any<string>(), Arg.Any<UploadAttachmentOutput>(), Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    CacheStore[call.Arg<string>()] = call.Arg<UploadAttachmentOutput>();
                    return Task.CompletedTask;
                });

            var reuseRecordRepo = Substitute.For<IRepository<BlobReuseRecord, Guid>>();
            reuseRecordRepo.FindAsync(Arg.Any<Expression<Func<BlobReuseRecord, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call => ReuseRecords.AsQueryable().FirstOrDefault(call.Arg<Expression<Func<BlobReuseRecord, bool>>>()));
            reuseRecordRepo.InsertAsync(Arg.Any<BlobReuseRecord>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    var rec = call.Arg<BlobReuseRecord>();
                    ReuseRecords.Add(rec);
                    return Task.FromResult(rec);
                });
            reuseRecordRepo.DeleteAsync(Arg.Any<BlobReuseRecord>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    ReuseRecords.Remove(call.Arg<BlobReuseRecord>());
                    return Task.CompletedTask;
                });

            var jsonSerializer = Substitute.For<IJsonSerializer>();
            jsonSerializer.Serialize(Arg.Any<object>(), Arg.Any<bool>(), Arg.Any<bool>())
                .Returns(call => System.Text.Json.JsonSerializer.Serialize(call.Arg<object>()));
            jsonSerializer.Deserialize<UploadAttachmentOutput>(Arg.Any<string>(), Arg.Any<bool>())
                .Returns(call => System.Text.Json.JsonSerializer.Deserialize<UploadAttachmentOutput>(call.Arg<string>())!);

            var currentUser = Substitute.For<ICurrentUser>();
            currentUser.Id.Returns(currentUserId);

            var guidGen = Substitute.For<IGuidGenerator>();
            guidGen.Create().Returns(_ => Guid.NewGuid());

            var blobResolver = Substitute.For<IBlobResolver>();
            blobResolver.GetDirectoryNameAsync(Arg.Any<BlobContext>()).Returns("2026/11/files");
            blobResolver.GenerateFileNameAsync(Arg.Any<string>()).Returns(call => $"f_{Guid.NewGuid():N}{call.Arg<string>()}");
            blobResolver.GetFileUrlAsync(Arg.Any<Guid>(), Arg.Any<string>()).Returns(call => $"/api/chat/file/{call.Arg<Guid>()}");

            var controller = new AttachmentController(
                sessionUnitManager,
                chatObjectManager,
                validatorManager,
                Substitute.For<IMediaResolver>(),
                msgOptions,
                uploadOptions,
                jsonSerializer);

            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            lazy.LazyGetRequiredService<ICurrentUser>().Returns(currentUser);
            lazy.LazyGetRequiredService<IBlobManager>().Returns(blobManager);
            lazy.LazyGetRequiredService<IBlobResolver>().Returns(blobResolver);
            lazy.LazyGetRequiredService<IClock>().Returns(clock);
            lazy.LazyGetRequiredService<IGuidGenerator>().Returns(guidGen);
            lazy.LazyGetService<IGuidGenerator>().Returns(guidGen);
            lazy.LazyGetService<IGuidGenerator>(Arg.Any<IGuidGenerator>()).Returns(guidGen);
            lazy.LazyGetRequiredService<IAttachmentReuseTokenService>().Returns(TokenService);
            lazy.LazyGetRequiredService<IRepository<Blob, Guid>>().Returns(blobRepository);
            lazy.LazyGetRequiredService<IRepository<BlobReuseRecord, Guid>>().Returns(reuseRecordRepo);
            lazy.LazyGetRequiredService<IDistributedCache<UploadAttachmentOutput, string>>().Returns(reuseCache);
            var uowManager = Substitute.For<Volo.Abp.Uow.IUnitOfWorkManager>();
            uowManager.Current.Returns((Volo.Abp.Uow.IUnitOfWork)null);
            lazy.LazyGetRequiredService<Volo.Abp.Uow.IUnitOfWorkManager>().Returns(uowManager);
            ReuseRecordRepository = reuseRecordRepo;
            UowManager = uowManager;
            controller.LazyServiceProvider = lazy;
            return controller;
        }

        public IRepository<BlobReuseRecord, Guid> ReuseRecordRepository { get; private set; }
        public Volo.Abp.Uow.IUnitOfWorkManager UowManager { get; private set; }
    }

    private static IFormFile CreateFormFile(string fileName, string contentType, byte[] content)
        => new FormFile(new MemoryStream(content), 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };

    [Fact]
    public async Task Same_account_second_identity_reuses_upload_without_extra_physical_write_and_is_idempotent_by_ClientUploadId()
    {
        var setup = new TestSetup();
        var controller = setup.CreateController(setup.AccountUserId, [101L, 102L]);
        var content = Encoding.UTF8.GetBytes("multi-identity-shared-pdf");
        var formFile = CreateFormFile("report.pdf", "application/pdf", content);

        var uploadA = await controller.UploadFileAsync(setup.UnitAId, formFile);
        Assert.NotNull(uploadA.ReuseToken);
        Assert.Equal(1, setup.PhysicalSaveCount);
        Assert.Equal(1L, setup.StorageObjectRefCount);

        // Cross-ChatObject reuse under the same user account must be rejected.
        await Assert.ThrowsAsync<AbpAuthorizationException>(() => controller.ReuseAsync(setup.UnitBId, new ReuseAttachmentInput
        {
            ReuseToken = uploadA.ReuseToken,
            ClientUploadId = "cross-co-001",
            FileName = "shared-report.pdf"
        }));

        // Same ChatObject can redeem its own ReuseToken without extra physical write.
        var reuseA2 = await controller.ReuseAsync(setup.UnitAId, new ReuseAttachmentInput
        {
            ReuseToken = uploadA.ReuseToken,
            ClientUploadId = "client-op-001",
            FileName = "shared-report.pdf"
        });

        Assert.NotEqual(uploadA.BlobId, reuseA2.BlobId);
        Assert.Equal("shared-report.pdf", reuseA2.FileName);
        Assert.Equal(1, setup.PhysicalSaveCount);
        Assert.Equal(2L, setup.StorageObjectRefCount);

        var blobA = setup.StoredBlobs.Single(x => x.Id == uploadA.BlobId);
        var blobA2 = setup.StoredBlobs.Single(x => x.Id == reuseA2.BlobId);
        Assert.Equal(blobA.StorageObjectId, blobA2.StorageObjectId);
        Assert.Equal(101L, blobA.UploaderChatObjectId);
        Assert.Equal(101L, blobA2.UploaderChatObjectId);
        Assert.Equal(BlobStatus.Temp, blobA2.Status);

        // Retry with the same ClientUploadId should be idempotent.
        var retryA2 = await controller.ReuseAsync(setup.UnitAId, new ReuseAttachmentInput
        {
            ReuseToken = uploadA.ReuseToken,
            ClientUploadId = "client-op-001",
            FileName = "shared-report.pdf"
        });
        Assert.Equal(reuseA2.BlobId, retryA2.BlobId);
        Assert.Equal(2L, setup.StorageObjectRefCount);

        // Clearing Redis cache must still hit persistent DB BlobReuseRecord with zero extra Temp blobs.
        setup.CacheStore.Clear();
        var retryFromDb = await controller.ReuseAsync(setup.UnitAId, new ReuseAttachmentInput
        {
            ReuseToken = uploadA.ReuseToken,
            ClientUploadId = "client-op-001",
            FileName = "shared-report.pdf"
        });
        Assert.Equal(reuseA2.BlobId, retryFromDb.BlobId);
        Assert.Equal(2L, setup.StorageObjectRefCount);

        // Changing FileName under the same ClientUploadId must be rejected.
        await Assert.ThrowsAsync<BusinessException>(() =>
            controller.ReuseAsync(setup.UnitAId, new ReuseAttachmentInput
            {
                ReuseToken = uploadA.ReuseToken,
                ClientUploadId = "client-op-001",
                FileName = "different-name.pdf"
            }));
    }

    [Fact]
    public async Task ReuseAsync_rejects_cross_account_token_disabled_unit_tampered_and_expired_tokens()
    {
        var setup = new TestSetup();
        var accountAController = setup.CreateController(setup.AccountUserId, [101L, 102L]);
        var uploadA = await accountAController.UploadFileAsync(
            setup.UnitAId,
            CreateFormFile("secret.pdf", "application/pdf", Encoding.UTF8.GetBytes("secret-bytes")));
        Assert.False(string.IsNullOrWhiteSpace(uploadA.ReuseToken));

        // 1. Different account trying to redeem account A's token -> rejected.
        var accountBId = Guid.NewGuid();
        var accountBController = setup.CreateController(accountBId, [101L, 102L]);
        var crossAccountEx = await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
            accountBController.ReuseAsync(setup.UnitBId, new ReuseAttachmentInput { ReuseToken = uploadA.ReuseToken }));
        Assert.Contains("invalid or expired", crossAccountEx.Message, StringComparison.OrdinalIgnoreCase);

        // 2. Disabled / non-input-enabled target SessionUnit -> rejected.
        await Assert.ThrowsAsync<BusinessException>(() =>
            accountAController.ReuseAsync(setup.DisabledUnitId, new ReuseAttachmentInput { ReuseToken = uploadA.ReuseToken }));

        // 3. Tampered token -> rejected.
        await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
            accountAController.ReuseAsync(setup.UnitBId, new ReuseAttachmentInput { ReuseToken = uploadA.ReuseToken + "tampered" }));

        // 4. Expired token -> rejected.
        setup.Now = setup.Now.AddHours(30);
        await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
            accountAController.ReuseAsync(setup.UnitBId, new ReuseAttachmentInput { ReuseToken = uploadA.ReuseToken }));
    }

    [Fact]
    public async Task ReuseAsync_rejects_muted_target_SessionUnit()
    {
        var setup = new TestSetup();
        var controller = setup.CreateController(setup.AccountUserId, [101L, 102L]);
        var uploadA = await controller.UploadFileAsync(
            setup.UnitAId,
            CreateFormFile("muted-test.pdf", "application/pdf", Encoding.UTF8.GetBytes("muted-bytes")));

        var unitManager = controller.GetType();
        Assert.NotNull(unitManager);
        // Set mute on UnitB cache item by simulating muted target via clock rewind/forward?
        // Actually reuse a dedicated muted UnitId with MuteExpireTime in future:
        var mutedUnit = new SessionUnitCacheItem
        {
            Id = setup.UnitBId,
            OwnerId = 102L,
            SessionId = setup.SessionId,
            IsEnabled = true,
            IsInputEnabled = true,
            MuteExpireTime = setup.Now.AddMinutes(30)
        };
        var suMgrField = typeof(AttachmentController).GetField("<sessionUnitManager>P",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (suMgrField?.GetValue(controller) is ISessionUnitManager mockMgr)
        {
            mockMgr.GetCacheAsync(setup.UnitBId).Returns(mutedUnit);
            await Assert.ThrowsAsync<BusinessException>(() =>
                controller.ReuseAsync(setup.UnitBId, new ReuseAttachmentInput { ReuseToken = uploadA.ReuseToken }));
        }
    }

    [Fact]
    public async Task ReuseAsync_rolls_back_and_returns_committed_winner_on_unique_constraint_conflict()
    {
        var setup = new TestSetup();
        var controller = setup.CreateController(setup.AccountUserId, [101L, 102L]);
        var uploadA = await controller.UploadFileAsync(
            setup.UnitAId,
            CreateFormFile("race.pdf", "application/pdf", Encoding.UTF8.GetBytes("race-bytes")));

        // First request succeeds and commits winner record
        var winner = await controller.ReuseAsync(setup.UnitAId, new ReuseAttachmentInput
        {
            ReuseToken = uploadA.ReuseToken,
            ClientUploadId = "race-op-1",
            FileName = "race.pdf"
        });
        var winnerRecord = Assert.Single(setup.ReuseRecords);

        // Simulate concurrent loser: initial read misses (cleared cache & transient empty read), then InsertAsync hits SQLite UNIQUE constraint (code 19)
        setup.CacheStore.Clear();
        var findCall = 0;
        setup.ReuseRecordRepository
            .FindAsync(Arg.Any<Expression<Func<BlobReuseRecord, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                findCall++;
                return findCall == 1 ? null : winnerRecord;
            });

        var sqliteUniqueEx = new Microsoft.Data.Sqlite.SqliteException("SQLite Error 19: 'UNIQUE constraint failed: Chat_BlobReuseRecord'.", 19);
        setup.ReuseRecordRepository
            .InsertAsync(Arg.Any<BlobReuseRecord>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns<Task<BlobReuseRecord>>(_ => throw new InvalidOperationException("Wrapped constraint violation", sqliteUniqueEx));

        var loserWriteUow = Substitute.For<Volo.Abp.Uow.IUnitOfWork>();
        var readUow = Substitute.For<Volo.Abp.Uow.IUnitOfWork>();
        setup.UowManager.Begin(Arg.Any<Volo.Abp.Uow.AbpUnitOfWorkOptions>(), Arg.Any<bool>())
            .Returns(call =>
            {
                var opts = call.Arg<Volo.Abp.Uow.AbpUnitOfWorkOptions>();
                return opts.IsTransactional ? loserWriteUow : readUow;
            });

        var recovered = await controller.ReuseAsync(setup.UnitAId, new ReuseAttachmentInput
        {
            ReuseToken = uploadA.ReuseToken,
            ClientUploadId = "race-op-1",
            FileName = "race.pdf"
        });

        Assert.Equal(winner.BlobId, recovered.BlobId);
        await loserWriteUow.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
        await readUow.Received(2).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReuseAsync_enforces_feature_switch_and_ClientUploadId_length_limit()
    {
        var setup = new TestSetup();
        var controller = setup.CreateController(setup.AccountUserId, [101L, 102L]);
        var uploadA = await controller.UploadFileAsync(
            setup.UnitAId,
            CreateFormFile("switch-test.pdf", "application/pdf", Encoding.UTF8.GetBytes("switch-bytes")));

        // 超过 64 字符的 ClientUploadId 必须拒绝
        await Assert.ThrowsAsync<BusinessException>(() =>
            controller.ReuseAsync(setup.UnitBId, new ReuseAttachmentInput
            {
                ReuseToken = uploadA.ReuseToken,
                ClientUploadId = new string('x', 65),
            }));

        // 关闭 EnableAttachmentReuse 后拒绝兑换
        var disabledReuseController = setup.CreateController(setup.AccountUserId, [101L, 102L], enableAttachmentReuse: false);
        await Assert.ThrowsAsync<BusinessException>(() =>
            disabledReuseController.ReuseAsync(setup.UnitBId, new ReuseAttachmentInput
            {
                ReuseToken = uploadA.ReuseToken,
                ClientUploadId = "valid-op-1",
            }));
    }
}
