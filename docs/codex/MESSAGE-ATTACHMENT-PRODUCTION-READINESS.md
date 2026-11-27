# 消息附件生产评估与修复计划

评估日期为 2026-11-27。基准为 IczpNet.Chat `6dc3229c`、Flutter `17bbfd5`、Electron `2fb85f8`。部署拓扑参照 Rctea.IM 的 Chat API、Chat Worker 和 SchemaMigrator 配置。范围覆盖物理文件去重、按身份签发逻辑附件凭证、按消息授权下载，以及普通转发、聊天记录、异步 GIF 和客户端缓存。

**发布结论：暂未达到生产发布标准。** 核心方案可沿用。当前代码已有共享物理对象、独立逻辑附件和消息引用，常规上传及转发路径基本齐备。优先关闭私有媒体的下载与缓存授权缺口，再补齐长期运行和故障恢复验证。生产多实例、实际 SQL Server 与 MinIO、客户端打包联调均为放量门槛。

## 三条链路的评估

| 能力 | 当前证据 | 生产判定 |
| --- | --- | --- |
| 物理文件去重 | 服务端 SHA-256，隔离域和容器内的活跃唯一约束，版本化对象键，旧附件读取兼容。过期逻辑附件已改为数据库条件认领，物理对象计数与逻辑附件在事务路径内提交，Worker 已补扫无实际引用的 Ready 对象。 | **有条件通过代码评估。** 普通上传入口、回收失败和灰度开关仍需验证。 |
| 上传凭证按身份隔离 | 同账号兑换凭证经服务端验证后，按目标 SessionUnit 签发独立逻辑 Blob。数据库已有兑换唯一键和冲突后重读，Flutter 和 Electron 均已接入兑换。 | **主流程可用。** 历史无归属 Blob、Flutter 业务入口的幂等键以及派生参数匹配仍存在缺口。 |
| 下载按消息授权 | 服务端已按 BlobMessageRef、消息可见性和结构化历史字段校验，跨会话聊天记录改为逐跳验证。Electron 已按来源发送令牌，缓存已有消息授权入口。 | **未通过。** 旧发附件入口仍可公开存储，Electron 与 Flutter 的缓存命中仍存在绕开消息授权的路径。 |

后端上轮的两个并发问题已有实质整改。回收用 SQL 条件更新决定是否能删除过期 Temp，物理引用增量也移至逻辑写入所在工作单元。GIF 生成结果改为在业务事务内登记外箱，Chat Worker 定时处理。Electron 原有远程媒体 Bearer 发送问题已按 API 同源限制，并逐跳处理重定向。Flutter 先前缺失的同步进度文件已进入当前提交树。 [临时附件认领](../../src/IczpNet.Chat.EntityFrameworkCore/Repositories/MessageRepository.cs#L783) · [物理引用写入](../../src/IczpNet.Chat.Domain/Blobs/BlobManager.cs#L185) · [GIF 外箱写入](../../src/IczpNet.Chat.Domain/Blobs/GenerateVideoGifBackgroundJob.cs#L207)

## P1 发布前修复

### P1-01 旧聊天上传入口仍可创建绕过消息授权的附件

`MessageSenderController` 中仍有写入 `IsPublic = true` 的聊天附件路径。下游授权服务遇到公开 Blob 会直接允许下载。普通发送的归属校验仅在 Blob 有上传者字段时拒绝其他身份，历史无归属附件没有明确的用途约束。生产功能需要明确区分聊天私有附件与头像等公开资源。 [旧发送入口](../../src/IczpNet.Chat.HttpApi/Controllers/MessageSenderController.cs#L200) · [公开下载分支](../../src/IczpNet.Chat.Domain/Blobs/BlobAccessAuthorizationService.cs#L47) · [发送归属校验](../../src/IczpNet.Chat.Domain/MessageSections/Messages/MessageManager.Attachments.cs#L122)

**修复**　让旧聊天上传路由也创建带明确上传者、角色和生命周期的私有附件。对历史公开聊天附件按实际消息用途分批迁移，保留单独审核过的公开资源用途。普通发送严格校验当前上传凭证及主附件和衍生件的归属。聊天记录和普通转发沿用已授权的消息路径。

**验收**　旧客户端发送后，目标可见成员可正常下载，脱离消息上下文且没有授权的访问被拒绝。头像等公开资源继续按已有约定提供。覆盖同账号多身份、不同账号、私密消息和历史无归属 Blob。

### P1-02 Electron 私有缓存校验未验证 Blob 与消息的关系

Main 中的 `authorizeCachedBlob` 回调只确认 SQLite 里存在目标消息及其三个状态，未使用收到的 `blobId`，也未核对聊天记录根、子消息和附件之间的关系。缺少完整上下文时，`ensureRemoteCached` 可以直接命中私有缓存。同账号、同 Blob 的合并下载也共用 Promise，等待者未单独完成授权。缓存归一化已解决重复磁盘存储，授权上下文仍需独立判断。 [Main 授权回调](F:/Dev/GotoIM/gotoim-electron/src/main/index.ts:301) · [缓存与合并下载](F:/Dev/GotoIM/gotoim-electron/src/main/services/MediaCacheService.ts:1025)

**修复**　使用统一的主进程附件策略，校验账号、服务来源、身份和消息上下文、Blob 之间的完整关系。旧 URL 缺少消息上下文时交服务端按兼容规则验证。并行请求可以共享字节传输及磁盘文件，每个请求须先取得自己的访问结论。离线授权应明确时效、撤回与个人删除的处理，以及状态过期后的行为。

**验收**　相同 Blob 在不同消息、身份和嵌套聊天记录上下文中并发访问时，每个请求独立裁决。撤回、个人删除、登出和切号后，缓存命中遵循约定。

### P1-03 Electron 的本地路径、导出和复制绕过统一缓存授权

本地协议虽新增账户绑定的短期句柄，但现有 Renderer 仍构造带路径的 URL，主进程把整个媒体缓存根目录视为允许读取。该目录存储多个账号的文件。另存为与复制图片也有直接查磁盘缓存的分支。句柄记录的窗口 ID 目前未在解析时验证。 [本地 URL](F:/Dev/GotoIM/gotoim-electron/src/renderer/src/common/utils/mediaUrl.ts:28) · [本地文件判定](F:/Dev/GotoIM/gotoim-electron/src/main/services/MediaCacheService.ts:670) · [另存为入口](F:/Dev/GotoIM/gotoim-electron/src/main/windows/registerShellIpc.ts:459)

**修复**　所有私有预览、Range、另存为与复制统一经过 Main 的附件访问入口。以受信窗口和账号签发、解析本地句柄，并校验实际文件的账号归属。原始路径兼容只允许已授权的具体文件。登出和切号时撤销句柄并终止旧下载，避免暴露完整缓存目录。

**验收**　跨账号缓存文件、过期句柄、错误窗口、预览后撤回、旧本地路径均按权限处理。正常文件预览、复制和视频拖动进度继续可用。

### P1-04 Flutter 私有缓存缺少消息授权上下文

`findCachedPath` 可以在网络请求前返回内存或磁盘路径。已核对的缓存入口没有消息授权结果输入，业务附件链接也尚未串接 `su`、`msg`、`rootMsg`。图片和视频部分仍使用单独的渲染缓存。服务端按消息授权只能约束实际发出的网络请求。 [缓存直返](F:/Dev/GotoIM/gotoim-flutter/lib/core/services/file/attachment_transfer_service.dart:102) · [图片渲染](F:/Dev/GotoIM/gotoim-flutter/lib/features/chat/presentation/message_content/image_message_content.dart:63)

**修复**　给下载、预览、缓存、另存为补齐统一访问上下文和同一套在线／离线授权规则。Flutter 账号切换、登出和消息撤回应触发状态及相应缓存访问凭证失效。流媒体和缩略图同样校验。

**验收**　在桌面端和移动端分别验证账号切换、已缓存附件的撤回与个人删除、聊天记录跨会话下载、离线恢复和重新登录。

## P2 生产加固与运维

| 项目 | 当前缺口与修复建议 |
| --- | --- |
| Flutter 兑换幂等 | 底层 `uploadAttachment` 支持 `ClientUploadId`，业务入口 `sendUploadFile` 未传入该值，发送失败重试时无法稳定命中持久化兑换记录。将本地发送操作的稳定 ID 持久化并向下传递，同一次请求保留相同的源附件和参数。现有 Flutter 单测直接调用底层方法，另补业务入口测试。 [业务入口](F:/Dev/GotoIM/gotoim-flutter/lib/features/chat/data/datasources/message_api.dart:493) |
| 物理对象异常恢复 | 旧版 Temp 的物理删除失败后只有日志，已经软删除的逻辑记录不会重新出现在普通扫描中。上传接管 `Deleting` 对象会替换旧键，旧键清理也是尽力执行。将旧键与失败删除记录持久化，回收任务分批重试，并对 SQL Server 下普通上传与兑换的事务边界做故障注入。 [旧对象清理](../../src/IczpNet.Chat.Domain/Blobs/TempBlobCleanupService.cs#L58) · [替换旧键](../../src/IczpNet.Chat.Domain/Blobs/BlobManager.cs#L170) |
| 灰度和运行配置 | `EnablePhysicalDeduplication` 默认开启，显式复用已有 `StorageObjectId` 的分支先于开关判断。部署时需要明确各开关是停止普通对象去重、停止新共享引用，还是关闭兑换。签名密钥应在每个服务启动时验证。GIF 外箱由专用 Chat Worker 注册，生产健康检查应验证实际 Worker 实例及消费进度。 [存储分支](../../src/IczpNet.Chat.Domain/Blobs/BlobManager.cs#L66) · [签名配置](../../src/IczpNet.Chat.Domain/ChatDomainModule.cs#L55) · [外箱 Worker 注册](../../host/IczpNet.Chat.BackgroundWorker/ChatBackgroundWorkerModule.cs#L56) |
| 附件日志和直链 | Electron 记录完整远程媒体 URL，现有日志脱敏规则识别 `signature` 等名称，但未覆盖 Chat 的 `sig`。记录授权结果、来源和耗时即可。MinIO 预签名 URL 的撤回窗口应与业务规则一致，需要即时撤回的资源通过每次请求都执行授权的入口。 [媒体日志](F:/Dev/GotoIM/gotoim-electron/src/main/services/MediaCacheService.ts:1107) · [日志脱敏](F:/Dev/GotoIM/gotoim-electron/src/main/logging/AppLogger.ts:55) · [服务端直链](../../src/IczpNet.Chat.HttpApi/Controllers/FileController.cs#L84) |
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

## 本次验证

本次复核了上述提交和部署入口，未应用迁移或修改业务代码。后端使用定向测试，Flutter 和 Electron 使用附件相关测试。预生产环境的三端联调、多实例故障注入、迁移演练及安装包验收仍待执行。

| 仓库 | 实测 |
| --- | --- |
| IczpNet.Chat 宿主构建 | HttpApi.Host 和 BackgroundWorker 构建均成功，各 0 警告、0 错误。 |
| IczpNet.Chat Application | 附件兑换与下载控制器定向测试 6 项通过。 |
| IczpNet.Chat EF Core | 附件授权、转发和物理去重定向测试 11 项通过。 |
| Flutter | 上传兑换、缓存键、分类缓存与本地附件复用 15 项通过。 |
| Electron | 媒体缓存、附件浏览、多媒体编辑与布局四个测试文件共 27 项通过。Main 和 Renderer 类型检查通过。 |

