# IczpNet.Chat Codex 开发规范

## 1. 模块定位

`IczpNet.Chat` 是整个 IM 系统的核心聊天业务模块。

主应用 `Rctea.IM` 主要负责集中引用和组合各个 ABP 模块。

聊天相关核心业务实现优先位于：

```text
F:\Dev\abpvnext\IczpNet.Chat
```

因此：

- 聊天业务逻辑优先修改 `IczpNet.Chat`；
- 不要为了实现聊天功能，把业务逻辑写入 `Rctea.IM.HttpApi.Host`；
- `Rctea.IM` 应主要负责模块组合、Host 配置、认证配置和运行环境配置；
- 如果功能本质属于 Chat 模块，应优先在 `IczpNet.Chat` 内完成。

修改聊天相关功能之前，必须先确定当前功能属于：

- Domain
- Application.Contracts
- Application
- EntityFrameworkCore
- HttpApi
- Redis / Cache
- BackgroundJob
- SignalR

中的哪一层。

------

# 2. 核心原则

这是一个实际运行的大型 IM 系统，不是 Demo 项目。

所有修改默认必须考虑：

- 大数据量；
- 高并发；
- 多实例部署；
- Redis 网络 RTT；
- SQL Server 查询成本；
- Job 重试；
- 数据一致性；
- 客户端重复请求；
- 消息乱序；
- 消息重复；
- 缓存失效；
- Redis 超时；
- 数据库回源。

禁止只根据“小数据测试可以运行”判断设计正确。

------

# 3. 修改前必须先探索

对于非简单修改，不要立即编辑代码。

先完成以下分析：

1. 找到功能入口；
2. 找到 ApplicationService；
3. 找到对应 Manager / DomainService；
4. 找到 Repository；
5. 找到 Redis / CacheManager；
6. 找到 BackgroundJob；
7. 找到相关 Entity；
8. 找到 EF Core Mapping；
9. 找到相关索引；
10. 找到调用方；
11. 找到已有测试；
12. 确认是否存在多实例并发问题。

修改前至少明确：

```text
所属子模块：
涉及项目：
涉及实体：
涉及 Manager：
涉及 Repository：
涉及 Redis Key：
涉及 BackgroundJob：
涉及 SignalR：
涉及数据库：
涉及公开 API：
并发影响：
Migration 影响：
兼容性影响：
```

然后再实施。

------

# 4. 优先使用现有实现

创建任何新类之前，必须先搜索现有代码。

尤其是：

- Manager
- Repository
- CacheManager
- RedisStore
- BackgroundJob
- DTO
- Helper
- Extension
- Options
- DistributedLock
- Batch 操作
- 分页实现
- Redis Key Factory

不要创建第二套完成相同功能的实现。

如果项目已经存在某种模式，应优先延续该模式。

------

# 5. ABP 分层

IczpNet.Chat 应遵循标准 ABP 模块分层。

典型结构：

```text
IczpNet.Chat.Domain.Shared
        ↓
IczpNet.Chat.Domain
        ↓
IczpNet.Chat.Application.Contracts
        ↓
IczpNet.Chat.Application
        ↓
IczpNet.Chat.HttpApi
```

持久化层：

```text
IczpNet.Chat.EntityFrameworkCore
```

禁止跨层随意引用。

------

# 6. Domain.Shared

主要放：

- Enum
- Constants
- ErrorCodes
- Localization
- 公共领域定义

禁止：

- 数据库访问；
- Redis 操作；
- HttpContext；
- DbContext；
- ApplicationService；
- 具体基础设施实现。

------

# 7. Domain

核心业务规则优先位于 Domain。

典型对象包括：

```text
Session
SessionUnit
Message
DeletedRecorder
MessageStat
MessageReminder
Follow
```

典型 Manager 包括：

```text
SessionManager
SessionUnitManager
MessageManager
```

如果一个逻辑属于：

> 无论由 HTTP、BackgroundJob、SignalR 或其他入口调用，都必须遵守的业务规则

那么它通常应该位于 Domain / Manager 中，而不是只写在 ApplicationService。

------

# 8. Application

Application 层负责业务用例编排。

典型流程：

```text
ApplicationService
        ↓
Manager
        ↓
Repository / CacheManager
```

不要把几百行业务规则全部堆在 ApplicationService。

ApplicationService 应主要处理：

- 权限；
- 输入验证；
- DTO；
- 调用 Manager；
- 调用 Repository；
- 调用 Cache；
- UnitOfWork；
- 返回结果。

------

# 9. Repository

复杂数据库查询优先封装到 Repository。

尤其涉及：

- Message
- Session
- SessionUnit
- DeletedRecorder

的大数据查询。

不要在多个 ApplicationService 中复制类似 LINQ。

如果查询明显属于数据访问能力，应考虑加入现有 Repository。

------

# 10. Message 表

`Message` 是系统最重要的大表之一。

所有 Message 查询默认按照“大量历史数据”设计。

避免：

```text
全表扫描
超大 Offset
无界 ToListAsync
不必要 Include
N+1 查询
客户端过滤大量数据库结果
```

优先：

```text
Id 游标
范围查询
Projection
Take
Batch
合适索引
```

------

# 11. Message ID

Message 主键 `Id` 是 long 自增值。

不要假设：

```text
同一个 Session 中相邻消息 Id 一定 +1
```

因为 Id 是全局消息表自增。

例如同一个 Session 的消息可能为：

```text
100
105
113
140
```

因此判断会话消息连续性时，不允许简单使用：

```text
nextId == currentId + 1
```

------

# 12. Session 内消息顺序

如果代码使用：

```text
SessionMessageId
SortId
LastSessionMessageId
```

等会话级排序字段，必须确认其初始化、Redis 自增和数据库持久化机制。

涉及会话级自增时应优先使用 Redis 原子命令，例如：

```text
HINCRBY
INCR
```

禁止：

```text
读取当前值
+1
写回
```

这种非原子实现。

------

# 13. 消息分页

消息历史分页优先采用游标方式。

例如：

```text
Id < MaxMessageId
```

获取更早消息。

获取最新消息：

```text
Id > MinMessageId
```

不要改成：

```text
Skip(100000)
Take(50)
```

这种大 Offset 查询。

------

# 14. Redis 会话消息缓存

会话消息 Redis 缓存通常使用 Sorted Set。

设计原则：

```text
member = MessageId
score  = MessageId
```

或遵循项目现有实现。

必须保持：

- 最新消息可以直接追加；
- 历史消息主要向前补；
- Redis 缓存范围可判断；
- 数据库作为最终数据源。

不要随意改变 Redis ZSET member / score 语义。

------

# 15. 历史消息缓存补齐

历史补齐通常只向更早方向加载。

例如：

```text
Redis 当前：
1100 ... 2000

请求：
minMessageId = 1000
```

可以从数据库补：

```text
1000 ... 1100
```

但必须：

- 分批查询；
- 分批写 Redis；
- 控制每批数据量；
- 避免一次加载几十万条；
- 即使客户端请求中途失败，也不能写入错误缓存。

如果数据量巨大，应优先考虑直接从数据库返回，而不是强制一次补齐全部 Redis。

------

# 16. DeletedRecorder

查询用户可见消息时必须考虑删除记录。

不能仅仅：

```csharp
SessionId == sessionId
```

就认为全部消息可见。

需要检查当前 SessionUnit 对应的：

```text
DeletedRecorder
```

或已有删除消息缓存。

修改消息查询前，先搜索：

```text
DeletedRecorderManager
GetDeletedMessageIdListAsync
```

以及相关调用。

------

# 17. SessionUnit

`SessionUnit` 是 Chat 模块最核心、最高频的实体之一。

修改 SessionUnit 时必须重点检查：

- OwnerId
- SessionId
- DestinationId
- LastMessageId
- PublicBadge
- PrivateBadge
- FollowingCount
- RemindAllCount
- RemindMeCount
- Sorting
- Ticks
- ReadMessageId
- PeerReadMessageId
- LastSendMessageId
- LastSendTime
- IsImmersed
- MuteExpireTime

不要只修改数据库实体而忽略 Redis 对应字段。

------

# 18. SessionUnit Redis

SessionUnit Redis 数据可能包括：

```text
PublicBadge
PrivateBadge
RemindAllCount
RemindMeCount
FollowingCount
LastMessageId
Sorting
Ticks
OwnerId
SessionId
Settings.ReadedMessageId
```

修改字段前必须确认：

```text
数据库字段
Redis Hash 字段
RedisMapper
初始化逻辑
BatchGet
BatchSet
Dirty Flush
客户端使用方式
```

是否都需要同步调整。

------

# 19. SessionUnit Dirty 机制

SessionUnit 高频状态通常采用：

```text
Redis
    ↓
Dirty
    ↓
BackgroundJob
    ↓
Database
```

的 Write-Behind 模式。

不要为了实现简单，把它改成：

```text
每发送一条消息
    ↓
UPDATE SessionUnit
```

这会导致高频数据库写入。

------

# 20. Dirty ZSET

Dirty 数据结构必须保持现有语义。

修改以下逻辑前必须完整阅读：

```text
AddDirty
RenameDirty
ProcessingKey
GetAndRemoveProcessingDirty
FlushDirty
FlushSessionUnitJob
```

以及所有相关代码。

Dirty 机制通常需要满足：

```text
正在写入的 Dirty Key
        ↓ Rename
Processing Key
        ↓
后台处理
```

从而避免处理期间新 Dirty 数据被覆盖。

禁止在不了解 Rename / Processing 机制的情况下简化代码。

------

# 21. Dirty 去重

同一个 SessionUnit 在一批 Dirty 数据中可能被重复标记。

应尽量：

- 去重；
- 保留最新状态；
- 避免重复数据库更新。

如果 Dirty score 使用：

```text
LastMessageId
```

需要理解其版本/顺序语义后再修改。

------

# 22. FlushDirty

修改 `FlushDirtyAsync` 时必须重点检查：

```text
Dirty 总数
ProcessingKey
scanSize
jobSize
JobIndex
JobTotal
Redis Progress
异常恢复
重复执行
```

不要一次读取全部 Dirty 数据。

优先：

```text
Scan Batch
    ↓
Split Job
    ↓
BackgroundJob
```

------

# 23. BackgroundJob 进度

如果一个 ProcessingKey 被拆成多个 Job，应保持明确的：

```text
JobIndex
JobTotal
```

例如：

```text
3 / 5
```

进度写入 Redis 时应考虑：

- Job 重试；
- Job 重复完成；
- 多实例；
- ProcessingKey 唯一性；
- 完成状态；
- TTL。

------

# 24. BackgroundJob

所有 Chat BackgroundJob 默认要求：

- 可重试；
- 尽可能幂等；
- 批次有上限；
- 不加载无限数据；
- 多实例安全；
- 部分成功后可以继续；
- 重复执行不会产生错误累计。

不要假设 Job 永远只执行一次。

------

# 25. Redis Key

修改任何 Redis Key 前必须：

1. 找定义；
2. 找所有写入位置；
3. 找所有读取位置；
4. 找删除位置；
5. 找 TTL；
6. 找初始化；
7. 找数据库回源。

禁止随意改变已有 Key 格式。

如果必须修改 Redis Key，必须明确兼容策略。

------

# 26. Redis Key Prefix

必须遵循项目现有 KeyPrefix / Key Factory。

不要在代码中到处：

```csharp
$"session:{id}"
```

如果项目已有：

```text
SessionUnitKey(...)
SessionMessageSetKey(...)
OwnerSortedSetKey(...)
DirtyKey(...)
```

则继续使用已有方法。

------

# 27. Redis RTT

Redis 性能问题不能只看：

```text
Redis commandstats usec
```

Server 端命令执行只有几微秒，并不代表应用侧一次调用只耗几微秒。

还需要考虑：

```text
网络 RTT
连接等待
线程池
序列化
Pipeline
Batch
Docker 网络
客户端超时
```

高频逻辑尽量减少 Redis round trip。

------

# 28. Redis Batch

大量 Redis 操作优先使用：

```text
IBatch
Pipeline
批量 HashGet
批量 KeyExists
批量 SortedSet
```

不要写成：

```csharp
foreach (...)
{
    await redis.SomeCommandAsync();
}
```

除非数据量很小或业务需要严格串行。

------

# 29. Redis 原子性

涉及高频 Counter 时必须优先考虑：

```text
HINCRBY
INCR
ZADD
SET NX
Lua Script
```

不要使用：

```text
GET
计算
SET
```

实现并发计数。

------

# 30. Badge

Badge 相关字段属于高并发状态。

可能包括：

```text
PublicBadge
PrivateBadge
RemindAllCount
RemindMeCount
FollowingCount
Owner TotalBadge
```

发送一条消息可能同时影响多个 SessionUnit。

修改 Badge 时必须考虑：

```text
Sender
Receiver
Public
Private
Reminder
Follower
ReadState
Owner 汇总
```

------

# 31. BatchIncrement

涉及类似：

```text
BatchIncrementBadgeAndSetLastMessageAsync
BatchIncrementAsync
```

的逻辑时，不要拆成大量单用户 Redis 请求。

优先保持批量操作。

需要检查：

```text
members
reminderIds
followerIds
receiverType
isPrivate
isRemindAll
```

等语义。

------

# 32. LastMessageId

`LastMessageId` 应具有单调递增语义。

更新时应该考虑：

```text
旧值 > 新值
```

是否可能发生。

例如由于：

- Job 乱序；
- SignalR 乱序；
- Redis 延迟；
- 重试；
- 多实例；

旧消息可能晚于新消息到达。

必要时使用版本或者最大值语义，而不是无条件覆盖。

------

# 33. ReadMessageId

ReadMessageId 同样通常应保持单调递增。

不能出现：

```text
当前 ReadMessageId = 2000
请求更新 = 1900
最终变成 1900
```

除非业务明确允许。

修改已读逻辑必须考虑：

- 客户端重复请求；
- 多设备；
- 请求乱序；
- 多实例；
- PeerReadMessageId；
- 已读回执。

------

# 34. MessageStat

MessageStat 等统计数据在并发环境下不能简单采用：

```text
查询有没有
没有 -> Insert
有 -> Count++
```

而不考虑并发。

必须检查项目已有：

- 分布式锁；
- 唯一索引；
- Insert 重试；
- Update；
- Redis 聚合；

等实现。

不要引入新的并发竞态。

------

# 35. Owner SortedSet

会话列表通常使用 Owner SortedSet。

可能存在：

```text
OwnerSortedSetKey
OwnerToppingSetKey
OwnerBadgeSetKey
```

修改会话排序必须先理解：

```text
Sorting
Ticks
score
置顶
非置顶
```

之间的关系。

不要简单改 score 公式。

------

# 36. Sorting / Ticks

如果当前排序规则类似：

```text
score = sorting * MULT + ticks
```

则必须确认：

- MULT 当前值；
- JavaScript Number 精度；
- Redis double 精度；
- Sorting 最大值；
- Ticks 单位。

不要随意修改 MULT。

尤其需要避免超过 JavaScript 安全整数精度后导致前端排序错误。

------

# 37. SessionUnit 初始化

如果 SessionUnit Redis 尚未初始化，需要从数据库加载。

修改初始化流程时必须考虑：

```text
多个实例同时初始化
缓存部分存在
数据库数据量大
批量加载
Redis 超时
重复初始化
初始化期间新消息写入
```

不能简单：

```text
不存在
↓
查全部
↓
全部覆盖
```

而忽略并发期间的新数据。

------

# 38. Exists

批量判断缓存是否存在时应优先考虑现有：

```text
ExistsMapAsync
Batch KeyExists
```

不要产生大量顺序 Redis `EXISTS` 调用。

------

# 39. Message Cache 初始化

消息缓存必须区分：

```text
最新消息追加
```

和：

```text
历史消息向前补齐
```

这两个方向。

新消息只能推动缓存尾部向前。

历史数据加载主要扩展缓存头部。

不要因为历史加载覆盖最新消息。

------

# 40. 数据库回源

Redis 不是最终数据源。

如果缓存缺失或范围不足，需要根据当前设计从数据库回源。

但数据库回源必须：

- 有明确范围；
- 有 Take；
- 有 Batch；
- 有 Cursor；
- 避免一次查太多。

------

# 41. EF Core

修改 EF Core 查询前首先查看实际 SQL 语义。

重点关注：

```text
OR
UNION
IN
NOT EXISTS
JOIN
ORDER BY
TOP
OFFSET
```

不要只看 LINQ 是否简洁。

对于 Message 等大表，SQL 执行计划比 LINQ 美观更重要。

------

# 42. OR 查询

聊天查询中：

```text
Public
Private Sender
Private Receiver
```

可能产生复杂 OR。

如果当前已有：

```text
Union
```

拆分方案，不要为了代码简洁重新合并成一个巨大的 OR。

需要基于实际索引和执行计划判断。

------

# 43. Include

大表查询慎用：

```csharp
.Include(...)
.ThenInclude(...)
```

如果只需要：

```text
Id
SessionId
SenderId
```

优先 Projection。

------

# 44. ExecuteUpdateAsync

大量状态更新优先考虑：

```text
ExecuteUpdateAsync
```

或者项目已有批量更新方式。

不要对几千条 Entity：

```csharp
foreach
{
    repository.UpdateAsync(...)
}
```

产生大量 SQL。

------

# 45. 分块数据库更新

即使使用批量更新，也要控制批次。

例如：

```text
200
500
1000
```

具体采用项目现有约定。

不要构造超大的：

```text
WHERE Id IN (...)
```

导致 SQL 参数过多或执行计划异常。

------

# 46. Transaction / UnitOfWork

ABP 方法默认可能存在 UnitOfWork。

修改：

- BackgroundJob；
- 长循环；
- 大批量处理；

时必须考虑事务边界。

避免一个 UnitOfWork 包含数十万数据处理。

优先按批次提交。

------

# 47. SignalR

Chat SignalR 默认支持：

```text
多实例
一个用户多连接
一个用户多设备
断线重连
```

不能认为：

```text
UserId -> 一个 ConnectionId
```

修改连接管理前必须检查现有：

```text
ConnectionCacheManager
ConnectionId
DeviceType
ChatObjectId
Session
Redis
```

逻辑。

------

# 48. SignalR 多实例

单实例：

```text
ConcurrentDictionary
```

只能代表当前实例状态。

需要全局状态时使用项目已有 Redis / 分布式实现。

不要把全局在线状态改成内存缓存。

------

# 49. 消息推送

发送消息到客户端时应考虑：

```text
消息已经入库但推送失败
推送成功但客户端 ACK 失败
重复 SignalR 推送
客户端断线
客户端重连
多设备
```

不要设计成“SignalR 推送成功 = 消息可靠送达”。

数据库消息仍然应作为最终可靠数据。

------

# 50. 消息重复

客户端可能因为：

- 网络重试；
- 请求超时；
- SignalR 重连；

导致重复提交。

涉及消息发送时必须检查：

```text
ClientMessageId
ServerId
Idempotency
```

已有机制。

不要破坏现有客户端消息去重能力。

------

# 51. Message ClientMessageId

如果 ClientMessageId 当前承担幂等或本地消息匹配用途：

禁止随意更改：

- 类型；
- 唯一性；
- 生命周期；
- API 返回；
- 数据库索引。

需要兼容 Flutter、Uniapp、Web、Electron 等客户端。

------

# 52. API 兼容

Chat API 已经可能被多个客户端使用。

除非任务明确要求，禁止随意修改：

```text
Route
HTTP Method
DTO Property
JSON Property
Enum Value
字段语义
分页语义
```

特别是：

```text
minMessageId
maxMessageId
hasMore
readMessageId
serverId
clientMessageId
```

等字段。

------

# 53. null 与默认值

不要随意把：

```text
null
0
false
Guid.Empty
```

互相替换。

聊天系统很多字段的：

```text
null
```

可能代表“尚未初始化”，而：

```text
0
```

代表已有明确值。

修改 DTO / Entity 前必须确认语义。

------

# 54. 时间

系统时间字段需要明确单位。

尤其：

```text
Ticks
Sorting
CreationTime
LastSendTime
```

不要混淆：

```text
.NET ticks
Unix seconds
Unix milliseconds
JavaScript timestamp
```

如果项目当前 `Ticks` 使用 JavaScript 毫秒时间戳，必须保持一致。

------

# 55. DateTime

遵循项目已有 ABP Clock / 时区策略。

不要直接依赖服务器本机时区。

不要到处使用：

```csharp
DateTime.Now
```

如果项目已经统一使用：

```text
Clock
IClock
```

则继续沿用。

------

# 56. 多实例锁

如果需要锁定共享业务状态，不要直接使用：

```csharp
lock
SemaphoreSlim
```

解决跨实例并发。

根据场景使用：

- Redis 原子命令；
- ABP DistributedLock；
- 数据库唯一约束；
- Version；
- 乐观并发。

------

# 57. 分布式锁范围

使用分布式锁时：

锁粒度应尽量小。

例如优先：

```text
Message:{id}
Session:{id}
SessionUnit:{id}
```

而不是：

```text
ChatGlobalLock
```

避免所有聊天操作串行化。

------

# 58. 超时

涉及 Redis / SQL / HTTP 时，不能通过无限增加 timeout 掩盖根本问题。

如果出现超时，应先分析：

```text
调用次数
批量大小
RTT
并发
线程池
连接池
SQL
Redis slowlog
commandstats
```

然后再判断是否需要调整 timeout。

------

# 59. CancellationToken

如果调用链支持 CancellationToken，应持续传递。

特别是：

- Repository；
- ApplicationService；
- 大批量查询；
- Background processing；

不要无理由丢弃 CancellationToken。

------

# 60. 日志

高频消息路径不要增加大量 Information 日志。

性能日志优先记录聚合信息：

```text
Count
BatchSize
Elapsed
RedisElapsed
DbElapsed
JobIndex
JobTotal
ProcessingKey
```

禁止记录：

- AccessToken；
- RefreshToken；
- Password；
- Secret；
- 完整消息敏感内容。

------

# 61. Stopwatch

如果进行性能优化，可以使用 Stopwatch。

但不要永久在每条消息路径输出大量日志。

性能分析结束后应保留真正有价值的指标。

------

# 62. Migration

除非任务明确要求，不要自动生成 Migration。

禁止自动执行：

```text
dotnet ef database update
```

禁止自动运行 DbMigrator 修改实际数据库。

如果 Entity 已修改但未生成 Migration，要明确说明。

------

# 63. 索引

修改大表查询时必须同时检查已有 Index。

不要看到查询慢就立即新增索引。

需要先判断：

```text
已有索引是否可以利用
查询条件顺序
ORDER BY
筛选比例
Include Column
重复索引
写入成本
```

Message 属于高写入表，新增索引会增加 INSERT 成本。

------

# 64. 删除行为

修改：

```text
ForeignKey
Required
Optional
DeleteBehavior
```

时必须检查现有历史数据和 EF Migration。

不要随意启用 Cascade Delete。

聊天系统大表关系优先谨慎处理。

------

# 65. 测试

修改完成后优先运行受影响项目的测试。

不要因为整个 Solution 测试很多就跳过最小相关测试。

如果没有找到对应测试，要明确说明。

------

# 66. 编译

优先编译当前模块受影响项目。

例如：

```text
Domain
        ↓
Application
        ↓
EntityFrameworkCore
        ↓
HttpApi
```

根据依赖逐步验证。

最后再根据修改范围决定是否编译整个 Solution。

------

# 67. 不要修改 Rctea.IM 中的聊天业务

如果任务属于 Chat 核心逻辑：

优先修改：

```text
F:\Dev\abpvnext\IczpNet.Chat
```

不要优先修改：

```text
Rctea.IM.HttpApi.Host
```

Host 主要负责：

- 引用模块；
- 配置；
- Middleware；
- Auth；
- Hosting；
- DI；
- Swagger；
- CORS。

只有真正属于 Host 的配置才应修改主项目。

------

# 68. 跨仓库 / 跨目录修改

如果一个任务同时涉及：

```text
Rctea.IM
+
IczpNet.Chat
```

先明确每一项修改分别属于哪个项目。

例如：

```text
IczpNet.Chat：
实现业务能力

Rctea.IM：
引用模块 / 配置模块 / 注册 Host
```

不要把实现细节放错位置。

------

# 69. 调用方检查

修改公共方法之前必须搜索所有调用位置。

尤其：

```text
GetLatestAsync
FlushDirtyAsync
BatchIncrementAsync
SetListByOwnerAsync
GetManyAsync
```

等高频核心方法。

修改参数或语义之前必须确认所有调用方。

------

# 70. 修改方法签名

除非必要，优先保持已有 public 方法签名。

如果必须修改签名：

1. 找所有调用；
2. 修改调用方；
3. 检查接口；
4. 检查继承；
5. 检查测试；
6. 检查跨模块依赖。

------

# 71. 重构

可以重构，但必须服务于当前任务。

禁止为了“代码看起来更现代”进行大范围无关重构。

例如修 Redis Timeout 时不要同时：

```text
改命名
拆所有 Manager
重做 DTO
升级 ABP
改 API
重新设计数据库
```

------

# 72. TODO

如果发现问题但不属于当前任务：

不要顺手扩大修改。

可以在最终总结中列为：

```text
后续建议
```

但不要擅自实现。

------

# 73. Codex 探索大型任务

对于复杂 Chat 任务，可以并行探索不同区域：

```text
Agent 1：
Message / Repository / SQL

Agent 2：
Session / SessionUnit

Agent 3：
Redis / Cache

Agent 4：
BackgroundJob / Dirty

Agent 5：
SignalR / 在线状态
```

探索阶段优先只读。

不要让多个 Agent 同时大范围修改同一组核心文件。

------

# 74. 修改后的 Diff Review

完成修改后必须重新检查 Diff。

重点寻找：

- 无关文件；
- 意外格式化；
- Public API 变化；
- DTO 字段变化；
- Redis Key 变化；
- Migration；
- PackageReference；
- appsettings；
- Debug 日志；
- 临时代码；
- TODO；
- 注释掉的旧代码。

------

# 75. 最终报告

完成任务后至少说明：

## 修改内容

做了什么。

## 主要文件

哪些文件发生了变化。

## 数据流程

如果涉及 Redis / DB / Job，应描述最终流程。

例如：

```text
发送消息
    ↓
Redis SessionUnit 更新
    ↓
Dirty ZSET
    ↓
ProcessingKey
    ↓
BackgroundJob
    ↓
批量 EF Core Update
```

## 并发处理

说明如何保证：

```text
多实例
重试
乱序
重复执行
```

## 性能影响

说明是否：

```text
增加 Redis RTT
增加 SQL
减少 SQL
增加内存
改变批次
```

## 验证

说明实际运行过：

```text
dotnet build ...
dotnet test ...
```

以及结果。

## 未完成事项

如果有环境限制或 Migration 未生成，应明确说明。

------

# 76. 核心设计判断顺序

遇到 Chat 模块问题时，优先按照：

```text
正确性
    ↓
并发安全
    ↓
数据一致性
    ↓
性能
    ↓
可维护性
    ↓
代码简洁
```

进行设计判断。

不要为了代码更短牺牲：

- 并发；
- 一致性；
- 性能；
- 兼容性。

------

# 77. 最重要的规则

任何 Chat 核心修改都必须记住：

> Message 是大表。

> SessionUnit 是高频状态。

> Redis 是性能层，不是最终数据源。

> SQL Server 是最终持久化数据源。

> BackgroundJob 必须允许重试。

> SignalR 不是可靠消息存储。

> 多实例环境下进程内锁不能解决全局并发。

> Message.Id 全局自增不代表同 Session 内连续。

> ReadMessageId 和 LastMessageId 等状态必须考虑乱序。

> 不要为了一个局部需求破坏现有 Redis → Dirty → DB 的批量同步设计。

> 不要把 Chat 业务逻辑移动到 Rctea.IM Host。

> 修改前先理解现有数据流，修改后必须检查完整数据流。