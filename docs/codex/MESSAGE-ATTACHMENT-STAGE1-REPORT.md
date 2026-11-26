# IczpNet.Chat 消息附件授权与去重第一阶段报告

## 1. 本阶段目标与设计决策

围绕 [MESSAGE-ATTACHMENT-DESIGN.md](./MESSAGE-ATTACHMENT-DESIGN.md) 第一阶段目标完成以下改动：

1. **聊天记录再次转发附件授权与事务原子性**
   - 将 `MessageSender.SendHistoryAsync` 中的附件绑定移入 `MessageManager.CreateMessageCoreAsync` 同一工作单元事务内（`BindAttachmentsAfterInsertAsync`），消除“消息先提交并推送、附件授权后补写”的竞态窗口。
   - 在 `MessageManager.BindAttachmentsAfterInsertAsync` 统一支持新建合并转发与 `History` 单条再次转发的 `History` 内部附件收集。
   - 收紧 `BindHistoryAttachmentsAsync`：遍历深度限为 1..5 级、最大访问消息数 500、强制校验证引用消息非私密（`!IsPrivate`）且未撤回（`!IsRollbacked && !RollbackTime.HasValue`），并对旧版结构化内容/附件表兜底收集。
   - `MessageRepository.BindBlobsToMessageAsync` 改为严格幂等：仅对实际新增的 `(BlobId, MessageId)` 插入 `BlobMessageRef` 并按增量累加 `Blob.RefCount`，复用 `(BlobId, MessageId)` 唯一索引防重。

2. **异步 GIF 生成与多级转发/聊天记录传播拆分**
   - 将 `GenerateVideoGifBackgroundJob` 拆分为“生成并原子写入 GIF（`PropagateVideoGifBackgroundJob` 触发）”与“下游传播重试 Job”。
   - 使用 `IAbpDistributedLock` 与 `IMessageRepository.TrySetVideoGifAsync`（基于 `GifBlobId == null` 的数据库条件更新）保证并发任务只发布一个有效 GIF。
   - 新增 `IMessageManager.PropagateVideoGifAsync`：按共享 `VideoContent` 游标分批查找各级转发消息，及沿 `HistoryMessage` 最深 5 层向上查找包含该视频的聊天记录根消息；幂等补写 `BlobMessageRef` 并在快照/引用变化时同步更新 `ContentJson` 与写入 `MessageChange`（提交后触发增量同步与缓存失效）。

3. **保护共享内容归属 (`OwnerId`) 与转发加载修复**
   - 在 `MessageManager.CreateMessageCoreAsync` 中将 `messageContent.SetOwnerId(senderSessionUnit.OwnerId)` 限定为仅在非转发消息（`!message.ForwardMessageId.HasValue`）执行，防止群成员转发他人的消息时覆盖原始共享 `MessageContent.OwnerId`。
   - 修复 `MessageManager.ForwardAsync` 改用 `Repository.GetCacheMissMessagesAsync([sourceMessageId])`（`AsNoTracking` + `LoadMessageContentsBatchAsync`），避免受跟踪的 `sourceMessage` 带着默认新建的 `DeletedCounter` 等计数器在外层 `UnitOfWork.CompleteAsync()` 时触发主键重复插入冲突。

4. **私有附件下载与签名统一鉴权及客户端兼容**
   - `FileController.SignAsync` 改为复用 `BlobAccessAuthorizationService.CheckAccessAsync` 执行与下载一致的上下文鉴权。
   - `BlobAccessAuthorizationService`：
     - `Temp` 附件严格校验 `ExpireTime >= Clock.Now`（过期的临时附件拒绝访问）。
     - 带 `su/msg/rootMsg` 上下文请求时通过 `IMessageRepository.CanReadAttachmentAsync` 校验发送/接收可见性、私密范围、清空/开始截止、个人删除 (`DeletedRecorder`)、撤回 (`!IsRollbacked && !RollbackTime.HasValue`) 以及 `BlobMessageRef` / 同会话引用关系。
     - 兼容已登录旧客户端只传 `BlobId`（无 `su/msg/rootMsg`）：即使上传者本人访问已绑定私有附件，也必须先验证当前账号所属任一活跃 `SessionUnit` 下至少存在一条可见且未撤回的目标消息，无可见消息（如已撤回/已被个人删除/无权会话）时拒绝访问。

---

## 2. 变更文件清单

- 修改：
  - `src/IczpNet.Chat.Domain/MessageSections/Messages/IMessageRepository.cs`
  - `src/IczpNet.Chat.EntityFrameworkCore/Repositories/MessageRepository.cs`
  - `src/IczpNet.Chat.Domain/MessageSections/Messages/IMessageManager.cs`
  - `src/IczpNet.Chat.Domain/MessageSections/Messages/MessageManager.cs`
  - `src/IczpNet.Chat.Domain/MessageSections/Messages/MessageManager.Attachments.cs`
  - `src/IczpNet.Chat.Domain/MessageSections/MessageSender.cs`
  - `src/IczpNet.Chat.Domain/Blobs/GenerateVideoGifBackgroundJob.cs`
  - `src/IczpNet.Chat.Domain/Blobs/BlobAccessAuthorizationService.cs`
  - `src/IczpNet.Chat.HttpApi/Controllers/FileController.cs`
- 新增：
  - `src/IczpNet.Chat.Domain/Blobs/PropagateVideoGifArgs.cs`
  - `src/IczpNet.Chat.Domain/Blobs/PropagateVideoGifBackgroundJob.cs`
  - `test/IczpNet.Chat.EntityFrameworkCore.Tests/EntityFrameworkCore/AttachmentAuthorizationAndForwarding_Tests.cs`
  - `test/IczpNet.Chat.Application.Tests/FileControllerAuthorization_Tests.cs`
  - `docs/codex/MESSAGE-ATTACHMENT-STAGE1-REPORT.md`

---

## 3. 验证命令与实测结果

在 `f:\Dev\abpvnext\IczpNet.Chat` 根目录执行（单节点串行参数 `-p:UseSharedCompilation=false -m:1 --no-restore`）：

1. **EF Core 集成测试**：
   ```powershell
   dotnet test test\IczpNet.Chat.EntityFrameworkCore.Tests\IczpNet.Chat.EntityFrameworkCore.Tests.csproj -p:UseSharedCompilation=false -m:1 --no-restore
   ```
   - **结果**：通过 37，失败 0，跳过 0，总计 37。
   - 覆盖新增加入的 4 个回归测试（嵌套 `History` 转发授权与私密/撤回过滤、多级转发与聊天记录 `PropagateVideoGifAsync` 幂等传播、转发保持源 `MessageContent.OwnerId`、`BlobAccessAuthorizationService` 上下文与撤回/个人删除/过期 Temp 规则）。

2. **Application 与 HttpApi 控制器集成测试**：
   ```powershell
   dotnet test test\IczpNet.Chat.Application.Tests\IczpNet.Chat.Application.Tests.csproj -p:UseSharedCompilation=false -m:1 --no-restore
   ```
   - **结果**：通过 78，失败 0，跳过 2（既有的 `SampleAppService_Tests`），总计 80。
   - 覆盖 `FileControllerAuthorization_Tests`（合法 `su/msg` 上下文下载与签名、旧版仅 `BlobId` URL 兼容放行、无权/撤回/错误根消息拒绝访问）。

3. **Domain 单元测试（附件/可见性/幂等相关套件）**：
   ```powershell
   dotnet test test\IczpNet.Chat.Domain.Tests\IczpNet.Chat.Domain.Tests.csproj --filter "FullyQualifiedName~BlobSignature|FullyQualifiedName~BlobResolver|FullyQualifiedName~AttachmentUploadValidator|FullyQualifiedName~MessageVisibility|FullyQualifiedName~MessageIdempotency" -p:UseSharedCompilation=false -m:1 --no-restore
   ```
   - **结果**：通过 14，失败 0，跳过 0，总计 14。
   - **全量补充说明**：全量运行 `IczpNet.Chat.Domain.Tests` 时通过 218、跳过 22（既有依赖真实 Redis 的用例）、失败 1（既有非本阶段改动的 `MessageSenderHydration_Tests.Cached_messages_and_quotes_share_one_batch_sender_lookup`，断言 `GetCacheManyAsync` 调用 1 次实际收到 2 次）。

4. **宿主编译验收**：
   ```powershell
   dotnet build host\IczpNet.Chat.HttpApi.Host\IczpNet.Chat.HttpApi.Host.csproj -p:UseSharedCompilation=false -m:1 --no-restore
   dotnet build host\IczpNet.Chat.BackgroundWorker\IczpNet.Chat.BackgroundWorker.csproj -p:UseSharedCompilation=false -m:1 --no-restore
   ```
   - **结果**：均成功生成，0 警告，0 错误。

---

## 4. 客户端兼容性确认

- **旧版只传 `/api/chat/file/{blobId}` 的已登录客户端**：经 `BlobAccessAuthorizationService` 查同账号名下活跃 `SessionUnit` 是否存在至少一条可见且未撤回的消息（含旧结构化附件兼容表），兼容放行合法历史访问并在撤回/个人删除后拒绝。
- **新版带 `?su=...&msg=...&rootMsg=...` 与签名 URL (`?exp=...&sig=...`)**：`SignAsync` 与 `GetAsync` 均走统一 `CheckAccessAsync`，签名下载时仍实时复核消息可见性、个人删除与撤回状态。
- **Temp 上传预览**：在未过期的上传有效期内允许上传身份预览，过期后拒绝。

---

## 5. 已知风险与下阶段衔接

- 历史库中若存在早年缺少 `BlobMessageRef` 的旧消息，当前读链路已做 `BlobId` 结构化回查兼容；建议第二阶段后按消息 ID 范围低峰分批回填 `BlobMessageRef` 并观测日志。
- 上述第一阶段回归及两项宿主编译全部通过，已满足进入 **第二阶段（服务端物理文件去重）** 与 **第三阶段（多身份上传复用及客户端缓存优化）** 的验收门槛。
