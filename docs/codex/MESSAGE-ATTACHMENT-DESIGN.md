# IczpNet.Chat 消息附件授权与去重实施方案

消息附件采用 **物理文件去重、上传凭证按身份隔离、下载按消息授权** 的方案。第一阶段修复转发与下载鉴权，并验证现有客户端。相关项目编译、回归测试和客户端兼容验收通过后，再实施服务端存储去重。第三阶段优化同账号多身份的上传流量与客户端缓存。

同一账号的两个身份分别在同一个群发文件时，各自持有上传凭证，底层可以共享文件字节。多个身份收到同一条群消息时，使用同一个 `BlobId`，下载时各自按消息上下文鉴权。普通转发沿用源附件，并在目标消息上建立独立的下载授权。

## 第一阶段 转发与私有附件授权

### 聊天记录再次转发

普通图片、文件、语音和视频转发已经复用原 `BlobId`，并通过 `BlobMessageRef` 给目标消息授权。单条转发 `History` 时，通用绑定未收集内部附件，目标端按 `rootMsg` 下载可能缺少授权。当前合并转发已支持有限深度收集附件，但引用消息的会话和可见性仍需在收集时明确校验。 [转发入口](../../src/IczpNet.Chat.Domain/MessageSections/Messages/MessageManager.cs#L951) · [附件绑定](../../src/IczpNet.Chat.Domain/MessageSections/Messages/MessageManager.Attachments.cs#L182)

- 在 `MessageManager` 的消息创建流程内处理 `History` 附件。将源聊天记录已授权、且允许随记录展示的附件绑定到目标根消息。历史数据缺少引用时，沿 `HistoryMessage` 和明确允许展示的同会话引用批量收集，按消息去重并设置遍历深度和工作量上限。
- 合并转发和再次转发共用同一套收集规则。新选入的消息按操作身份校验消息可见性、私密范围、撤回和个人删除。再次转发已有记录时，按该身份能够查看的根记录及其允许展示的内容授权。目标会话成员通过目标根消息访问附件。
- 目标消息、附件引用以及引用计数应在同一事务提交，提交后再推送。合并转发当前在发送返回后另行绑定附件，实施时应将绑定移入消息提交边界。复用现有 `(BlobId, MessageId)` 唯一约束，只有实际新增引用才增加 `RefCount`。 [合并转发入口](../../src/IczpNet.Chat.Domain/MessageSections/MessageSender.cs#L186) · [引用索引](../../src/IczpNet.Chat.Domain/Blobs/BlobMessageRef.cs#L12)

### 异步 GIF 同步

`GenerateVideoGifBackgroundJob` 当前只补一级转发的 GIF 引用，且只刷新原消息的 `ContentJson` 和 `MessageChange`。通过共享 `VideoContent` 的消息关联找到各级普通转发，再沿 `HistoryMessage` 关系反查受影响的聊天记录根消息。查询应使用索引和游标分批执行。 [GIF Job](../../src/IczpNet.Chat.Domain/Blobs/GenerateVideoGifBackgroundJob.cs#L119) · [内容关联映射](../../src/IczpNet.Chat.EntityFrameworkCore/EntityFrameworkCore/ChatDbContextModelCreatingExtensions.cs#L504)

- 将 GIF 生成与下游传播做成可重试的两个步骤。持久化传播任务或进度，复用已有持久化 Job 机制。重跑时发现 GIF 已存在，仍应补完待处理的引用和变更。用数据库条件更新或现有分布式协调保证并发 Job 只发布一个有效 GIF。
- 对各级转发补写 GIF 引用并刷新保存媒体字段的快照。对已有聊天记录补写根消息授权，并按其实际读取方式刷新快照或让客户端重新拉取。沿用 `MessageChange` 和缓存失效流程，事务提交后发布变更。仅在引用或快照实际发生变化时写入变更。
- 覆盖生成期间新增普通转发或合并转发的竞态。新消息从共享内容读取已就绪的 GIF，生成任务负责补齐先提交的消息。两条路径共用幂等绑定，并用续扫或提交后补偿处理交错提交。

### 保护共享内容归属

仅在创建新内容时执行 `SetOwnerId`。转发原内容时，源内容的 `OwnerId` 保持原作者归属，新消息独立记录转发者及目标会话。验收时使用群成员转发另一成员消息的用例。现有转发入口对操作身份的检查允许这一场景。 [内容归属赋值](../../src/IczpNet.Chat.Domain/MessageSections/Messages/MessageManager.cs#L517) · [转发入口](../../src/IczpNet.Chat.Domain/MessageSections/Messages/MessageManager.cs#L951)

### 下载鉴权与客户端兼容

签名签发与下载共用附件授权规则，复用 `FileController` 和 `BlobAccessAuthorizationService`。保留现有路由、签名参数和响应格式。当前无上下文的私有附件兼容分支主要按上传者和会话成员资格判断，需要收口到实际可见的消息。 [下载与签名接口](../../src/IczpNet.Chat.HttpApi/Controllers/FileController.cs#L31) · [附件授权服务](../../src/IczpNet.Chat.Domain/Blobs/BlobAccessAuthorizationService.cs#L66)

| 请求场景 | 实施规则 |
| --- | --- |
| 签名签发及带 `su/msg/rootMsg` 的下载 | 校验当前身份或签名、签名时效、根消息或普通消息可见性，以及该附件与消息的授权关系。私密收件范围、历史窗口、清空、个人删除和撤回应明确覆盖。签名下载仍需重新检查消息可见性。 |
| 已登录旧客户端只传 `BlobId` | 在同账号所属身份中，按有索引的附件引用检测是否存在一条当前可见且未撤回的消息。查找使用有界查询，保留旧 URL。上传者的已绑定私有附件同样经过消息授权。 |
| 早期私有消息缺少 `BlobMessageRef` | 通过结构化的附件字段和消息关联精确反查，并执行同样的消息可见性校验。分批回填引用。历史兼容查询采用精确字段匹配。 |
| 未发送的 Temp 附件及已公开资源 | Temp 预览校验所属身份与有效期。现有公开资源保持原协议，逐步将旧 `send-upload-*` 创建的聊天私有附件纳入统一流程。先核对仍依赖公开 URL 的客户端和历史资源。 |

撤回按当前消息处理，独立且仍有效的转发按自身上下文鉴权。个人删除只影响该身份对应的消息可见性。MinIO 预签名直链在有效期内可继续使用，需保持短时效。若产品要求撤回后立即阻断已签发链接，相关私有资源应通过应用鉴权后代理读取。

### 第一阶段验证与上线门槛

业务改动落在 Chat 的 Domain、EntityFrameworkCore 和 HttpApi。Application 沿用现有消息入口和 DTO，主 Host 负责配置与装配。先复用现有引用约束、事务和消息变更机制。如果持久化补偿或查询确需扩展表结构或索引，应单独评审迁移。完成第一阶段实现后，从 Chat 仓库根目录运行受影响项目的构建，并执行相关测试。

```powershell
dotnet build src/IczpNet.Chat.Domain/IczpNet.Chat.Domain.csproj
dotnet build src/IczpNet.Chat.EntityFrameworkCore/IczpNet.Chat.EntityFrameworkCore.csproj
dotnet build src/IczpNet.Chat.HttpApi/IczpNet.Chat.HttpApi.csproj
dotnet test test/IczpNet.Chat.Domain.Tests/IczpNet.Chat.Domain.Tests.csproj --filter 'FullyQualifiedName~BlobSignature|FullyQualifiedName~AttachmentUploadValidator|FullyQualifiedName~TemplateMessagePipelineReview'
dotnet test test/IczpNet.Chat.EntityFrameworkCore.Tests/IczpNet.Chat.EntityFrameworkCore.Tests.csproj --filter 'FullyQualifiedName~MessageVisibilityQuery|FullyQualifiedName~MessageChangePersistence'
```

增加下表所需的回归用例，并将其纳入测试过滤范围。签名和下载走实际 HTTP 入口做集成验证，至少覆盖现有 Flutter 的旧 URL 与带消息上下文的请求。执行结果应记录实际命令和通过情况。

| 验收场景 | 通过条件 |
| --- | --- |
| 合并转发后再次转发，含嵌套聊天记录 | 目标端在根消息上下文下能下载已授权媒体，私密或不可见引用未被带出。历史缺引用的合法附件可以恢复引用。 |
| 原消息转发两次后才生成 GIF，期间有合并转发 | 各级普通转发和聊天记录可访问 GIF，增量同步后的客户端可渲染。并发执行或重跑时，同一消息附件关系只有一条，计数一致。 |
| 成员转发另一成员的消息 | 源 `MessageContent.OwnerId` 保持原作者归属，新消息显示正确的转发者。 |
| 新旧客户端获取私有附件 | 合法的旧 URL、带上下文下载和签名下载继续工作。无消息权限、错误根消息、撤回消息和个人删除场景按各自授权规则拒绝。 |
| 事务失败与多实例竞态 | 消息与授权原子提交。中途失败可重试，无已推送却未授权的消息，也无重复计数。 |

以上验证通过、客户端兼容结果确认后，再进入第二阶段。有真实数据回填时，先按消息 ID 范围验证，再分批执行并观测旧链接的失败率、Job 积压以及引用写入冲突。

## 第二阶段 服务端物理文件去重

- 保留逻辑 `Blob` 的 `BlobId`、上传身份、文件名、附件角色、父子关系以及 `Temp/Bound` 生命周期。通过可空的 `StorageObjectId` 关联 `BlobObject`。旧数据继续按 `Container + Name` 读取，普通转发沿用原 `BlobId`。
- 新增物理对象表，以存储隔离域、容器、服务端计算的 SHA-256 和字节数建立唯一约束。上传经现有验证器校验后，再按实际存储字节去重。原图、压缩图、缩略图和封面分别匹配。后续如需节省转码，以源摘要、完整处理参数和算法版本定位衍生结果，区分系统生成和用户自传封面。
- 使用 `Pending → Ready → Deleting` 状态协调数据库与对象存储。只有 `Ready` 对象可以新增逻辑引用。临时附件过期与发送绑定用条件更新协调，物理回收先在数据库确认无逻辑引用并标记 `Deleting`，再由 Worker 分批删对象。清理按带版本的实际对象键执行，避免旧任务重试时误删后续上传对象。Worker 可恢复超时写入和删除，并能从数据库核对引用计数。
- 通过 Chat SchemaMigrator 交付正式迁移，先部署兼容读路径及新版 API、Worker，再开新写入。发布验收覆盖 MinIO 或文件系统读取、短时直链、原下载文件名、并发上传和清理重试。历史数据以分批回填方式迁移。若需要关闭新写入，保留新版读取和清理能力直至共享对象完成处置。

## 第三阶段 多身份上传与客户端缓存

- 同账号复用一次已校验上传的结果。服务端签发账号绑定的短期凭据，兑换时重新校验目标 `SessionUnit` 的归属、发言权和附件策略，为新身份签发自己的 Temp 主附件及子附件。客户端用 `ClientUploadId` 在账号、身份、目标会话和上传参数的作用域内重试。群成员资格和文件摘要均不足以兑换附件。
- 客户端优先按「服务端实例 + 登录账号 + `BlobId`」缓存私有媒体字节，在使用缓存前确认当前消息的访问资格，避免把短时签名参数作为缓存标识。不同 `BlobId` 指向同一物理内容时，可后续使用鉴权成功后返回的账号作用域内不透明标识复用字节。切换账号时隔离缓存。

验收示例为同一账号的身份 A、B 在同一群各发一条消息。客户端只上传一次原文件，服务端生成两套逻辑附件凭证并共享实际存储对象。两个身份查看同一条群消息或其普通转发时，只需下载一份对应 `BlobId` 的字节，每次展示仍须满足当前消息的访问条件。
