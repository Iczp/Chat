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

            // 聊天记录根消息新增 GIF 授权后也应写入 MessageChange 以驱动增量同步
            var updatedHistoryRoot = await db.Message.AsNoTracking().SingleAsync(x => x.Id == 4004L);
            Assert.NotNull(updatedHistoryRoot.MessageChangeId);
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

            // 根消息 + 无关子消息 -> 拒绝（必须存在 History/同会话Quote 关联）
            var unrelatedSession = guidGen.Create();
            await MessageVisibilityQuery_Tests.InsertAsync<Session>(db, new() { ["Id"] = unrelatedSession });
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new()
            {
                ["Id"] = 6099L,
                ["SessionId"] = unrelatedSession,
                ["SessionSeq"] = 1L,
                ["SenderId"] = 101L,
                ["MessageType"] = MessageTypes.Image,
                ["ExtraProperties"] = new ExtraPropertyDictionary()
            });
            await db.SaveChangesAsync();
            await Assert.ThrowsAsync<BusinessException>(() => auth.CheckAccessAsync(boundBlobId, unitAlice, 6099L, 6001L, [101L], false));

            // 超过 50 条其他会话的较新引用也不应截断旧客户端对较早合法消息的访问
            for (int i = 0; i < 55; i++)
            {
                long extMsgId = 6200L + i;
                await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new()
                {
                    ["Id"] = extMsgId,
                    ["SessionId"] = unrelatedSession,
                    ["SessionSeq"] = i + 2L,
                    ["SenderId"] = 999L,
                    ["MessageType"] = MessageTypes.Image,
                    ["CreationTime"] = DateTime.UtcNow.AddMinutes(i + 1),
                    ["ExtraProperties"] = new ExtraPropertyDictionary()
                });
                await MessageVisibilityQuery_Tests.InsertAsync<BlobMessageRef>(db, new()
                {
                    ["Id"] = guidGen.Create(),
                    ["BlobId"] = boundBlobId,
                    ["MessageId"] = extMsgId,
                    ["SessionId"] = unrelatedSession,
                    ["SourceType"] = BlobRefSourceTypes.Forward,
                    ["CreationTime"] = DateTime.UtcNow.AddMinutes(i + 1)
                });
            }
            await db.SaveChangesAsync();
            Assert.NotNull(await auth.CheckAccessAsync(boundBlobId, null, null, null, [101L], false));

            // 已过期 Temp 在绑定消息阶段必须被拒绝
            var msgRepo = ServiceProvider.GetRequiredService<IMessageRepository>();
            await Assert.ThrowsAsync<BusinessException>(() =>
                msgRepo.BindBlobsToMessageAsync(6001L, sessionA, [expiredTempBlobId], BlobRefSourceTypes.Send, now: now, markAsBound: true));

            // 跨会话转发聊天记录：Session B 中的 History 引用 Session A 消息，该消息又引用 Session A 的附件消息 -> 允许合法下载
            var sessionB = guidGen.Create();
            var unitBobInB = guidGen.Create();
            var crossHistContentId = guidGen.Create();
            var crossQuoteBlobId = guidGen.Create();
            await MessageVisibilityQuery_Tests.InsertAsync<Session>(db, new() { ["Id"] = sessionB });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnit>(db, new() { ["Id"] = unitBobInB, ["SessionId"] = sessionB, ["OwnerId"] = 102L });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnitSetting>(db, new() { ["SessionUnitId"] = unitBobInB, ["SessionId"] = sessionB, ["IsEnabled"] = true });
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new() { ["Id"] = 6301L, ["SessionId"] = sessionA, ["SessionSeq"] = 200L, ["SenderId"] = 101L, ["MessageType"] = MessageTypes.Image, ["ExtraProperties"] = new ExtraPropertyDictionary() });
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new() { ["Id"] = 6302L, ["SessionId"] = sessionA, ["SessionSeq"] = 201L, ["SenderId"] = 101L, ["QuoteMessageId"] = 6301L, ["MessageType"] = MessageTypes.Text, ["ExtraProperties"] = new ExtraPropertyDictionary() });
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new() { ["Id"] = 6303L, ["SessionId"] = sessionB, ["SessionSeq"] = 1L, ["SenderId"] = 101L, ["MessageType"] = MessageTypes.History, ["ExtraProperties"] = new ExtraPropertyDictionary() });
            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = crossQuoteBlobId,
                ["Container"] = "chat-files",
                ["Name"] = "2026/11/cross-quote.png",
                ["FileName"] = "cross-quote.png",
                ["MimeType"] = "image/png",
                ["Suffix"] = ".png",
                ["Status"] = BlobStatus.Bound,
                ["RefCount"] = 1
            });
            await MessageVisibilityQuery_Tests.InsertAsync<BlobMessageRef>(db, new() { ["Id"] = guidGen.Create(), ["BlobId"] = crossQuoteBlobId, ["MessageId"] = 6301L, ["SessionId"] = sessionA, ["SourceType"] = BlobRefSourceTypes.Send });
            await MessageVisibilityQuery_Tests.InsertAsync<BlobMessageRef>(db, new() { ["Id"] = guidGen.Create(), ["BlobId"] = crossQuoteBlobId, ["MessageId"] = 6303L, ["SessionId"] = sessionB, ["SourceType"] = BlobRefSourceTypes.History });
            var crossHc = new HistoryContent(crossHistContentId, "跨会话记录", "desc");
            db.Set<HistoryContent>().Add(crossHc);
            var m6303 = await db.Message.SingleAsync(x => x.Id == 6303L);
            m6303.HistoryContentList.Add(crossHc);
            db.Set<HistoryMessage>().Add(new HistoryMessage(crossHc, await db.Message.SingleAsync(x => x.Id == 6302L)));
            await db.SaveChangesAsync();
            Assert.True(await msgRepo.CanReadAttachmentAsync(unitBobInB, 6303L, 6301L));
            Assert.NotNull(await auth.CheckAccessAsync(crossQuoteBlobId, unitBobInB, 6301L, 6303L, [102L], false));
        });
    }

    [Fact]
    public async Task VideoGifPropagationOutbox_commits_and_rolls_back_atomically_with_UOW_and_propagates()
    {
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var uowManager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var outboxRepo = ServiceProvider.GetRequiredService<Volo.Abp.Domain.Repositories.IRepository<VideoGifPropagationOutbox, Guid>>();
        var outboxService = ServiceProvider.GetRequiredService<IVideoGifPropagationOutboxService>();
        var msgRepo = ServiceProvider.GetRequiredService<IMessageRepository>();

        var sessionA = guidGen.Create();
        var sessionB = guidGen.Create();
        var videoBlobId = guidGen.Create();
        var gifBlobId = guidGen.Create();
        var videoContentId = guidGen.Create();
        var gifUrl = $"/api/chat/file/{gifBlobId}";

        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await MessageVisibilityQuery_Tests.InsertAsync<Session>(db, new() { ["Id"] = sessionA });
            await MessageVisibilityQuery_Tests.InsertAsync<Session>(db, new() { ["Id"] = sessionB });
            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = videoBlobId,
                ["Container"] = "chat-files",
                ["Name"] = "2026/11/v-outbox.mp4",
                ["FileName"] = "v-outbox.mp4",
                ["MimeType"] = "video/mp4",
                ["Suffix"] = ".mp4",
                ["Status"] = BlobStatus.Bound,
                ["RefCount"] = 2
            });
            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = gifBlobId,
                ["Container"] = "chat-files",
                ["Name"] = "2026/11/v-outbox.gif",
                ["FileName"] = "v-outbox.gif",
                ["MimeType"] = "image/gif",
                ["Suffix"] = ".gif",
                ["Status"] = BlobStatus.Bound,
                ["RefCount"] = 0
            });

            var videoContent = new VideoContent
            {
                BlobId = videoBlobId,
                Url = $"/api/chat/file/{videoBlobId}",
                FileName = "v-outbox.mp4",
                Width = 640,
                Height = 360
            };
            SetProp(videoContent, nameof(VideoContent.Id), videoContentId);
            db.Set<VideoContent>().Add(videoContent);

            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new() { ["Id"] = 4101L, ["SessionId"] = sessionA, ["SessionSeq"] = 1L, ["MessageType"] = MessageTypes.Video, ["ExtraProperties"] = new ExtraPropertyDictionary() });
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new() { ["Id"] = 4102L, ["SessionId"] = sessionB, ["SessionSeq"] = 1L, ["ForwardMessageId"] = 4101L, ["MessageType"] = MessageTypes.Video, ["ExtraProperties"] = new ExtraPropertyDictionary() });
            (await db.Message.SingleAsync(x => x.Id == 4101L)).VideoContentList.Add(videoContent);
            (await db.Message.SingleAsync(x => x.Id == 4102L)).VideoContentList.Add(videoContent);
            await db.SaveChangesAsync();
        });

        // 未提交（回滚）的业务 UOW 不能留下游离 Outbox 记录
        var rolledBackOutboxId = guidGen.Create();
        using (var rollbackUow = uowManager.Begin(requiresNew: true, isTransactional: true))
        {
            await outboxRepo.InsertAsync(
                new VideoGifPropagationOutbox(rolledBackOutboxId, 4101L, videoContentId, gifBlobId, gifUrl),
                autoSave: true);
            // 故意不调用 CompleteAsync
        }

        using (var verifyRollbackUow = uowManager.Begin(requiresNew: true, isTransactional: false))
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            Assert.False(await db.VideoGifPropagationOutbox.AnyAsync(x => x.Id == rolledBackOutboxId));
            await verifyRollbackUow.CompleteAsync();
        }

        // 与 GIF 写入同事务提交的 Outbox 记录由 Worker 服务投递到所有转发消息并标记 ProcessedTime
        var committedOutboxId = guidGen.Create();
        using (var commitUow = uowManager.Begin(requiresNew: true, isTransactional: true))
        {
            Assert.True(await msgRepo.TrySetVideoGifAsync(videoContentId, gifBlobId, gifUrl));
            await outboxRepo.InsertAsync(
                new VideoGifPropagationOutbox(committedOutboxId, 4101L, videoContentId, gifBlobId, gifUrl),
                autoSave: true);
            await commitUow.CompleteAsync();
        }

        var processedFirst = await outboxService.ProcessPendingAsync();
        Assert.Equal(1, processedFirst);
        var processedSecond = await outboxService.ProcessPendingAsync();
        Assert.Equal(0, processedSecond);

        using (var verifyUow = uowManager.Begin(requiresNew: true, isTransactional: false))
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var outboxRow = await db.VideoGifPropagationOutbox.AsNoTracking().SingleAsync(x => x.Id == committedOutboxId);
            Assert.NotNull(outboxRow.ProcessedTime);

            var refMsgIds = await db.BlobMessageRef.AsNoTracking()
                .Where(x => x.BlobId == gifBlobId)
                .OrderBy(x => x.MessageId)
                .Select(x => x.MessageId)
                .ToListAsync();
            Assert.Equal([4101L, 4102L], refMsgIds);

            var gifBlob = await db.Blob.AsNoTracking().SingleAsync(x => x.Id == gifBlobId);
            Assert.Equal(2, gifBlob.RefCount);
            await verifyUow.CompleteAsync();
        }
    }

    [Fact]
    public async Task BlobReuseRecord_unique_index_rejects_competing_transaction_and_rolls_back_leaving_winner_for_reread()
    {
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var uowManager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        var userId = guidGen.Create();
        var sessionUnitId = guidGen.Create();
        var sourcePrimaryBlobId = guidGen.Create();
        var clientUploadId = $"op-{guidGen.Create():N}";
        var fileNameDigest = "0123456789ABCDEF";
        var blob1Id = guidGen.Create();
        var blob2Id = guidGen.Create();
        var winnerJson = $"{{\"BlobId\":\"{blob1Id}\"}}";
        var loserJson = $"{{\"BlobId\":\"{blob2Id}\"}}";

        // 第一个事务胜出并提交
        using (var winnerUow = uowManager.Begin(requiresNew: true, isTransactional: true))
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
            db.Blob.Add(new Blob(blob1Id, "chat-files")
            {
                FileName = "a.bin",
                Name = "2026/11/a1.bin",
                MimeType = "application/octet-stream",
                Suffix = ".bin",
                Status = BlobStatus.Temp,
                ExpireTime = DateTime.UtcNow.AddHours(2),
            });
            db.BlobReuseRecord.Add(new BlobReuseRecord(
                guidGen.Create(),
                userId,
                sessionUnitId,
                sourcePrimaryBlobId,
                clientUploadId,
                fileNameDigest,
                blob1Id,
                winnerJson,
                DateTime.UtcNow.AddHours(2)));
            await db.SaveChangesAsync();
            await winnerUow.CompleteAsync();
        }

        // 第二个并发事务各自创建 Temp Blob 后插入相同 ClientUploadId -> 唯一索引冲突并回滚
        DbUpdateException conflictEx = null;
        using (var loserUow = uowManager.Begin(requiresNew: true, isTransactional: true))
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
            db.Blob.Add(new Blob(blob2Id, "chat-files")
            {
                FileName = "a.bin",
                Name = "2026/11/a2.bin",
                MimeType = "application/octet-stream",
                Suffix = ".bin",
                Status = BlobStatus.Temp,
                ExpireTime = DateTime.UtcNow.AddHours(2),
            });
            db.BlobReuseRecord.Add(new BlobReuseRecord(
                guidGen.Create(),
                userId,
                sessionUnitId,
                sourcePrimaryBlobId,
                clientUploadId,
                fileNameDigest,
                blob2Id,
                loserJson,
                DateTime.UtcNow.AddHours(2)));
            conflictEx = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            // 不调用 CompleteAsync，让败者事务回滚其新建的 blob2
        }

        Assert.True(BlobDbExceptionHelper.IsUniqueConstraintViolation(conflictEx));

        // 开启新 UOW 重读，确认 blob2 已回滚且可读取胜出记录
        using (var recoverUow = uowManager.Begin(requiresNew: true, isTransactional: false))
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            Assert.False(await db.Blob.AsNoTracking().AnyAsync(x => x.Id == blob2Id));
            var winnerRecord = await db.BlobReuseRecord.AsNoTracking().SingleAsync(x =>
                x.UserId == userId &&
                x.SessionUnitId == sessionUnitId &&
                x.SourcePrimaryBlobId == sourcePrimaryBlobId &&
                x.ClientUploadId == clientUploadId);
            Assert.Equal(blob1Id, winnerRecord.PrimaryBlobId);
            Assert.Equal(winnerJson, winnerRecord.ResultJson);
            await recoverUow.CompleteAsync();
        }
    }

    [Fact]
    public async Task PrepareAndValidateAttachmentAsync_rejects_unowned_or_mismatched_parent_private_blobs()
    {
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var messageManager = ServiceProvider.GetRequiredService<IMessageManager>();
        var method = typeof(MessageManager).GetMethod(
            "PrepareAndValidateAttachmentAsync",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

        var sessionId = guidGen.Create();
        var unitId = guidGen.Create();
        var unownedBlobId = guidGen.Create();
        var validPrimaryId = guidGen.Create();
        var foreignThumbId = guidGen.Create();

        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = unownedBlobId,
                ["Container"] = "chat-files",
                ["Name"] = "2026/11/unowned.png",
                ["FileName"] = "unowned.png",
                ["MimeType"] = "image/png",
                ["Suffix"] = ".png",
                ["IsPublic"] = false,
                ["Role"] = BlobRoles.Primary,
                ["Status"] = BlobStatus.Temp,
                ["ExpireTime"] = DateTime.UtcNow.AddHours(2),
            });
            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = validPrimaryId,
                ["Container"] = "chat-files",
                ["Name"] = "2026/11/valid-primary.png",
                ["FileName"] = "valid-primary.png",
                ["MimeType"] = "image/png",
                ["Suffix"] = ".png",
                ["IsPublic"] = false,
                ["UploaderChatObjectId"] = 101L,
                ["Role"] = BlobRoles.Primary,
                ["Status"] = BlobStatus.Temp,
                ["ExpireTime"] = DateTime.UtcNow.AddHours(2),
            });
            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = foreignThumbId,
                ["Container"] = "chat-files",
                ["Name"] = "2026/11/foreign-thumb.png",
                ["FileName"] = "foreign-thumb.png",
                ["MimeType"] = "image/png",
                ["Suffix"] = ".png",
                ["IsPublic"] = false,
                ["UploaderChatObjectId"] = 101L,
                ["ParentBlobId"] = guidGen.Create(),
                ["Role"] = BlobRoles.Thumbnail,
                ["Status"] = BlobStatus.Temp,
                ["ExpireTime"] = DateTime.UtcNow.AddHours(2),
            });
            await db.SaveChangesAsync();

            var senderUnit = new SessionUnitCacheItem { Id = unitId, OwnerId = 101L, SessionId = sessionId };

            // 缺少 UploaderChatObjectId 的私有附件凭证拒绝用于发送
            await Assert.ThrowsAsync<BusinessException>(() =>
                (Task)method.Invoke(messageManager, [senderUnit, new FileContent { BlobId = unownedBlobId }])!);

            // ParentBlobId 不匹配的私有衍生图拒绝用于发送
            await Assert.ThrowsAsync<BusinessException>(() =>
                (Task)method.Invoke(messageManager, [senderUnit, new ImageContent { BlobId = validPrimaryId, ThumbnailBlobId = foreignThumbId }])!);
        });
    }

    [Fact]
    public async Task LegacyPublicBlobAuditService_backfills_refs_and_privatizes_only_confirmed_chat_blobs()
    {
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var auditService = ServiceProvider.GetRequiredService<ILegacyPublicBlobAuditService>();

        var sessionA = guidGen.Create();
        var unitA = guidGen.Create();
        var chatLegacyBlobId = guidGen.Create();
        var portraitBlobId = guidGen.Create();
        var orphanPublicChatBlobId = guidGen.Create();

        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await MessageVisibilityQuery_Tests.InsertAsync<Session>(db, new() { ["Id"] = sessionA });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnit>(db, new() { ["Id"] = unitA, ["SessionId"] = sessionA, ["OwnerId"] = 101L });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnitSetting>(db, new() { ["SessionUnitId"] = unitA, ["SessionId"] = sessionA, ["IsEnabled"] = true });

            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = chatLegacyBlobId,
                ["Container"] = "chat-files",
                ["Name"] = "2024/05/legacy-chat.pdf",
                ["FileName"] = "legacy-chat.pdf",
                ["MimeType"] = "application/pdf",
                ["Suffix"] = ".pdf",
                ["IsPublic"] = true,
                ["Status"] = BlobStatus.Temp,
                ["RefCount"] = 0,
            });
            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = portraitBlobId,
                ["Container"] = "chat-object-portraits",
                ["Name"] = "101/avatar.png",
                ["FileName"] = "avatar.png",
                ["MimeType"] = "image/png",
                ["Suffix"] = ".png",
                ["IsPublic"] = true,
                ["Status"] = BlobStatus.Bound,
                ["RefCount"] = 1,
            });
            await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
            {
                ["Id"] = orphanPublicChatBlobId,
                ["Container"] = "chat-files",
                ["Name"] = "2024/05/unclassified.png",
                ["FileName"] = "unclassified.png",
                ["MimeType"] = "image/png",
                ["Suffix"] = ".png",
                ["IsPublic"] = true,
                ["Status"] = BlobStatus.Bound,
                ["RefCount"] = 0,
            });

            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new()
            {
                ["Id"] = 7001L,
                ["SessionId"] = sessionA,
                ["SessionSeq"] = 1L,
                ["SenderId"] = 101L,
                ["SenderSessionUnitId"] = unitA,
                ["MessageType"] = MessageTypes.File,
                ["ExtraProperties"] = new ExtraPropertyDictionary(),
            });
            var fileContent = new FileContent { BlobId = chatLegacyBlobId, Url = $"/api/chat/file/{chatLegacyBlobId}", FileName = "legacy-chat.pdf" };
            SetProp(fileContent, nameof(FileContent.Id), guidGen.Create());
            db.Set<FileContent>().Add(fileContent);
            (await db.Message.SingleAsync(x => x.Id == 7001L)).FileContentList.Add(fileContent);
            await db.SaveChangesAsync();
        });

        var dryRunResult = await auditService.AuditAndMigrateBatchAsync(dryRun: true);
        Assert.Equal(3, dryRunResult.CandidateCount);
        Assert.Equal(1, dryRunResult.MissingRefCount);
        Assert.Equal(2, dryRunResult.UnclassifiedCount);
        Assert.Equal(0, dryRunResult.BackfilledRefCount);
        Assert.Equal(0, dryRunResult.PrivatizedCount);

        var applyResult = await auditService.AuditAndMigrateBatchAsync(dryRun: false);
        Assert.Equal(1, applyResult.ConfirmedChatCount);
        Assert.Equal(1, applyResult.BackfilledRefCount);
        Assert.Equal(1, applyResult.PrivatizedCount);

        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var updatedChatBlob = await db.Blob.AsNoTracking().SingleAsync(x => x.Id == chatLegacyBlobId);
            Assert.False(updatedChatBlob.IsPublic);
            Assert.Equal(BlobStatus.Bound, updatedChatBlob.Status);
            Assert.Null(updatedChatBlob.ExpireTime);
            Assert.Equal(1, updatedChatBlob.RefCount);
            Assert.True((await db.Blob.AsNoTracking().SingleAsync(x => x.Id == portraitBlobId)).IsPublic);
            Assert.True((await db.Blob.AsNoTracking().SingleAsync(x => x.Id == orphanPublicChatBlobId)).IsPublic);
            Assert.True(await db.BlobMessageRef.AsNoTracking().AnyAsync(x => x.BlobId == chatLegacyBlobId && x.MessageId == 7001L));
        });

        // Re-running audit must be strictly idempotent
        var rerunResult = await auditService.AuditAndMigrateBatchAsync(dryRun: false);
        Assert.Equal(0, rerunResult.ConfirmedChatCount);
        Assert.Equal(0, rerunResult.BackfilledRefCount);
        Assert.Equal(0, rerunResult.PrivatizedCount);
    }

    [Fact]
    public async Task RootMessage_and_ChildMessage_authorization_rejects_mismatched_sibling_blob()
    {
        var guidGen = ServiceProvider.GetRequiredService<IGuidGenerator>();
        var authService = ServiceProvider.GetRequiredService<IBlobAccessAuthorizationService>();

        var sessionA = guidGen.Create();
        var unitA = guidGen.Create();
        var blob1Id = guidGen.Create();
        var blob2Id = guidGen.Create();

        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await MessageVisibilityQuery_Tests.InsertAsync<Session>(db, new() { ["Id"] = sessionA });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnit>(db, new() { ["Id"] = unitA, ["SessionId"] = sessionA, ["OwnerId"] = 101L });
            await MessageVisibilityQuery_Tests.InsertAsync<SessionUnitSetting>(db, new() { ["SessionUnitId"] = unitA, ["SessionId"] = sessionA, ["IsEnabled"] = true });

            foreach (var bId in new[] { blob1Id, blob2Id })
            {
                await MessageVisibilityQuery_Tests.InsertAsync<Blob>(db, new()
                {
                    ["Id"] = bId,
                    ["Container"] = "chat-files",
                    ["Name"] = $"2026/11/{bId:N}.png",
                    ["FileName"] = $"{bId:N}.png",
                    ["MimeType"] = "image/png",
                    ["IsPublic"] = false,
                    ["Status"] = BlobStatus.Bound,
                    ["RefCount"] = 2,
                });
            }

            foreach (var (mid, type) in new[] { (7101L, MessageTypes.Image), (7102L, MessageTypes.Image), (7103L, MessageTypes.History) })
            {
                await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new()
                {
                    ["Id"] = mid,
                    ["SessionId"] = sessionA,
                    ["SessionSeq"] = mid - 7100L,
                    ["SenderId"] = 101L,
                    ["SenderSessionUnitId"] = unitA,
                    ["MessageType"] = type,
                    ["ExtraProperties"] = new ExtraPropertyDictionary(),
                });
            }

            var ic1 = new ImageContent { BlobId = blob1Id, Url = $"/api/chat/file/{blob1Id}" };
            SetProp(ic1, nameof(ImageContent.Id), guidGen.Create());
            var ic2 = new ImageContent { BlobId = blob2Id, Url = $"/api/chat/file/{blob2Id}" };
            SetProp(ic2, nameof(ImageContent.Id), guidGen.Create());
            db.Set<ImageContent>().AddRange(ic1, ic2);
            (await db.Message.SingleAsync(x => x.Id == 7101L)).ImageContentList.Add(ic1);
            (await db.Message.SingleAsync(x => x.Id == 7102L)).ImageContentList.Add(ic2);

            var hc = new HistoryContent(guidGen.Create(), "Pair", "Pair");
            db.Set<HistoryContent>().Add(hc);
            (await db.Message.SingleAsync(x => x.Id == 7103L)).HistoryContentList.Add(hc);
            db.Set<HistoryMessage>().AddRange(
                new HistoryMessage(hc, await db.Message.SingleAsync(x => x.Id == 7101L)),
                new HistoryMessage(hc, await db.Message.SingleAsync(x => x.Id == 7102L)));

            db.BlobMessageRef.AddRange(
                new BlobMessageRef(guidGen.Create(), blob1Id, 7101L, sessionA, BlobRefSourceTypes.Send),
                new BlobMessageRef(guidGen.Create(), blob2Id, 7102L, sessionA, BlobRefSourceTypes.Send),
                new BlobMessageRef(guidGen.Create(), blob1Id, 7103L, sessionA, BlobRefSourceTypes.History),
                new BlobMessageRef(guidGen.Create(), blob2Id, 7103L, sessionA, BlobRefSourceTypes.History));
            await db.SaveChangesAsync();
        });

        await WithUnitOfWorkAsync(async () =>
        {
            // Valid child1 + blob1 under root 7103 succeeds
            var ok = await authService.CheckAccessAsync(blob1Id, unitA, 7101L, 7103L, [101L], hasValidSignature: true);
            Assert.NotNull(ok);

            // Swapped child2 + blob1 under same root 7103 MUST fail
            await Assert.ThrowsAsync<BusinessException>(() =>
                authService.CheckAccessAsync(blob1Id, unitA, 7102L, 7103L, [101L], hasValidSignature: true));
        });
    }
}
