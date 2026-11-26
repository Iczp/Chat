using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using StackExchange.Redis;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Guids;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Xunit;

namespace IczpNet.Chat.EntityFrameworkCore;

[DependsOn(typeof(MessageQueryReviewTestModule))]
public class AttachmentAuthorizationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var mux = Substitute.For<IConnectionMultiplexer>();
        mux.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(Substitute.For<IDatabase>());
        context.Services.Replace(ServiceDescriptor.Singleton(mux));
        var distLock = Substitute.For<Volo.Abp.DistributedLocking.IAbpDistributedLock>();
        distLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(_ => Task.FromResult<Volo.Abp.DistributedLocking.IAbpDistributedLockHandle>(Substitute.For<Volo.Abp.DistributedLocking.IAbpDistributedLockHandle>()));
        context.Services.Replace(ServiceDescriptor.Singleton(distLock));
        context.Services.PostConfigure<IczpNet.Chat.Options.MessageOptions>(opt => opt.ChangeSyncEnabled = true);
    }
}

[Collection("MessageQueryReview")]
public class AttachmentAuthorizationAndForwarding_Tests : ChatTestBase<AttachmentAuthorizationTestModule>
{
    public AttachmentAuthorizationAndForwarding_Tests()
    {
        ServiceProvider.UseStaticObjectMapper();
    }

    private static void SetProp(object target, string name, object value)
        => target.GetType().GetProperty(name)!.SetValue(target, value);

    [Fact]
    public async Task History_forward_and_nested_history_bind_only_visible_non_private_attachments_idempotently()
    {
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var sessionA = guidGen.Create();
        var sessionB = guidGen.Create();
        var sessionC = guidGen.Create();
        var unitA1 = guidGen.Create();
        var unitB1 = guidGen.Create();
        var unitC1 = guidGen.Create();
        var unitOther = guidGen.Create();

        var imgBlobId = guidGen.Create();
        var thumbBlobId = guidGen.Create();
        var privateBlobId = guidGen.Create();
        var recalledBlobId = guidGen.Create();
        var legacyBlobId = guidGen.Create();

        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            foreach (var s in new[] { sessionA, sessionB, sessionC })
                await MessageVisibilityQuery_Tests.InsertAsync<Session>(db, new() { ["Id"] = s });

            foreach (var (su, sid, owner) in new[] { (unitA1, sessionA, 10L), (unitB1, sessionB, 10L), (unitC1, sessionC, 10L), (unitOther, sessionA, 20L) })
            {
                await MessageVisibilityQuery_Tests.InsertAsync<SessionUnit>(db, new() { ["Id"] = su, ["SessionId"] = sid, ["OwnerId"] = owner });
                await MessageVisibilityQuery_Tests.InsertAsync<SessionUnitSetting>(db, new() { ["SessionUnitId"] = su, ["SessionId"] = sid });
            }

            foreach (var bId in new[] { imgBlobId, thumbBlobId, privateBlobId, recalledBlobId, legacyBlobId })
            {
                await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
                {
                    ["Id"] = bId,
                    ["Container"] = "chat-files",
                    ["Name"] = $"2026/11/{bId:N}.png",
                    ["FileName"] = $"{bId:N}.png",
                    ["MimeType"] = "image/png",
                    ["Suffix"] = ".png",
                    ["Status"] = BlobStatus.Bound,
                    ["RefCount"] = 1
                });
            }

            // 正常公开消息 1001
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new()
            {
                ["Id"] = 1001L,
                ["SessionId"] = sessionA,
                ["SessionSeq"] = 1L,
                ["SenderId"] = 10L,
                ["SenderSessionUnitId"] = unitA1,
                ["MessageType"] = MessageTypes.Image,
                ["ExtraProperties"] = new ExtraPropertyDictionary()
            });
            await MessageVisibilityQuery_Tests.InsertAsync<BlobMessageRef>(db, new()
            {
                ["Id"] = guidGen.Create(),
                ["BlobId"] = imgBlobId,
                ["MessageId"] = 1001L,
                ["SessionId"] = sessionA,
                ["SourceType"] = BlobRefSourceTypes.Send
            });
            await MessageVisibilityQuery_Tests.InsertAsync<BlobMessageRef>(db, new()
            {
                ["Id"] = guidGen.Create(),
                ["BlobId"] = thumbBlobId,
                ["MessageId"] = 1001L,
                ["SessionId"] = sessionA,
                ["SourceType"] = BlobRefSourceTypes.Send
            });

            // 私密消息 1002（不应被合并/转发带出）
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new()
            {
                ["Id"] = 1002L,
                ["SessionId"] = sessionA,
                ["SessionSeq"] = 2L,
                ["SenderId"] = 20L,
                ["SenderSessionUnitId"] = unitOther,
                ["IsPrivate"] = true,
                ["ReceiverSessionUnitId"] = unitA1,
                ["MessageType"] = MessageTypes.File,
                ["ExtraProperties"] = new ExtraPropertyDictionary()
            });
            await MessageVisibilityQuery_Tests.InsertAsync<BlobMessageRef>(db, new()
            {
                ["Id"] = guidGen.Create(),
                ["BlobId"] = privateBlobId,
                ["MessageId"] = 1002L,
                ["SessionId"] = sessionA,
                ["SourceType"] = BlobRefSourceTypes.Send
            });

            // 已撤回消息 1003（不应被带出）
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new()
            {
                ["Id"] = 1003L,
                ["SessionId"] = sessionA,
                ["SessionSeq"] = 3L,
                ["SenderId"] = 10L,
                ["SenderSessionUnitId"] = unitA1,
                ["IsRollbacked"] = true,
                ["RollbackTime"] = DateTime.UtcNow,
                ["MessageType"] = MessageTypes.File,
                ["ExtraProperties"] = new ExtraPropertyDictionary()
            });
            await MessageVisibilityQuery_Tests.InsertAsync<BlobMessageRef>(db, new()
            {
                ["Id"] = guidGen.Create(),
                ["BlobId"] = recalledBlobId,
                ["MessageId"] = 1003L,
                ["SessionId"] = sessionA,
                ["SourceType"] = BlobRefSourceTypes.Send
            });

            // 历史缺 BlobMessageRef 的合法图片消息 1004（通过 ImageContent 结构化表回退收集）
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new()
            {
                ["Id"] = 1004L,
                ["SessionId"] = sessionA,
                ["SessionSeq"] = 4L,
                ["SenderId"] = 10L,
                ["SenderSessionUnitId"] = unitA1,
                ["MessageType"] = MessageTypes.Image,
                ["ExtraProperties"] = new ExtraPropertyDictionary()
            });
            var legacyImgContent = new ImageContent { BlobId = legacyBlobId, Url = $"/api/chat/file/{legacyBlobId}", FileName = "legacy.png" };
            SetProp(legacyImgContent, nameof(ImageContent.Id), guidGen.Create());
            db.Set<ImageContent>().Add(legacyImgContent);
            var msg1004 = await db.Message.SingleAsync(x => x.Id == 1004L);
            msg1004.ImageContentList.Add(legacyImgContent);

            // 第一层聊天记录消息 2001（包含 1001, 1002, 1003, 1004）
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new()
            {
                ["Id"] = 2001L,
                ["SessionId"] = sessionB,
                ["SessionSeq"] = 1L,
                ["SenderId"] = 10L,
                ["SenderSessionUnitId"] = unitB1,
                ["MessageType"] = MessageTypes.History,
                ["ExtraProperties"] = new ExtraPropertyDictionary()
            });
            var histContent = new HistoryContent(guidGen.Create(), "聊天记录", "desc");
            db.Set<HistoryContent>().Add(histContent);
            var msg2001 = await db.Message.SingleAsync(x => x.Id == 2001L);
            msg2001.HistoryContentList.Add(histContent);
            foreach (var childId in new[] { 1001L, 1002L, 1003L, 1004L })
            {
                db.Set<HistoryMessage>().Add(new HistoryMessage(histContent, await db.Message.SingleAsync(x => x.Id == childId)));
            }

            // 再次转发到 sessionC 的聊天记录消息 3001
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new()
            {
                ["Id"] = 3001L,
                ["SessionId"] = sessionC,
                ["SessionSeq"] = 1L,
                ["SenderId"] = 10L,
                ["SenderSessionUnitId"] = unitC1,
                ["ForwardMessageId"] = 2001L,
                ["MessageType"] = MessageTypes.History,
                ["ExtraProperties"] = new ExtraPropertyDictionary()
            });

            await db.SaveChangesAsync();

            var messageManager = ServiceProvider.GetRequiredService<IMessageManager>();
            await messageManager.BindHistoryAttachmentsAsync(3001L, sessionC, [2001L]);
            // 重复调用应幂等，不重复递增 RefCount
            await messageManager.BindHistoryAttachmentsAsync(3001L, sessionC, [2001L]);
            await db.SaveChangesAsync();

            var refs = await db.BlobMessageRef.AsNoTracking()
                .Where(x => x.MessageId == 3001L)
                .Select(x => x.BlobId)
                .ToListAsync();

            Assert.Contains(imgBlobId, refs);
            Assert.Contains(thumbBlobId, refs);
            Assert.Contains(legacyBlobId, refs);
            Assert.DoesNotContain(privateBlobId, refs);
            Assert.DoesNotContain(recalledBlobId, refs);
            Assert.Equal(3, refs.Count);

            var imgBlob = await db.Blob.AsNoTracking().SingleAsync(x => x.Id == imgBlobId);
            Assert.Equal(2, imgBlob.RefCount);
        });
    }

    [Fact]
    public async Task PropagateVideoGifAsync_updates_all_forward_levels_and_history_roots_idempotently()
    {
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var sessionA = guidGen.Create();
        var sessionB = guidGen.Create();
        var sessionC = guidGen.Create();
        var sessionD = guidGen.Create();
        var videoBlobId = guidGen.Create();
        var gifBlobId = guidGen.Create();
        var videoContentId = guidGen.Create();
        var gifUrl = $"/api/chat/file/{gifBlobId}";

        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            foreach (var s in new[] { sessionA, sessionB, sessionC, sessionD })
                await MessageVisibilityQuery_Tests.InsertAsync<Session>(db, new() { ["Id"] = s });

            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = videoBlobId,
                ["Container"] = "chat-files",
                ["Name"] = "2026/11/v.mp4",
                ["FileName"] = "v.mp4",
                ["MimeType"] = "video/mp4",
                ["Suffix"] = ".mp4",
                ["Status"] = BlobStatus.Bound,
                ["RefCount"] = 3
            });
            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = gifBlobId,
                ["Container"] = "chat-files",
                ["Name"] = "2026/11/v.gif",
                ["FileName"] = "v.gif",
                ["MimeType"] = "image/gif",
                ["Suffix"] = ".gif",
                ["Status"] = BlobStatus.Bound,
                ["RefCount"] = 0
            });

            var videoContent = new VideoContent
            {
                BlobId = videoBlobId,
                Url = $"/api/chat/file/{videoBlobId}",
                FileName = "v.mp4",
                Width = 640,
                Height = 360
            };
            SetProp(videoContent, nameof(VideoContent.Id), videoContentId);
            db.Set<VideoContent>().Add(videoContent);

            // 原消息 4001 -> 一级转发 4002 -> 二级转发 4003
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new() { ["Id"] = 4001L, ["SessionId"] = sessionA, ["SessionSeq"] = 1L, ["MessageType"] = MessageTypes.Video, ["ExtraProperties"] = new ExtraPropertyDictionary() });
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new() { ["Id"] = 4002L, ["SessionId"] = sessionB, ["SessionSeq"] = 1L, ["ForwardMessageId"] = 4001L, ["MessageType"] = MessageTypes.Video, ["ExtraProperties"] = new ExtraPropertyDictionary() });
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new() { ["Id"] = 4003L, ["SessionId"] = sessionC, ["SessionSeq"] = 1L, ["ForwardMessageId"] = 4002L, ["MessageType"] = MessageTypes.Video, ["ExtraProperties"] = new ExtraPropertyDictionary() });

            foreach (var id in new[] { 4001L, 4002L, 4003L })
            {
                var m = await db.Message.SingleAsync(x => x.Id == id);
                m.VideoContentList.Add(videoContent);
            }

            // 期间产生的合并转发聊天记录 4004（包含 4002）
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new() { ["Id"] = 4004L, ["SessionId"] = sessionD, ["SessionSeq"] = 1L, ["MessageType"] = MessageTypes.History, ["ExtraProperties"] = new ExtraPropertyDictionary() });
            var hc = new HistoryContent(guidGen.Create(), "合并视频", "desc");
            db.Set<HistoryContent>().Add(hc);
            var m4004 = await db.Message.SingleAsync(x => x.Id == 4004L);
            m4004.HistoryContentList.Add(hc);
            db.Set<HistoryMessage>().Add(new HistoryMessage(hc, await db.Message.SingleAsync(x => x.Id == 4002L)));

            await db.SaveChangesAsync();
        });

        var repo = ServiceProvider.GetRequiredService<IMessageRepository>();
        Assert.True(await repo.TrySetVideoGifAsync(videoContentId, gifBlobId, gifUrl));
        // 并发第二次条件更新必须返回 false
        Assert.False(await repo.TrySetVideoGifAsync(videoContentId, guidGen.Create(), "/api/chat/file/other"));

        var manager = ServiceProvider.GetRequiredService<IMessageManager>();
        await manager.PropagateVideoGifAsync(4001L, videoContentId, gifBlobId, gifUrl);
        // 重跑一次必须幂等，不重复增加 RefCount
        await manager.PropagateVideoGifAsync(4001L, videoContentId, gifBlobId, gifUrl);

        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var refs = await db.BlobMessageRef.AsNoTracking()
                .Where(x => x.BlobId == gifBlobId)
                .OrderBy(x => x.MessageId)
                .ToListAsync();

            Assert.Equal([4001L, 4002L, 4003L, 4004L], refs.Select(x => x.MessageId).ToArray());
            Assert.Equal(BlobRefSourceTypes.Send, refs.Single(x => x.MessageId == 4001L).SourceType);
            Assert.Equal(BlobRefSourceTypes.Forward, refs.Single(x => x.MessageId == 4002L).SourceType);
            Assert.Equal(BlobRefSourceTypes.Forward, refs.Single(x => x.MessageId == 4003L).SourceType);
            Assert.Equal(BlobRefSourceTypes.History, refs.Single(x => x.MessageId == 4004L).SourceType);

            var gifBlob = await db.Blob.AsNoTracking().SingleAsync(x => x.Id == gifBlobId);
            Assert.Equal(4, gifBlob.RefCount);

            foreach (var id in new[] { 4001L, 4002L, 4003L })
            {
                var msg = await db.Message.AsNoTracking().SingleAsync(x => x.Id == id);
                Assert.Contains(gifUrl, msg.ContentJson);
                Assert.NotNull(msg.MessageChangeId);
            }
        });
    }

    [Fact]
    public async Task ForwardAsync_preserves_original_content_ownerId_when_member_forwards_another_members_message()
    {
        ServiceProvider.UseStaticObjectMapper();
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var sessionA = guidGen.Create();
        var sessionB = guidGen.Create();
        var authorUnitId = guidGen.Create();
        var forwarderUnitAId = guidGen.Create();
        var forwarderUnitBId = guidGen.Create();
        var imgBlobId = guidGen.Create();

        long srcMessageId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await MessageVisibilityQuery_Tests.InsertAsync<Session>(db, new() { ["Id"] = sessionA });
            await MessageVisibilityQuery_Tests.InsertAsync<Session>(db, new() { ["Id"] = sessionB });
            await MessageVisibilityQuery_Tests.InsertAsync<IczpNet.Chat.ChatObjects.ChatObject>(db, new() { ["Id"] = 100L, ["Name"] = "Alice", ["Code"] = "alice" });
            await MessageVisibilityQuery_Tests.InsertAsync<IczpNet.Chat.ChatObjects.ChatObject>(db, new() { ["Id"] = 200L, ["Name"] = "Bob", ["Code"] = "bob" });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnit>(db, new() { ["Id"] = authorUnitId, ["SessionId"] = sessionA, ["OwnerId"] = 100L });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnitSetting>(db, new() { ["SessionUnitId"] = authorUnitId, ["SessionId"] = sessionA, ["IsEnabled"] = true });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnit>(db, new() { ["Id"] = forwarderUnitAId, ["SessionId"] = sessionA, ["OwnerId"] = 200L });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnitSetting>(db, new() { ["SessionUnitId"] = forwarderUnitAId, ["SessionId"] = sessionA, ["IsEnabled"] = true, ["IsInputEnabled"] = true });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnit>(db, new() { ["Id"] = forwarderUnitBId, ["SessionId"] = sessionB, ["OwnerId"] = 200L });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnitSetting>(db, new() { ["SessionUnitId"] = forwarderUnitBId, ["SessionId"] = sessionB, ["IsEnabled"] = true, ["IsInputEnabled"] = true });

            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = imgBlobId,
                ["Container"] = "chat-files",
                ["Name"] = "2026/11/a.png",
                ["FileName"] = "a.png",
                ["MimeType"] = "image/png",
                ["Suffix"] = ".png",
                ["UploaderChatObjectId"] = 100L,
                ["Status"] = BlobStatus.Bound,
                ["RefCount"] = 1
            });

            var srcMsg = new Message(new SessionUnitCacheItem { Id = authorUnitId, OwnerId = 100L, SessionId = sessionA })
            {
                CreationTime = DateTime.UtcNow,
                ClientMessageId = "src-msg-5001"
            };
            SetProp(srcMsg, nameof(Message.SessionSeq), 1L);
            SetProp(srcMsg, nameof(Message.MessageType), MessageTypes.Image);
            db.Message.Add(srcMsg);
            await db.SaveChangesAsync();
            srcMessageId = srcMsg.Id;

            var content = new ImageContent { BlobId = imgBlobId, Url = $"/api/chat/file/{imgBlobId}", FileName = "a.png" };
            SetProp(content, nameof(ImageContent.Id), guidGen.Create());
            content.SetOwnerId(100L);
            db.Set<ImageContent>().Add(content);
            srcMsg.ImageContentList.Add(content);
            await db.SaveChangesAsync();
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var manager = ServiceProvider.GetRequiredService<IMessageManager>();
            var forwarded = await manager.ForwardAsync(forwarderUnitAId, srcMessageId, [forwarderUnitBId]);
            var fwdMsg = Assert.Single(forwarded);
            Assert.Equal(200L, fwdMsg.SenderId);
            Assert.Equal(srcMessageId, fwdMsg.ForwardMessageId);

            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var savedContent = await db.Set<ImageContent>().AsNoTracking().SingleAsync(x => x.BlobId == imgBlobId);
            Assert.Equal(100L, savedContent.OwnerId);
        });
    }

    [Fact]
    public async Task BlobAccessAuthorizationService_enforces_message_visibility_recall_and_deleted_rules()
    {
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var sessionA = guidGen.Create();
        var unitAlice = guidGen.Create();
        var unitBob = guidGen.Create();
        var boundBlobId = guidGen.Create();
        var recalledBlobId = guidGen.Create();
        var tempBlobId = guidGen.Create();
        var expiredTempBlobId = guidGen.Create();

        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await MessageVisibilityQuery_Tests.InsertAsync<Session>(db, new() { ["Id"] = sessionA });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnit>(db, new() { ["Id"] = unitAlice, ["SessionId"] = sessionA, ["OwnerId"] = 101L });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnitSetting>(db, new() { ["SessionUnitId"] = unitAlice, ["SessionId"] = sessionA });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnit>(db, new() { ["Id"] = unitBob, ["SessionId"] = sessionA, ["OwnerId"] = 102L });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnitSetting>(db, new() { ["SessionUnitId"] = unitBob, ["SessionId"] = sessionA });

            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = boundBlobId,
                ["Container"] = "chat-files",
                ["Name"] = "2026/11/bound.png",
                ["FileName"] = "bound.png",
                ["MimeType"] = "image/png",
                ["Suffix"] = ".png",
                ["UploaderChatObjectId"] = 101L,
                ["Status"] = BlobStatus.Bound,
                ["RefCount"] = 1
            });
            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = recalledBlobId,
                ["Container"] = "chat-files",
                ["Name"] = "2026/11/recalled.png",
                ["FileName"] = "recalled.png",
                ["MimeType"] = "image/png",
                ["Suffix"] = ".png",
                ["UploaderChatObjectId"] = 101L,
                ["Status"] = BlobStatus.Bound,
                ["RefCount"] = 1
            });
            var now = ServiceProvider.GetRequiredService<Volo.Abp.Timing.IClock>().Now;
            db.Blob.Add(new Blob(tempBlobId, "chat-files")
            {
                Name = "2026/11/temp.png",
                FileName = "temp.png",
                MimeType = "image/png",
                Suffix = ".png",
                UploaderChatObjectId = 101L,
                Status = BlobStatus.Temp,
                ExpireTime = now.AddHours(1)
            });
            db.Blob.Add(new Blob(expiredTempBlobId, "chat-files")
            {
                Name = "2026/11/expired.png",
                FileName = "expired.png",
                MimeType = "image/png",
                Suffix = ".png",
                UploaderChatObjectId = 101L,
                Status = BlobStatus.Temp,
                ExpireTime = now.AddHours(-1)
            });

            // 6001: 正常消息；6002: 已撤回消息
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new() { ["Id"] = 6001L, ["SessionId"] = sessionA, ["SessionSeq"] = 1L, ["SenderId"] = 101L, ["SenderSessionUnitId"] = unitAlice, ["MessageType"] = MessageTypes.Image, ["ExtraProperties"] = new ExtraPropertyDictionary() });
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new() { ["Id"] = 6002L, ["SessionId"] = sessionA, ["SessionSeq"] = 2L, ["SenderId"] = 101L, ["SenderSessionUnitId"] = unitAlice, ["IsRollbacked"] = true, ["RollbackTime"] = DateTime.UtcNow, ["MessageType"] = MessageTypes.Image, ["ExtraProperties"] = new ExtraPropertyDictionary() });

            await MessageVisibilityQuery_Tests.InsertAsync<BlobMessageRef>(db, new() { ["Id"] = guidGen.Create(), ["BlobId"] = boundBlobId, ["MessageId"] = 6001L, ["SessionId"] = sessionA, ["SourceType"] = BlobRefSourceTypes.Send });
            await MessageVisibilityQuery_Tests.InsertAsync<BlobMessageRef>(db, new() { ["Id"] = guidGen.Create(), ["BlobId"] = recalledBlobId, ["MessageId"] = 6002L, ["SessionId"] = sessionA, ["SourceType"] = BlobRefSourceTypes.Send });

            // Bob 个人删除了 6001
            await MessageVisibilityQuery_Tests.InsertAsync<DeletedRecorder>(db, new() { ["SessionUnitId"] = unitBob, ["MessageId"] = 6001L });
            await db.SaveChangesAsync();

            var auth = ServiceProvider.GetRequiredService<IBlobAccessAuthorizationService>();

            // Alice 旧客户端只传 BlobId -> 允许；带上下文 -> 允许；带有效签名 -> 允许
            Assert.NotNull(await auth.CheckAccessAsync(boundBlobId, null, null, null, [101L], false));
            Assert.NotNull(await auth.CheckAccessAsync(boundBlobId, unitAlice, 6001L, null, [101L], false));
            Assert.NotNull(await auth.CheckAccessAsync(boundBlobId, unitAlice, 6001L, null, Array.Empty<long>(), true));

            // Bob 删除了 6001 -> 旧客户端仅传 BlobId 拒绝；带上下文拒绝；带签名仍因消息不可见拒绝
            await Assert.ThrowsAsync<BusinessException>(() => auth.CheckAccessAsync(boundBlobId, null, null, null, [102L], false));
            await Assert.ThrowsAsync<BusinessException>(() => auth.CheckAccessAsync(boundBlobId, unitBob, 6001L, null, [102L], false));
            await Assert.ThrowsAsync<BusinessException>(() => auth.CheckAccessAsync(boundBlobId, unitBob, 6001L, null, Array.Empty<long>(), true));

            // 已撤回消息 6002 -> 上传者本人也不可绕过消息授权
            await Assert.ThrowsAsync<BusinessException>(() => auth.CheckAccessAsync(recalledBlobId, null, null, null, [101L], false));
            await Assert.ThrowsAsync<BusinessException>(() => auth.CheckAccessAsync(recalledBlobId, unitAlice, 6002L, null, [101L], false));

            // Temp 附件：上传者未过期允许，过期或非上传者拒绝
            Assert.NotNull(await auth.CheckAccessAsync(tempBlobId, null, null, null, [101L], false));
            await Assert.ThrowsAsync<BusinessException>(() => auth.CheckAccessAsync(tempBlobId, null, null, null, [102L], false));
            await Assert.ThrowsAsync<BusinessException>(() => auth.CheckAccessAsync(expiredTempBlobId, null, null, null, [101L], false));
        });
    }
}
