# Chat 后端安全与同步问题修复记录

日期：2026-10-06。范围：IczpNet.Chat 后端、Flutter 会话同步；Rctea.IM 仅调整 Worker 的模块组装。

本记录对应本次代码审查发现的九类问题。以下“已修复”指源码修改和对应验证，不代表代码已经部署，也不代表旧数据已经清理。

## 1. 问题清单与改进

| 编号 | 优先级 | 原问题与触发条件 | 本次改进 |
| --- | --- | --- | --- |
| 1 | P1 | 会话归属断言被注释；发送、History、Latest 只根据传入 SessionUnitId 加载数据，登录者可访问不属于自己的会话。批量校验仅判断任意一个所属对象也会留下混入他人 ID 的绕过路径。 | ChatAppService 和 CrudChatAppService 强制检查全部传入 OwnerId 均属于当前用户；未登录或任一对象不属于当前用户均拒绝。发送、转发、历史和增量入口补齐归属校验。可选回调不再直接 await 空任务。群成员详情校验 Visitor 的归属。 |
| 2 | P1 | 撤回只检查时间限制，没有检查发送者；知道 MessageId 即可能撤回他人消息。 | MessageSenderAppService 根据消息 SenderId 校验当前用户归属，再进入既有撤回时间规则。没有为普通撤回入口增加管理员绕过。 |
| 3 | P1 | 共享 Session 消息 ZSET 仅按 DeletedRecorder 过滤，没有私密消息参与者、入群历史和清空时间边界。详情、引用和聊天记录分享也存在绕过同一规则的路径。 | 扩展已有 OwnerMessageSpecification：限定会话单元，检查私密参与者、Killed、Start/EndMessageId、HistoryFristTime/HistoryLastTime、ClearTime 和 DeletedRecorder。Repository 统一查询可见 ID、数量和访问权限；Redis 继续缓存消息内容。普通列表、详情、引用、转发及聊天记录分享同步使用该规则。 |
| 4 | P1 | 公开 Hub 方法允许客户端指定 Command，并向全体连接广播含用户、设备和 IP 信息的连接池。另有 HTTP SignalR 调试控制器可直接发测试命令。 | 保留 Hub 方法签名，但调用时明确抛出 HubException，禁止客户端广播服务端命令；SignalR 调试控制器标为 NonController，不再发布 HTTP 路由。业务消息仍走原有发送、权限及事件流程。 |
| 5 | P1 | Processing 记录在任务入队前已删除；进程退出、数据库提交失败或入队失败会丢失持久化标记。秒级 Processing Key 可能冲突，全部任务一次加载到内存也没有总量限制。 | GUID Processing Key；原子 RENAME + 恢复索引；最多读取 5,000 条，不提前删除。任务独立数据库事务提交成功后，以原始版本执行 Lua ACK；失败保留记录。重复任务先检查自己是否仍有待确认记录。持久化批次使用分布式锁，避免旧快照后提交覆盖新状态。Worker 周期触发分批调度及恢复。 |
| 6 | P2 | Flutter 网络重试复用 ClientMessageId，后端却无条件 INSERT；提交成功但响应丢失后，重试可能重复消息和副作用。 | 按 SenderSessionUnitId + ClientMessageId 获取现有 ABP 分布式锁；锁内查询已有消息，命中则返回原消息。新消息用独立事务提交，提交完成才释放锁。重复请求不再创建内容、插入消息或再次更新计数。 |
| 7 | P2 | 会话变更先分页再过滤 Ticks；降序结果配合 Flutter 最大 Ticks 游标，超过一页会丢记录；相同 Ticks 也没有第二游标。 | 对已有好友索引建立临时原子快照，按批读取，先筛选再分页；按 Ticks 升序及规范 GUID 文本升序返回。增加可选 CursorId 和 extra 游标信息。Flutter 使用复合游标，首次重放边界毫秒，完整读取 HasMore 页。 |
| 8 | P2 | 历史循环把最小 ID 减一，Redis 又排除上边界；上一批最小 ID 为 104 时，会遗漏 103。返回 NextCursorId 也指向被移除的多取记录。 | Repository 统一使用 Id < MaxMessageId，直接传递上一页最后一条已返回消息 ID；不做减一。API 去掉多取记录后，再计算 NextCursorId。 |
| 9 | P2 | Latest 假设 MinMessageId 必须仍在缓存；锚点缺失时可提前返回空结果，过滤不足时又向反方向移动游标。 | Repository 使用 Id > MinMessageId 升序受限查询，与锚点是否存在、全局 ID 是否连续以及缓存是否完整无关。API 的多取一条只用于 HasMore。 |

## 2. 模块与分层

- Application：当前用户归属检查、发送/撤回/转发入口、列表及详情授权、分页 DTO 的组装。
- Domain：沿用 OwnerMessageSpecification；发送幂等编排、事务与锁；Dirty 快照、任务版本参数、恢复索引和确认；周期调度 Worker。
- EntityFrameworkCore：可见 ID/数量/引用访问查询。过滤、排序和 Take 在 SQL 中执行，不把整个消息表加载到内存。
- Domain.Shared / Application.Contracts：现有 SessionUnitOptions 的调度配置、复合游标的可选参数。
- HttpApi：关闭调试广播入口。
- Host：Chat API Host / Worker 注册 Redis 分布式锁提供器。独立 API Host 补引用 Volo.Abp.DistributedLocking 10.6.1，与已有 Worker 版本一致；没有升级 ABP、EF Core 或 Redis 客户端。
- Rctea.IM：只在 BackgroundWorker 的初始化中注册 Chat 的周期 Worker，不把业务实现移入主项目，不增加 CMS 或 AppStatistic 业务依赖。

批量资源校验与“双方参与者”查询有所区别：共同会话查询只要求 SourceId 属于当前用户，TargetId 可为好友；申请列表的 OwnerId/DestinationId 是替代式方向筛选，绑定本人时保留自助查询，没有本人筛选时仍要求显式 GetAll 权限。不能把对方必须属于自己的条件误加到这些合法业务中。

## 3. 消息可见性与分享

统一规则以数据库中的会话关系、设置和删除记录为准。私密消息仅允许发送或接收该消息的 SessionUnit 读取；第三个群成员即使拥有自己的会话单元，也不允许读取。

历史起始时间包含边界，历史截止时间和清空时间排除边界。StartMessageId 为排他下界，EndMessageId 为包含式上界。消息游标仍使用全局 Message.Id，不要求同一 Session 内连续。

对于跨会话的详情，保留合法引用和聊天记录分享能力：请求会话中必须存在当前用户可见的引用/聊天记录消息，并且来源消息不能是私密或已撤回消息。不能仅凭“参加过某个源会话”就读取任意历史文件。

新建聊天记录分享最多选择 100 条，全部来源必须对该 Owner 可见，且不能包含私密或已撤回消息；不再静默略过非法 ID。补齐 HistoryMessage 关联。私密消息引用必须仍在原来的两个参与者之间，不能把私密消息预览带进公开群消息。

本次不会批量重写旧消息、旧聊天记录描述或内容快照。部署前后应另行审计历史非法分享/引用；源码鉴权修复不能撤销已经被外部客户端保存的内容。

## 4. Dirty 持久化与恢复

```text
消息/状态更新
  → 原有 Dirty ZSET
  → 原子切换到 GUID Processing ZSET，并登记 ProcessingKeys
  → 有限批次任务入队
  → Worker 获取持久化锁
  → 读取当前缓存状态并批量更新数据库
  → 独立数据库事务提交成功
  → Lua 按原始 Processing 版本 ACK
  → 清理已空 Processing Key 和恢复索引
```

约束：

1. 调度锁负责跨实例调度；持久化锁只串行化落库批次，不串行化聊天发送。
2. 单次扫描最多 5,000 条，默认每任务 1,000 条。不会先在内存组装整个 Dirty 集合的所有任务。
3. Processing Key 没有过期时间。新更新写入原 Dirty Key，旧任务 ACK 不会删除新一轮标记。
4. 未取得锁的调度返回空批次；未取得持久化锁的任务抛出异常，交给已有后台任务机制重试。
5. 入队前或事务提交前失败，Processing 记录仍保留；未收到确认的快照一分钟后允许重新调度，实际时间取决于 Worker 运行和队列状态。成功确认后允许继续派发剩余批次。
6. 任务可能重复入队，使用版本确认和待处理检查实现至少一次处理。Progress 属于诊断信息，失败重试不能靠 Progress 计数决定是否删除数据。
7. 缺失脏缓存时任务失败并保留标记，不把缺失当作成功。原有缓存过期/Redis 数据丢失本身不能被恢复索引自动重建；应监控堆积并保证 Redis 持久化及可用性。

配置（两个 Worker Host 都加载同一个 Chat 实现）：

```json
{
  "Chat": {
    "SessionUnits": {
      "AutoFlushDirtyEnabled": true,
      "FlushDirtyPeriodSeconds": 10
    }
  }
}
```

原有 CacheExpire、BatchLoadSize 保持原默认值。API Host 不注册该周期 Worker。

旧版未登记的 `Dirty:Processing:*` 残留 Key 不会自动进入新恢复索引。切换前应只读清点，确认没有旧实例仍在操作，再将确实待处理的 Key 登记到同前缀 `Dirty:ProcessingKeys` ZSET，score 使用 0。旧版已经删除且尚未入队的标记无法凭新代码恢复，需另行进行 Redis/数据库状态核对；本次没有执行真实数据补偿。

## 5. 幂等与部署条件

锁按发送会话单元和 ClientMessageId 隔离，不是全局发送锁；ClientMessageId 在锁名中使用 SHA-256 摘要，避免超长或敏感标识直接进入锁 Key。

使用已有 IAbpDistributedLock，并在两个独立 Chat Host 注册 RedisDistributedSynchronizationProvider。检测到 NullAbpDistributedLock 时拒绝关键操作，不把空锁当作跨实例保护。组合 Host 必须同样加载真正的分布式锁模块，使用相同 Redis、数据库编号和锁前缀。

没有新增数据库唯一索引，因此不需要清理重复记录或生成迁移。幂等保护要求所有写入实例均部署新逻辑并共享锁提供器；旧版实例或绕过 MessageManager 的直接数据库写入不受该锁保护。建议协调切换全部发送实例；如以后增加数据库唯一约束，应先检查历史重复数据，并显式生成及审核迁移。

## 6. API 与 Flutter 兼容性

保留现有路由和响应 items/totalCount。会话变更新增可选请求 CursorId，以及响应 extra.HasMore、extra.NextCursorTicks、extra.NextCursorId。

会话变更返回顺序改为 Ticks 升序。旧客户端可继续发送 MinTicks，但仅使用时间游标时不能完整处理跨页同 Ticks；应同步发布 Flutter 更新。Flutter 优先使用服务端游标，保留没有 extra 的旧响应解析能力。

历史/增量消息的 NextCursorId 现在指向最后一条真正返回的消息。越权请求会被拒绝；原来依赖这些漏洞的调用会失败。调试 SignalR HTTP 路由关闭，Hub 调试方法调用明确失败。

## 7. 性能影响与验证边界

- 消息列表先由 SQL 裁定可见 ID，再批量读取消息内容缓存，增加数据库查询，但修复缓存缺失及权限边界不一致。ID 查询只投影主键，使用游标、排序和受限 Take。
- HasMore 时的可见数量统计仍可能扫描较多匹配记录；本次没有执行生产 SQL Server 大表执行计划或压力测试，不能宣称千万级消息性能已验证。
- 现有好友 ZSET 的 score 包含置顶 Sorting，不能直接按 Ticks 做范围查询。本次选择保留该索引格式，按 Owner 建立临时原子快照并每次读取 500 条，内存仅保留所需的最小候选集合。快照在 finally 删除，异常退出时最多保留两分钟。
- 会话变更扫描时间及 Redis 快照成本仍与该 Owner 的会话数量相关；大量会话用户应实测并发延迟，必要时另行设计独立 Ticks 索引及兼容回填。本次没有引入长期第二套好友索引。
- Dirty 单次内存和任务参数量受控，但持久化批次串行化可能限制落库吞吐，需要监控 Dirty/Processing 深度、最老等待时间、提交耗时和缓存缺失。
- 自动落库 Worker 是本次新增行为，默认十秒调度。部署前可先设 AutoFlushDirtyEnabled=false，确认锁、数据库连接、后台任务执行和旧标记清点后启用。

## 8. 验证记录

验证使用隔离 Redis 3.2 容器和 SQLite 内存数据库；没有调用真实跨账号 API，没有修改真实数据库、运行迁移或部署服务。

已验证的用例：

- 全部所属对象校验；混入他人 OwnerId 被拒绝；无回调正常完成。
- 他人 SessionUnit 不能发送消息，他人消息不能被撤回。
- 私密参与者、DeletedRecorder、Killed、清空时间和全部历史边界。
- SQL Server 查询生成：EXISTS 过滤、主键游标、TOP 限制。
- SQLite Repository 实际分页：相邻 ID、稀疏 ID、缺失锚点、私密消息排除和可见数量。
- 幂等命中返回原消息，不执行内容创建与 INSERT，事务提交早于释放锁。
- 未入队 Processing 重启恢复、重复 ACK、新 Dirty 版本保留、有限批次派发。
- 数据库提交失败时不 ACK。
- Redis 中置顶/非置顶混合、150 条同 Ticks 分页；Flutter 同步全部 150 条并重放边界。
- Redis 中一万个会话条目，筛选后返回 31 条并按游标顺序排列，查询结束后临时快照清理。

最终验证结果：

| 验证项 | 结果 |
| --- | --- |
| Chat HttpApi、EntityFrameworkCore 项目构建 | 成功 |
| Chat 独立 HttpApi.Host 构建 | 成功；2 条原有 Ulid 转换器 CS8632 警告，无错误 |
| Chat 独立 BackgroundWorker 构建 | 成功；无警告、无错误 |
| Rctea.IM.BackgroundWorker 构建 | 成功；无警告、无错误 |
| Domain 定向回归测试 | 8 个通过：7 个新增用例及已有 FriendScore 用例；无跳过 |
| Application 权限回归测试 | 2 个新增用例通过 |
| EntityFrameworkCore 可见性和分页测试 | 2 个新增用例通过 |
| Flutter 会话和消息仓库测试 | 26 个通过，包含新增同 Ticks 跨页用例 |
| Flutter 本次修改文件的定向 analyze | 无问题 |
| 本次改动的 diff 空白检查 | 通过；按 Windows CRLF 规则检查 |

后端共新增 11 个回归用例。上述结果仅覆盖列出的定向范围，未运行全部 Solution 测试。

可在对应仓库根目录复现：

```powershell
# IczpNet.Chat
dotnet build host/IczpNet.Chat.HttpApi.Host/IczpNet.Chat.HttpApi.Host.csproj
dotnet build host/IczpNet.Chat.BackgroundWorker/IczpNet.Chat.BackgroundWorker.csproj
# 设置为专用测试 Redis 地址，禁止指向业务 Redis。
$env:CHAT_REVIEW_REDIS = '127.0.0.1:<测试容器端口>'
dotnet test test/IczpNet.Chat.Domain.Tests/IczpNet.Chat.Domain.Tests.csproj --filter 'FullyQualifiedName~MessageVisibility|FullyQualifiedName~DirtyRecovery|FullyQualifiedName~DirtyCommit|FullyQualifiedName~MessageIdempotency|FullyQualifiedName~FriendScore'
dotnet test test/IczpNet.Chat.Application.Tests/IczpNet.Chat.Application.Tests.csproj --filter 'FullyQualifiedName~MessageSecurity'
dotnet test test/IczpNet.Chat.EntityFrameworkCore.Tests/IczpNet.Chat.EntityFrameworkCore.Tests.csproj --filter 'FullyQualifiedName~MessageVisibilityQuery'
# Rctea.IM
dotnet build src/Rctea.IM.BackgroundWorker/Rctea.IM.BackgroundWorker.csproj
# gotoim-flutter
flutter test test/session_repository_test.dart test/message_repository_test.dart
flutter analyze lib/features/session/data/datasources/session_unit_api.dart lib/features/session/data/models/paged_result_dto.dart lib/features/session/data/repositories/session_repository.dart test/session_repository_test.dart
```

Redis 集成测试仅在显式提供 CHAT_REVIEW_REDIS 时运行，使用随机测试 Key 前缀。本次使用绑定本机随机端口的临时 Redis 3.2 容器。一万个会话条目的单次筛选查询耗时 85ms；这是单 Owner、隔离环境的功能与规模验证，不代表生产并发延迟、SQL Server 消息大表性能或长期吞吐指标。

## 9. 主要实现文件

后端根目录：`F:\Dev\abpvnext\IczpNet.Chat`。

- Application/BaseAppServices：ChatAppService、CrudChatAppService。
- Application/MessageServices：MessageAppService、MessageSenderAppService。
- Application/SessionUnits：SessionUnitCacheAppService。
- Domain/Specifications：OwnerMessageSpecification。
- Domain/MessageSections：MessageSender、Messages/MessageManager、IMessageRepository。
- EntityFrameworkCore/Repositories：MessageRepository。
- Domain/SessionUnits：SessionUnitCacheManager、FlushDirtyProcessingJob、FlushDirtyProcessingJobArgs、SessionUnitFlushWorker。
- HttpApi：ChatHub、SignalRController。
- 独立 Host：ChatHttpApiHostModule、ChatBackgroundWorkerModule。

Flutter：`F:\Dev\GotoIM\gotoim-flutter\lib\features\session\data` 下的 session_unit_api.dart、paged_result_dto.dart、session_repository.dart；对应回归测试位于 test/session_repository_test.dart。

主项目组合文件：`F:\Dev\abpvnext\Rctea.IM\src\Rctea.IM.BackgroundWorker\IMBackgroundWorkerModule.cs`。
