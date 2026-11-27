# 消息附件生产评估与修复计划

评估日期为 2026-11-27。前序复查基准为 IczpNet.Chat `da66290d`（配置 `a0812a19`、部署 `3340c3d`）、Electron `8c5767c`、Flutter `c395cac`。部署拓扑参照 Rctea.IM 的 Chat API、Chat Worker 和 SchemaMigrator 配置。范围覆盖物理文件去重、按身份签发逻辑附件凭证、按消息授权下载，以及普通转发、聊天记录、异步 GIF 和客户端缓存。

**发布结论：三端 P1 代码修复与本地自动化验证已完成，暂未达到直接放量生产标准。** 当前核心阻塞已从“客户端/服务端 P1 授权缺口”收敛为：
1. `Chat_Module_v3` 仅完成了历史公开附件 `--dry-run` 盘点，尚未执行备份后的 `--apply` 实写迁移，且 2,448 条未关联活跃消息的公开 `chat-files` 仍待按历史用途处置；
2. 需用本轮后端代码重新打包/发布 `chat-api` 与 `chat-worker` 容器镜像，并在目标预生产环境完成真实登录态 JWT 三端联调、Windows NSIS 实际升级、macOS/Android/iOS 打包签名与双实例并发故障注入验收。

## 2026-11-27 P1 修复与验证结论

本轮按 **后端 → Electron → Flutter** 顺序完成全部 P1 发布前修复，未改动业务表结构：

| 阶段 | 已完成修复 | 最新验证结果 | 剩余未闭环项 |
| --- | --- | --- | --- |
| 后端（基于 `da66290d`） | `LegacyPublicBlobAuditService` 改为写事务内 `.AsNoTracking()` 重检并幂等调用 `BindBlobsToMessageAsync` 同步 `RefCount/Status/ExpireTime`；在 `SchemaMigrator` 接入 `--audit-legacy-public-blobs`（含 `sp_getapplock`）；`BlobAccessAuthorizationService` 强制校验 `(childMsgId, blobId)` 真实归属；`BlobAppService` 禁止经由 CRUD 将聊天附件公开或直接删除受保护对象；`MessageSenderController` 旧上传入口全量接入 `IAttachmentUploadValidatorManager`。 | `SchemaMigrator`、`BackgroundWorker`、`HttpApi.Host` 全部 **0 警告 0 错误** 构建通过；定向单测 **32 项全部通过**（Application `10/10`、EF Core `14/14`、Domain `8/8`）；真实库 `Chat_Module_v3` `--dry-run` 7 批扫完：`Candidates=3145`、`ConfirmedChat=318`、`MissingRefs=320`、`Unclassified=2827`。 | 尚未执行备份后的 `--apply` 实写与旧客户端兼容实测；2,448 条未关联公开 `chat-files` 仍需确认用途；本地运行中的 Docker `chat-api` / `chat-worker` 镜像尚未按新代码重新发布；P2 项与 Vben 管理页待继续推进。 |
| Electron（基于 `8c5767c`） | 在线签权短缓存收紧至 **5 秒**（`AUTH_VERDICT_CACHE_TTL_MS = 5_000`）；`cacheDir` 私有缓存不再写入原始路径白名单并显式拦截 `?path=` 直读；协议解析缺失调用窗口默认拒绝，仅协议层经 `isWindowAlive(grant.webContentsId, activeAccount)` 核验存活与当前账号后放行；`preload` 新增绑定可信 `event.sender.id` 的 `authorizeLocalMedia`；复制图片、另存为与定位文件传 `bypassVerdictCache: true`；撤回/删消息/状态恢复即时触发 `invalidateMessageAccess`。 | `pnpm run typecheck`（node + web）通过；`pnpm exec vitest run test/unit test/integration` **68 个测试文件、335 项测试全部通过**。 | 尚待用真实打包安装包完成 `v0.1.0 -> v0.1.1` NSIS 安装/升级、多账号同会话真实 JWT 联调及有 Apple 凭证环境下的 macOS 签名公证。 |
| Flutter（基于 `c395cac`） | `AttachmentTransferService` 将在线结论短缓存收紧至 **5 秒**，新增 `accessRevision`、`isProtectedChatAttachmentUrl` 与单次去重的 `forceVerifyOnline`（`open`/`saveAs` 对私有 `/api/chat/file/{blobId}` 强制在线重验并失败清理字节）；新增 `AuthorizedPrivateAttachmentGate` 接入 `ImageMessageContent` 与 `VideoMessageContent`，渲染本地缓存图/封面/GIF 前强检撤回/删除与在线鉴权并随 `accessRevision` 即时撤销；`HistoryMessageContent` 补全详情弹窗（子文本/图/视频/文件/语音/嵌套转发），将查看者 `sessionUnitId`、子消息 `serverId` 与外层可见 `rootMessageId`（同步透传 `VoiceMessageContent`）贯穿预览与传输。 | `flutter analyze --no-pub` 覆盖改动文件 **0 问题**；6 个附件与媒体相关测试文件共 **26 项测试全部通过**（含新增 UI 门禁、强制重验与 History 详情 `rootMsg` 传参用例）。 | 待在预生产/测试环境运行真实登录态 JWT 的桌面与移动端回归、多端撤回实时 Push 验证与大工程全量构建发布。 |
| Vben 管理端 | 后端 `BlobAppService` 写保护门槛已补齐。 | 已含在 `BlobAppServiceProtection_Tests` 定向验证中。 | Vben 管理界面与管理专用 API/预检审计仍在后续独立交付范围。 [附件管理方案](MESSAGE-ATTACHMENT-VBEN-ADMIN-PLAN.md) |

### 已复核的配置、测试和产物（本轮最新实测）

- **真实库只读盘点（`Chat_Module_v3`）**：执行 `dotnet run --no-build --project host/IczpNet.Chat.SchemaMigrator/IczpNet.Chat.SchemaMigrator.csproj -- --audit-legacy-public-blobs --dry-run --batch-size=500` 返回退出码 `0`（7 批扫描无异常，未修改业务行）。指标较前次确认一致：`Candidates=3,145`（`ConfirmedChat=318`、`MissingRefs=320`、`Unclassified=2,827`；其中历史盘点分类含未关联活跃消息的公开 `chat-files` `2,448` 与其他容器 `379`，且公开 Temp 带到期时间为 `0`）。
- **本轮代码变更后的实际命令验证汇总**：
  - 后端构建：`IczpNet.Chat.SchemaMigrator`、`IczpNet.Chat.BackgroundWorker`、`IczpNet.Chat.HttpApi.Host` 均构建成功（0 错误）。
  - 后端定向测试：
    - `IczpNet.Chat.Application.Tests`（`AttachmentReuse_Tests|FileControllerAuthorization_Tests|MessageSenderUploadAuthorization_Tests|BlobAppServiceProtection_Tests`）：**10 通过，0 失败，0 跳过**；
    - `IczpNet.Chat.EntityFrameworkCore.Tests`（`AttachmentAuthorizationAndForwarding_Tests|BlobDeduplication_Tests`，SQLite 内存库）：**14 通过，0 失败，0 跳过**；
    - `IczpNet.Chat.Domain.Tests`（`MessageIdempotency_Tests|MessageVisibility_Tests`）：**8 通过，0 失败，0 跳过**。
  - Electron：`pnpm run typecheck` + `pnpm exec vitest run test/unit test/integration`：**68 文件 335 项测试通过（0 失败）**（含 `media-cache-service.test.ts` 与 `shell-ipc-security.test.ts` 新增的 5 秒 TTL、原始路径拦截、窗口句柄校验及导出绕过缓存防护断言）。
  - Flutter：`flutter analyze --no-pub` 改动范围 0 issue；`flutter test --no-pub` 执行 `attachment_cache_key_test.dart`、`attachment_reuse_client_test.dart`、`attachment_local_reuse_test.dart`、`core/image_offline_cache_test.dart`、`chat_media_layout_test.dart` 与 `core/attachment_private_ui_gate_test.dart`：**26 项全部通过（0 失败）**。
- **生产密钥与容器状态提醒**：`Chat Worker` 与 `Chat API` 仓库配置中仍保留开发测试 `CHAT_ATTACHMENT_SIGNING_KEY` 默认值（900 秒），上线前须确保通过机密管理显式注入并取消生产默认回退；如测试密钥曾外泄须轮换。此外，本轮修改后 `deploy/` 中 `chat-api` / `chat-worker` Docker 镜像尚未重建推送。

### 历史公开附件盘点（前次细项 + 本轮 `SchemaMigrator` 复核一致）

以下为部署方的同口径只读结果。统计关系已核对，数据库记录及实写尚待操作方验证。

| 指标 | 结果 |
| --- | ---: |
| 有效 Blob | 5,431 |
| 公开候选 | 3,145 |
| 已关联活跃消息结构化字段的公开聊天附件 | 318 |
| 上述附件缺失的消息引用 | 320 |
| 未归类 | 2,827 |
| 未关联活跃消息的公开 `chat-files` | 2,448 |
| 其他容器 | 379 |
| 公开 Temp 且有到期时间 | 0 |

其他容器包括 `chat-object-portraits` 236 条、`user-portraits` 89 条、`resized` 54 条。公开历史附件当前没有到期时间，所以这批记录不会被按过期 Temp 的查询认领。**未关联活跃消息只表示盘点当前未找到结构化关联。** 2,448 条记录须继续核对旧客户端引用、公开用途和保留期，再决定迁移、归档或回收。318 条确认记录先修正迁移写入逻辑，再小批试跑，核对补引用、状态、计数，以及旧客户端登录态访问结果。

### 下一轮放量门槛

1. 后端关闭旧 CRUD 对聊天私有附件的公开化与直接删除能力。补齐审计命令入口，回填和私有化同事务提交，验证状态、计数、重试和旧客户端兼容。下载和签名对指定的子消息与 Blob 关系做精确判断。
2. Electron 仅通过带账号、可信窗口、消息上下文和时效的授权读取私有本地缓存。Flutter 将图片、视频封面和 History 中的查看、播放及保存收口到同一检查。两个客户端验收撤回、失权和切换账号后重新打开。
3. 以真实登录态 JWT 完成同账号双身份、异账号、嵌套 History 和多级转发的跨端联调。补 HTTP 级并发兑换、真实 GIF 传播和同一 Blob 的到期边界测试。
4. 完成 Windows NSIS 实际安装升级。macOS、Android、iOS 按本次发布目标完成对应平台构建和适用的签名／公证。Vben 的管理写操作须另外通过预检、并发及审计验收。

## 三条链路的评估

| 能力 | 当前证据 | 生产判定 |
| --- | --- | --- |
| 物理文件去重 | 服务端 SHA-256，隔离域和容器内的活跃唯一约束，版本化对象键，旧附件读取兼容。过期逻辑附件已改为数据库条件认领，物理对象计数与逻辑附件在事务路径内提交，Worker 已补扫无实际引用的 Ready 对象。 | **有条件通过代码评估。** 普通上传入口、回收失败和灰度开关仍需验证。 |
| 上传凭证按身份隔离 | 同账号兑换凭证经服务端验证后，按目标 SessionUnit 签发独立逻辑 Blob。数据库已有兑换唯一键和冲突后重读，Flutter 和 Electron 均已接入兑换。 | **主流程可用。** 历史无归属 Blob、Flutter 业务入口的幂等键以及派生参数匹配仍存在缺口。 |
| 下载按消息授权 | 服务端已按 BlobMessageRef、消息可见性和结构化历史字段校验，跨会话聊天记录改为逐跳验证。Electron 已按来源发送令牌，缓存已有消息授权入口。 | **未通过。** 旧发送上传已改为私有，历史公开数据、通用 Blob 管理写入口，以及客户端缓存与本地路径仍有权限缺口。 |

后端上轮的两个并发问题已有实质整改。回收用 SQL 条件更新决定是否能删除过期 Temp，物理引用增量也移至逻辑写入所在工作单元。GIF 生成结果改为在业务事务内登记外箱，Chat Worker 定时处理。Electron 原有远程媒体 Bearer 发送问题已按 API 同源限制，并逐跳处理重定向。Flutter 先前缺失的同步进度文件已进入当前提交树。 [临时附件认领](../../src/IczpNet.Chat.EntityFrameworkCore/Repositories/MessageRepository.cs#L783) · [物理引用写入](../../src/IczpNet.Chat.Domain/Blobs/BlobManager.cs#L185) · [GIF 外箱写入](../../src/IczpNet.Chat.Domain/Blobs/GenerateVideoGifBackgroundJob.cs#L207)

## P1 发布前修复状态（已完成代码修复与单测/集成验证）

### P1-01 旧聊天附件和通用 Blob 写入口保护与历史回填（已修复待库实写）

- **已落地修复**：[LegacyPublicBlobAuditService.cs](../../src/IczpNet.Chat.Domain/Blobs/LegacyPublicBlobAuditService.cs) 在写入事务内基于 `.AsNoTracking()` 重新确认活跃消息关联并幂等调用 `IMessageRepository.BindBlobsToMessageAsync(..., markAsBound: true)`，同步更新 `IsPublic = false`、`Status = BlobStatus.Bound`、`ExpireTime = null` 与 `RefCount`；[LegacyPublicBlobAuditCommand.cs](../../host/IczpNet.Chat.SchemaMigrator/LegacyPublicBlobAuditCommand.cs) 与 `Program.cs` 接入 `--audit-legacy-public-blobs [--dry-run | --apply]` 与分布式应用锁；[BlobAccessAuthorizationService.cs](../../src/IczpNet.Chat.Domain/Blobs/BlobAccessAuthorizationService.cs) 对传入 `rootMessageId` + `childMessageId` 强制核对 `BlobId` 是否确实隶属目标 `childMessageId`；[BlobAppService.cs](../../src/IczpNet.Chat.Application/Blobs/BlobAppService.cs) 封堵聊天/绑定/引用/共用存储对象 Blob 的公开化及直接 CRUD 删除；[MessageSenderController.cs](../../src/IczpNet.Chat.HttpApi/Controllers/MessageSenderController.cs) 的 4 条旧文件发送上传端点均统一接入 `IAttachmentUploadValidatorManager`。
- **仍待验收**：在 `Chat_Module_v3` 备份后执行 `--audit-legacy-public-blobs --apply`，评估并处置 2,448 条无活跃消息结构化引用的公开 `chat-files`，并重建部署 `chat-api` / `chat-worker` 镜像。

### P1-02 Electron 在线缓存校验、5 秒撤回窗口与本地句柄收口（已修复待端到端联调）

- **已落地修复**：[MediaCacheService.ts](F:/Dev/GotoIM/gotoim-electron/src/main/services/MediaCacheService.ts) 将 `AUTH_VERDICT_CACHE_TTL_MS` 收紧至 `5_000` ms；从 `ensureRemoteCached` 与 `authorizeLocalMedia` 移除所有私有缓存目录原始路径放行，并禁止 `cacheDir` 内路径通过 `?path=` 原始路径白名单；`resolveAuthorizedLocalRequest` 对缺少可信 `webContentsId` 的请求默认 fail-closed（仅在协议层经 `isWindowAlive(grant.webContentsId, activeAccount)` 验证窗口存活且当前账号一致时放行）；[registerShellIpc.ts](F:/Dev/GotoIM/gotoim-electron/src/main/windows/registerShellIpc.ts) 复制图片/另存为/定位文件均传 `bypassVerdictCache: true` 强制在线重验；[MessageLocalService.ts](F:/Dev/GotoIM/gotoim-electron/src/main/modules/messages/MessageLocalService.ts) 接入 `invalidateMessageAccess` 随撤回、删除与状态恢复即时撤销访问凭证。
- **仍待验收**：已打包客户端的多账号同会话、视频 Range 拖动与真机撤回实时联调。

### P1-03 Electron 聊天记录与媒体导出控制（已接入统一入口）

- P1 要求的原始路径清理、句柄与 `AUTHORIZE_LOCAL_MEDIA` 通道、复制/导出在线强制重验已在上述修改中闭环；P2 涉及的嵌套聊天记录媒体类型全量渲染持续跟踪。

### P1-04 Flutter 图片/视频封面/History 详情统一授权上下文（已修复待真机回归）

- **已落地修复**：[attachment_transfer_service.dart](F:/Dev/GotoIM/gotoim-flutter/lib/core/services/file/attachment_transfer_service.dart) 将 `_authorizedVerdicts` 收紧至 5 秒并增加单次去重的 `forceVerifyOnline`、`isProtectedChatAttachmentUrl` 与 `accessRevision`；新增 [authorized_private_attachment_gate.dart](F:/Dev/GotoIM/gotoim-flutter/lib/features/chat/presentation/message_content/authorized_private_attachment_gate.dart) 并接入 [image_message_content.dart](F:/Dev/GotoIM/gotoim-flutter/lib/features/chat/presentation/message_content/image_message_content.dart) 与 [video_message_content.dart](F:/Dev/GotoIM/gotoim-flutter/lib/features/chat/presentation/message_content/video_message_content.dart)，阻止本地缓存/缩略图绕过校验并随撤回/删除即时收回界面渲染与预览点击；[history_message_content.dart](F:/Dev/GotoIM/gotoim-flutter/lib/features/chat/presentation/message_content/history_message_content.dart) 补全可点击详情弹窗并向子图片/视频/文件/语音/嵌套 History 透传查看者 `su`、子 `msg` 与外层可见 `rootMsg`。
- **仍待验收**：以真实 JWT 完成桌面与移动端全流程实测回归。

## P2 生产加固与运维

| 项目 | 当前缺口与修复建议 |
| --- | --- |
| Flutter 兑换幂等 | Flutter 已把 `local.clientMessageId` 传入 `sendUploadFile` 及发送接口。仍需将兑换的过期回退与权限、参数冲突区分，验证进程重启后同一次操作保持源附件及参数。 [业务入口](F:/Dev/GotoIM/gotoim-flutter/lib/features/chat/data/datasources/message_api.dart:493) |
| 物理对象异常恢复 | 旧版 Temp 的物理删除失败后只有日志，已经软删除的逻辑记录不会重新出现在普通扫描中。上传接管 `Deleting` 对象会替换旧键，旧键清理也是尽力执行。将旧键与失败删除记录持久化，回收任务分批重试，并对 SQL Server 下普通上传与兑换的事务边界做故障注入。 [旧对象清理](../../src/IczpNet.Chat.Domain/Blobs/TempBlobCleanupService.cs#L58) · [替换旧键](../../src/IczpNet.Chat.Domain/Blobs/BlobManager.cs#L170) |
| 灰度和运行配置 | 已增加独立兑换开关和私有直链开关，兑换按去重开关决定是否沿用 `StorageObjectId`。启动签名校验已接入。发布前仍要检查目标环境的生产密钥注入与开关状态。GIF 外箱由专用 Chat Worker 注册，生产健康检查应验证实际 Worker 实例及消费进度。 [存储分支](../../src/IczpNet.Chat.Domain/Blobs/BlobManager.cs#L66) · [签名配置](../../src/IczpNet.Chat.Domain/ChatDomainModule.cs#L55) · [外箱 Worker 注册](../../host/IczpNet.Chat.BackgroundWorker/ChatBackgroundWorkerModule.cs#L56) |
| 附件日志和直链 | Electron 已补 `sig` 脱敏。服务端私有预签名直链默认关闭。若生产启用直链，应确认有效期符合撤回要求，并验证日志和响应不会公开非必要的签名参数。 [媒体日志](F:/Dev/GotoIM/gotoim-electron/src/main/services/MediaCacheService.ts:1107) · [日志脱敏](F:/Dev/GotoIM/gotoim-electron/src/main/logging/AppLogger.ts:55) · [服务端直链](../../src/IczpNet.Chat.HttpApi/Controllers/FileController.cs#L84) |
| 派生参数与清理保留期 | 上传复用需要按视频封面、原图、版本等会改变产物的参数隔离。兑换记录与 GIF 外箱应配置保留和按索引分批清理，并监控缓存命中、孤儿、积压及失败。撤回和个人删除以消息可见性为准，物理回收仍须核对所有有效引用。 |
| 大附件负载 | 服务端普通上传仍有全量字节缓冲，Electron 缓存未命中会读取完整响应并同步写盘，查找时同步遍历目录。为最大允许文件和并发下载设置内存与 Main 线程延迟目标，验证后按需改为流式校验、落盘和索引查找。 [原文件上传](../../src/IczpNet.Chat.HttpApi/Controllers/AttachmentController.cs#L721) · [Electron 缓存](F:/Dev/GotoIM/gotoim-electron/src/main/services/MediaCacheService.ts:1107) |

## 修复实施顺序

**先完成后端契约和发布前回归，再修 Electron，最后修 Flutter。** 两个客户端统一采用后端在第一阶段交付的消息授权规则。后端可先部署支持新旧调用的版本，三个阶段均通过后再扩大私有缓存和共享写入的使用范围。

Vben 管理能力并入本次工作。第一阶段同步交付管理 API 和保护规则，Electron、Flutter 验收后接入 Vben 管理页面。面向生产的管理写操作须另行通过权限、预检和审计验收。 [Vben 聊天附件管理实施方案](MESSAGE-ATTACHMENT-VBEN-ADMIN-PLAN.md)

### 第一阶段 后端 IczpNet.Chat

| 顺序 | 入口 | 具体措施 |
| --- | --- | --- |
| B1 私有上传归属 | `AttachmentController`、`MessageSenderController`、`MessageManager.Attachments` | 从现有上传逻辑提取内部用例，供旧 `send-upload-*` 复用。保留旧路由、表单及响应，按当前登录账号验证 `SessionUnit` 的归属、状态和发言资格。所有新聊天附件以 `IsPublic=false` 创建。主附件和衍生件都记录上传身份、角色及过期时间。发送校验按服务端保存的归属和父子关系判断，并对私有聊天凭证明确拒绝缺少归属的数据。头像等已确认的公开资源保留独立用途。 |
| B2 下载和签名共用授权 | `FileController.GetAsync/SignAsync`、`BlobAccessAuthorizationService`、`MessageRepository.CanReadAttachmentAsync` | 保留 `GET /api/chat/file/{id}` 和 `POST /api/chat/file/sign`。签发及读取均用同一条决策链验证账号、目标身份、消息可见性和附件关系。有 `rootMsg` 时，校验目标根记录、子消息可达关系及子消息的附件或允许展示的引用。整条路径仅在合法根或直接消息授权后返回内容。已登录且只传 `BlobId` 的旧请求沿用 SQL 侧可见消息查询。签名返回沿用现有格式。私有资源使用独立的短时直链策略，如要求即时撤回，启用每次请求都经过应用鉴权的读取方式。 |
| B3 历史数据与兼容上线 | Chat SchemaMigrator、附件引用查询、配置 | 增加可断点续跑的历史核对流程，先输出候选量、缺引用量和无法归类量。按消息 ID 分批利用结构化内容及现有引用确认聊天附件，补引用后再取消其历史公开标记。对同时承担公开用途或无法可靠归类的记录单独登记，按资源用途迁移。开启严格旧入口前，实际验证受支持的旧客户端仍能完成上传和下载。 |
| B4 物理对象及兑换事务 | `BlobManager`、`TempBlobCleanupService`、`AttachmentController.ReuseAsync`、GIF 外箱 | 常规上传和兑换都显式保证逻辑 Blob 与物理引用增量在同一事务，Pending 记录、物理写入和 Ready 更新继续分阶段。回收在事务内认领并保留待删对象版本和旧键，成功删字节后确认，可重试并防止旧任务删除新对象。兑换在读缓存或数据库结果前复核账号、目标身份和源附件有效性。按数据库字段长度约束 `ClientUploadId`，并规定同一参数的重试返回同一可用结果、参数变化返回业务冲突、凭证过期返回可识别结果。独立验证复用开关与共享写入开关的实际作用范围。把签名密钥配置校验移至启动阶段，并核对 Chat Worker 的 GIF 外箱消费、重试与积压告警。 |

**给两个客户端的契约**

- 普通附件请求传实际 `blobId`、当前目标 `sessionUnitId` 和服务器 `messageId`。聊天记录附件额外传外层可见的 `rootMessageId`，子消息 ID 保持原值。长整型消息 ID 在客户端以无损形式传输。直接下载沿用现有 `su/msg/rootMsg` 名称，在线检查复用现有签名接口，并由后端统一访问判断。后端提供脱敏后的正常、拒绝、过期及历史兼容请求样例。
- `401` 按现有会话流程刷新一次。明确的权限拒绝、资源撤回或不存在不得从已有私有缓存取出字节。签名服务或网络故障也不隐式开启私有缓存读取，本轮生产默认要求在线获取访问结论。一个已授权的播放或保存操作可复用其上下文，独立的新操作重新取得访问结论。
- 兑换继续使用 `POST /api/chat/attachments/reuse/{sessionUnitId}`。`ReuseToken` 按账号验证，返回的逻辑 Blob 按目标身份归属。客户端为同一个发送动作固定 `ClientUploadId`，不同动作使用不同 ID。明确权限拒绝和参数冲突应上报给发送状态，按后端约定可恢复的过期场景才回退到正常上传。

**进入 Electron 的条件**　Chat API 与 Worker 构建通过。先跑附件授权、普通和合并转发、GIF 传播及兑换定向测试，再用预生产 SQL Server、Redis 与 MinIO 跑双实例并发。必须覆盖旧入口不再新增公开聊天附件、私密消息、跨会话嵌套 History、个人删除、撤回、无上下文旧 URL、同账号两身份、同文件并发上传与过期回收。在预生产执行 B3 的迁移前后核对存量数、拒绝率和文件存在性。迁移由现有 SchemaMigrator 执行并记录版本。

### 第二阶段 Electron

| 顺序 | 入口 | 具体措施 |
| --- | --- | --- |
| E1 统一 Main 授权 | `src/main/index.ts` 的 `authorizeCachedBlob`、`MediaCacheService.ensureRemoteCached` | 在线通过第一阶段接口核验完整上下文。SQLite 中的撤回、删除和不可访问状态用于提前拒绝。既有内容状态仅用于完成在线校验前的界面展示和拒绝。磁盘与下载仍可按账号、来源及 BlobId 共用字节，授权缓存和等待者按完整消息上下文隔离。下载开始固定账号和来源，切号后废弃旧任务结果。 |
| E2 所有读取和导出走同一入口 | `MediaCacheService`、`registerShellIpc.ts`、`mediaUrl.ts` | 让预览、Range、另存为和复制图片共用 E1 受保护的读取入口。删除直接按缓存根目录放行及绕过授权的磁盘查找分支。主进程从可信窗口与已授权消息或文件选择记录签发句柄，解析时校验账号、窗口、有效期和实际文件路径。关闭窗口、切号或撤回后使对应句柄失效。保留 API 同源令牌策略。日志记录失败原因和来源，敏感 URL 查询参数全部脱敏。 |
| E3 补齐渲染透传 | `HistoryDetailModal.vue`、`MediaApp.vue`、`shared/contracts/window.ts` | 沿用当前已开始透传的 `rootMessageId`，补查嵌套记录、更换预览项、GIF 更新、文件另存为和复制入口。每个动作将外层可见根消息、当前身份和子消息上下文传入 Main。继续用现有 SQLite Outbox 的发送操作 ID 作为兑换幂等键，并在续发时保留同一操作。 |

**进入 Flutter 的条件**　干净检出运行 `pnpm run typecheck` 和附件相关 Vitest，通过后用打包客户端验证受控测试账号。覆盖首次下载及命中缓存、同 Blob 双上下文并行、切号下载中断、已缓存附件撤回和个人删除、错误窗口／过期句柄、预览后复制与另存为、视频 Range 拖动、跨会话嵌套聊天记录以及旧链接在线兼容。留存经过脱敏的结果供 Flutter 复用相同场景。

### 第三阶段 Flutter

| 顺序 | 入口 | 具体措施 |
| --- | --- | --- |
| F1 统一媒体上下文与缓存 | `AttachmentTransferService`、图片／视频／语音／文件消息组件、媒体预览组件 | 从消息模型提取服务器消息 ID 和当前 `SessionUnit`，在聊天记录内补根消息 ID。所有图片渲染、视频封面、语音播放、文件打开和保存均接入统一的访问检查。各组件在使用既有缓存前先取得在线访问结果，再以账号、服务实例、BlobId 定位字节。同一账号的不同身份可以复用已授权字节。切号、登出和收到消息撤回或个人删除变更时，使关联访问结果失效并取消不再有效的操作。 |
| F2 幂等进入真实发送链路 | `message_repository.dart`、`MessageApi.sendUploadFile`、`uploadAttachment` | 从本地持久化发送记录获取稳定 `ClientUploadId`，贯穿首次发送、失败重试和重启恢复，避免只在底层单测传参。确认每个发送身份获得独立的上传凭证。注册表按账号、来源、原图模式、封面和影响产物的参数查找，按第一阶段约定处理过期回退和权限拒绝。 |
| F3 兼容历史与平台差异 | `HistoryMessageContent`、媒体打开和另存为 | 补齐嵌套记录内的媒体操作，使根消息与子消息参数在移动端和桌面端保持一致。历史无上下文 URL 由后端旧读路径在线验证。未取得在线授权时，各媒体组件使用统一的不可访问状态。 |

**进入放量的条件**　Flutter 静态分析与附件测试通过。移动端和桌面端各完成回归，覆盖真实发送入口、同账号切身份、跨账号拒绝、进程重启后重试、媒体缓存命中、撤回／删除、跨会话嵌套 History，以及正常播放、导出与取消。后端、Electron、Flutter 均通过对应门槛后，按下节执行灰度。

## 发布门槛与回退

1. **权限门槛**　旧入口、签名、直链、缓存、Range、复制和另存为共用相容的授权规则。验证普通转发、跨会话嵌套聊天记录、异步 GIF、私密收件范围、历史窗口、撤回、个人删除和账号切换。客户端缓存只复用已独立授权的字节。
2. **可靠性门槛**　用两个 Chat API 和两个 Worker，连接测试专用 SQL Server、Redis 与 MinIO，执行同时上传相同字节、过期回收与发送交错、对象写入或删除失败、兑换并发、GIF 外箱重启恢复和 Redis 清空。逐项核对物理文件数、逻辑凭证、消息引用和计数，禁止用单实例单元测试替代。
3. **变更门槛**　在目标拓扑的预生产数据库验证 `AddBlobObjectDedup → FilterActiveBlobObjectUniqueIndex → AddBlobReuseRecord → AddVideoGifPropagationOutbox`，核对真实迁移历史和已存对象。过滤索引已经允许历史软删除行共存，回退到无过滤唯一索引前需检查重复历史键。应用回退期间保留共享对象兼容读取和新版回收能力。
4. **发布顺序**　先部署通过第一阶段验证的迁移、兼容 API 与 Chat Worker，核对健康检查。按 Electron、Flutter 的顺序验证并发布兼容客户端，完成三端验收再开启严格策略并扩大范围。放量前灰度验证跨身份的同一文件只存一份物理内容且各自持有逻辑凭证。以下载 403/404、对象未找到、外箱积压、孤儿量及附件请求延迟作为继续放量的依据。

## 上一轮自动化验证基线

以下为 6dc3229c、17bbfd5 和 2fb85f8 的上一轮结果。当前版本的复查证据见文首。

| 仓库 | 实测 |
| --- | --- |
| IczpNet.Chat 宿主构建 | HttpApi.Host 和 BackgroundWorker 构建均成功，各 0 警告、0 错误。 |
| IczpNet.Chat Application | 附件兑换与下载控制器定向测试 6 项通过。 |
| IczpNet.Chat EF Core | 附件授权、转发和物理去重定向测试 11 项通过。 |
| Flutter | 上传兑换、缓存键、分类缓存与本地附件复用 15 项通过。 |
| Electron | 媒体缓存、附件浏览、多媒体编辑与布局四个测试文件共 27 项通过。Main 和 Renderer 类型检查通过。 |

