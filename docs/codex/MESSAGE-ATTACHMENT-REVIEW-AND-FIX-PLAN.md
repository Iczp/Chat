# IczpNet.Chat 消息附件代码审查与修复计划

审查范围覆盖 Chat 的 `da21d700`、`60884c48`、`8779fbaf` 和 Flutter 的缓存提交 `274c74b`，以[附件实施方案](MESSAGE-ATTACHMENT-DESIGN.md)为验收依据。

**审查结论**。暂缓扩大共享附件写入和上传凭据复用的发布范围。** 历史转发绑定、共享内容归属保护、GIF 传播和服务端去重都已有实现。发布前优先修复两处可造成共享对象写入失败或字节丢失的生命周期问题，同时关闭签名配置与消息授权缺口。现有定向测试覆盖主要成功路径，仍需补齐并发、回收重试和私有附件兼容回归。

## P1 发布前修复

### 1. 物理对象软删除后同一文件无法重新上传

`BlobObject` 继承启用了软删除的 `BaseEntity<Guid>`。回收删除了实际文件，再通过 `DeleteAsync` 隐藏数据库行。迁移中的物理对象唯一索引覆盖软删除行。后续上传相同内容时，常规查询看不到旧行，新行还会占用同一个唯一键，导致上传失败。当前回收测试验证了最后一个引用到期后物理文件消失，未覆盖同内容重新上传。 [回收实现](../../src/IczpNet.Chat.Domain/Blobs/TempBlobCleanupService.cs#L135) · [实体继承](../../src/IczpNet.Chat.Domain/Blobs/BlobObject.cs#L10) · [唯一索引与外键](../../host/IczpNet.Chat.HttpApi.Host/Migrations/20261126124433_AddBlobObjectDedup.cs#L60)

修复时统一决定回收后如何释放活跃对象唯一键。可为活跃行建立过滤唯一索引并保留旧行审计，或者在妥善迁移、核对包括软删除行在内的外键引用后物理删除。保留不同对象版本的独立存储键。先检查已回收数据的兼容性，再准备正式迁移。验证最后一个引用清理后重新上传同一内容、删除任务重跑以及旧任务与新上传交错。

### 2. 回收与发送之间缺少持久化状态交接

Worker 先查出一批过期 Temp Blob，再按实体删除。旧路径甚至在删逻辑记录之前直接删对象。发送准备阶段没有拒绝已过期的 Temp，绑定阶段也没有要求附件仍处于可绑定状态。物理回收在外层 UOW 内保存 `Deleting` 后立即删对象，`autoSave` 的 SQL 刷新与事务提交是两个不同的时间点。共享对象的创建和复用锁也在调用方提交前释放。上述边界使发送与回收交错时可能出现状态倒退、请求失败，或对象已删而数据库事务回滚。写字节成功后 SQL 事务回滚时，也可能留下缺少持久记录的物理对象。 [发送准备](../../src/IczpNet.Chat.Domain/MessageSections/Messages/MessageManager.Attachments.cs#L111) · [引用绑定](../../src/IczpNet.Chat.EntityFrameworkCore/Repositories/MessageRepository.cs#L247) · [临时附件回收](../../src/IczpNet.Chat.Domain/Blobs/TempBlobCleanupService.cs#L29) · [物理对象写入](../../src/IczpNet.Chat.Domain/Blobs/BlobManager.cs#L64)

让数据库决定发送与回收谁先取得 Temp。发送只接受符合业务规则的附件状态，绑定和计数随消息事务提交。回收以条件更新确认 Blob 依然过期且无引用，提交后才按对应的不可变对象键执行 I/O。物理写入先持久化可恢复的 `Pending` 记录，上传成功后再推进 `Ready` 并绑定逻辑记录。将 `Deleting` 与版本化删除任务持久化，失败可重试。使定期修复也能找到未再次经过 Temp 批次的 `Ready` 孤儿，并处理锁超时、计数漂移和删除失败。

### 3. 私有附件签名与历史兼容授权需要精确校验

带上下文的历史回退仍使用 `ContentJson.Contains`，无法证明 Blob 出现在允许下载的附件字段。带 `rootMsg` 时，仓储只验证子消息存在、非私密、未撤回，未建立子消息属于该聊天记录或同会话引用的关系。签发和下载虽已共用服务，这两处规则仍影响权限边界。 [历史回退](../../src/IczpNet.Chat.Domain/Blobs/BlobAccessAuthorizationService.cs#L101) · [子消息校验](../../src/IczpNet.Chat.EntityFrameworkCore/Repositories/MessageRepository.cs#L88)

下载签名和上传复用凭据还共用了具有仓库内置默认值的 `SigningKey`，未见 Chat 模块在启动时强制校验密钥配置。使用默认值、空值或未正确注入密钥的部署无法依赖签名作为可信身份或账号凭据。实际运行环境的密钥配置需要由部署侧确认。 [密钥选项](../../src/IczpNet.Chat.Domain/Options/AttachmentSignatureOptions.cs#L8) · [复用凭据签名](../../src/IczpNet.Chat.Domain/Blobs/AttachmentReuseTokenService.cs#L132)

对历史数据按结构化附件字段或已验证的引用精确授权。使用 `rootMsg` 时，在有界遍历内验证根、子消息和该子消息附件的关系，并按业务规则处理撤回、私密和不可见引用。对生产签名配置拒绝已知默认值、空值和不符合密钥标准的值，安排 API 实例一致的密钥轮换并用测试覆盖旧签名过渡。继续收口尚会创建公开聊天附件的旧 `send-upload-*`，和历史公开资源兼容分开部署。 [旧上传入口](../../src/IczpNet.Chat.HttpApi/Controllers/MessageSenderController.cs#L200)

## P2 后续修复与兼容验收

### 4. 旧客户端无消息参数时可能错误拒绝合法下载

当前先按全局附件引用截取最近 50 条，再找当前账号身份。早期结构化路径也先对各类附件取 20 条，再判断账号可见性。文件高频转发后，即使访问者有合法历史消息，也可能因候选被截断遭到拒绝。命中候选后逐条调用仓储还会增加 SQL 往返。将 `BlobMessageRef`、当前账号在会话中的有效 `SessionUnit` 和消息可见性合并成数据库侧存在性查询，筛选权限后再终止搜索。结构化历史路径应用同样的关联条件。验收覆盖最近 50 条均来自其他会话、较早的引用仍合法的场景。 [旧链接候选查询](../../src/IczpNet.Chat.Domain/Blobs/BlobAccessAuthorizationService.cs#L129)

### 5. GIF 在聊天记录中的传播只完成了引用写入

普通转发分支会更新快照和 `MessageChange`，聊天记录分支只绑定根消息的 Blob 引用。已同步过聊天记录的客户端缺少根消息变更信号。传播反查只沿 `HistoryMessage` 的直接包含关系，也没有覆盖收集阶段支持的同会话引用。生成后通过 UOW 完成回调入队，生成结果持久化和传播任务提交之间还存在进程退出窗口。 [普通消息和聊天记录传播](../../src/IczpNet.Chat.Domain/MessageSections/Messages/MessageManager.Attachments.cs#L452) · [提交后入队](../../src/IczpNet.Chat.Domain/Blobs/GenerateVideoGifBackgroundJob.cs#L201)

使传播任务与已生成 GIF 在数据库里有可恢复的关联，沿用现有持久化事件或补偿机制。按与附件收集一致的关系找到根消息，限量查询并在新增可展示内容时触发既有同步和缓存失效。补测已加载的聊天记录、仅通过引用进入记录的 GIF、并发转发、进程在 GIF 提交后退出、下游任务重跑。

### 6. 引用写入的幂等仍依赖调用者和事务重试

`BindBlobsToMessageAsync` 先查已有引用，再增计数并插入。并发调用可能同时判断引用缺失，由唯一索引拦截第二次插入并抛错。顺序重跑测试可通过，且事务正确回滚时计数保持一致。数据库应在事务内以实际新增的引用作为唯一计数输入，并让重复写入正常返回。按实际 SQL Server 隔离级别验证两个实例对同一消息的绑定。 [引用绑定代码](../../src/IczpNet.Chat.EntityFrameworkCore/Repositories/MessageRepository.cs#L244)

### 7. 多身份上传复用的重试还未实现持久化幂等

`ClientUploadId` 当前只用于 Redis 查询和随后缓存结果。两个并行请求可同时未命中，缓存被清理后同一请求也会再次创建一组 Temp Blob。可覆盖的文件名未参与请求一致性核对。缓存先于请求 UOW 最终提交写入，还可能留下无法引用的 BlobId。目标校验只检查 `IsEnabled` 和 `IsInputEnabled`，实际发送额外检查正在禁言。 [复用入口](../../src/IczpNet.Chat.HttpApi/Controllers/AttachmentController.cs#L788) · [缓存写入](../../src/IczpNet.Chat.HttpApi/Controllers/AttachmentController.cs#L956) · [发送禁言校验](../../src/IczpNet.Chat.Domain/MessageSections/Messages/MessageManager.cs#L435)

把兑换规则移入 Chat Application/Domain 的统一流程，复用发送权限。以账号、目标 SessionUnit、源附件及 ClientUploadId 建数据库幂等约束，并记录请求参数摘要和结果。主附件、子附件和幂等结果同事务提交，Redis 只加速读取。返回缓存前核对有效期和当前权限。明确复用旧客户端上传结果与网络重试的边界，补测同一凭据并发兑换、参数变化、禁言和缓存清空。

### 8. 多身份缓存覆盖了部分下载入口，仍需验证账号切换和媒体展示

Flutter 已提交按来源和 BlobId 归一化的磁盘缓存。同一 `AttachmentTransferService` 的 `_states` 和 `_bytes` 仍仅按本地 `id` 索引，已有路径可在账号目录查询之前返回。现有测试给两个账号不同的本地 ID，无法覆盖该短路分支。图片和视频消息的 `CachedNetworkImage` 渲染入口没有显式接入新缓存键。当前 Flutter 源码中也未找到 `reuseToken` 或 `attachments/reuse` 的兑换调用。 [Flutter 传输状态](F:/Dev/GotoIM/gotoim-flutter/lib/core/services/file/attachment_transfer_service.dart:77) · [聊天图片入口](F:/Dev/GotoIM/gotoim-flutter/lib/features/chat/presentation/message_content/image_message_content.dart:63)

磁盘和内存使用同一套账号及服务实例范围，账号切换时刷新传输状态和私有媒体缓存入口。对缓存命中执行当前消息可见性或已定义的离线授权策略。给图片、视频、文件分别加端到端验收，再接入服务端兑换接口。更新旧分类缓存测试中预期跨账号回查成功的断言，并新增同一本地 ID 跨账号、同账号多身份和切换服务实例用例。 [现有缓存测试](F:/Dev/GotoIM/gotoim-flutter/test/attachment_cache_key_test.dart:87) · [失败断言](F:/Dev/GotoIM/gotoim-flutter/test/core/attachment_cache_classified_test.dart:175)

## 修复顺序与发布验收

1. **先处理鉴权和物理对象状态机。** 完成签名配置校验、精确历史授权、同内容重新上传、Temp 绑定与回收互斥。共享对象已写入的部署应保留兼容读路径。发布配置应允许独立控制新共享写入，回退时协调 API 与 Worker。
2. **再补转发同步与历史兼容。** 正常根消息、历史合法旧链接、部分引用回填、撤回、个人删除、GIF 生成与多级转发均通过验收。所有数据库写入和任务恢复路径使用有界批次。
3. **最后扩展多身份兑换和客户端缓存。** 先固化服务端幂等和账号边界，再完成 Flutter 接入及缓存覆盖。使用两实例、SQL Server 和实际对象存储验证提交与回收失败时序，再扩大部署范围。已有 SQLite 和内存对象存储测试可继续覆盖主流程。

## 本次验证

| 检查 | 结果 |
| --- | --- |
| Chat HttpApi.Host、Chat BackgroundWorker 构建 | 均成功，0 警告、0 错误 |
| Rctea.IM HttpApi.Host、Rctea.IM BackgroundWorker 构建 | 均成功，0 警告、0 错误 |
| Chat Application 附件复用与下载鉴权定向测试 | 4 通过 |
| Chat EF Core 附件转发、去重、可见性和变更定向测试 | 15 通过 |
| Chat Domain 签名、上传校验、可见性和发送幂等定向测试 | 14 通过 |
| Flutter 附件缓存键与分类目录测试 | 7 通过、1 失败。失败用例单独复跑一致，旧断言仍期望账号 2 找到账号 1 的文件，实际隔离结果为 `null`。 |

本次只做代码审查、构建和定向测试。多实例 SQL Server 与实际对象存储的并发验收，仍需在修复后执行。
