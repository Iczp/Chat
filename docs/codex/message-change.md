# GotoIM MessageChange V3 开发执行任务书

## 一、任务定位

项目：GotoIM

涉及项目：

- `F:\Dev\abpvnext\IczpNet.Chat`
- `F:\Dev\abpvnext\Rctea.IM`
- `F:\Dev\GotoIM\gotoim-electron`

技术栈：

- ABP Framework
- .NET / EF Core
- SQL Server
- Redis
- DistributedEventBus
- SignalR
- Electron / Vue3 / TypeScript
- SQLite / Drizzle

**本次任务是对现有消息变更同步系统进行重构，不是创建新的聊天系统。**

必须充分理解当前项目已有的消息实体、数据库关系、缓存机制、消息推送、用户身份与客户端存储机制。

本次采用指定的实体模型，不再重新提出基于 SessionSeq、SessionChangeSeq 或其他独立变更状态表的替代架构。

# 二、核心设计理念

## 2.1 设计目标

GotoIM 当前需要解决的问题是：

客户端可能存储大量历史消息。当服务端发生消息撤回、修改等操作时，客户端不应该反复提交自己缓存的全部 MessageId，让服务端逐条检查。

新的设计要求：

**服务端记录消息发生了什么变化；客户端只需要知道从上次同步之后，有哪些消息发生了变化。**

整个同步过程分为：

1. 服务端产生消息变更记录。
2. 每条消息指向自己最后一次变更。
3. 终端保存已经成功应用的最大变更 ID。
4. 终端请求大于该 ID 的消息变更。
5. 服务端只返回当前有效的最新变更。
6. 终端根据 MessageId 获取消息最新状态。
7. 终端更新本地 SQLite。
8. 成功后更新本地同步游标。
9. 向服务器确认设备已经完成的同步位置。

不再使用大量本地 MessageId 作为常规变更同步请求参数。

## 2.2 消息共享范围

一条 Message 属于一个 Session。

一个 Session 对应多个 SessionUnit。

例如：

```
Session S001
│
├── SessionUnit A
├── SessionUnit B
└── SessionUnit C
```

当 `Message 5001` 被撤回时：

```
Message 5001
    ↓
新增 MessageChange
    ↓
SessionId = S001
    ↓
A、B、C 所有有权访问该消息的成员
都需要同步这个变化
```

因此：

**MessageChange 是会话级共享消息变更记录，不是用户个人变更记录。**

MessageChange 不需要以下字段：

- SessionUnitId
- DeviceId
- UserId

它只需要明确：

- 哪个 Session 发生变更。
- 哪个 Message 发生变更。
- 发生了什么类型的变化。
- 变更记录的全局 ID。

当前用户的权限及对应 SessionUnit，应在查询时通过现有会话成员关系解析。

## 2.3 个人删除不属于 MessageChange

现有 DeletedRecorder 已经用于记录个人消息删除。

例如：

```
Session S001

Message 5001

SessionUnit A：删除
SessionUnit B：未删除
SessionUnit C：未删除
```

这种操作不能写到 MessageChange。

原因：

- Message 本身没有发生面向所有成员的共享变更。
- 只有 SessionUnit A 的个人可见状态发生变化。
- 其他成员不应该收到共享消息变更通知。

继续使用现有：

```
DeletedRecorder
```

个人删除跨当前成员的多个设备同步，沿用或完善现有个人删除同步机制。

**共享消息变更与个人删除是两套独立的业务同步范围，不能混合。**

# 三、最终实体设计

## TASK-01：重新创建 MessageChange

原有 `MessageChange` 已经存在，但结构与新设计不同。

本次要求先移除旧实体和表，再重新创建新实体。

新的 MessageChange 使用：

```
public class MessageChange : Entity<long>
{
    public override long Id { get; protected set; }

    public Guid SessionId { get; protected set; }

    public long MessageId { get; protected set; }

    public MessageChangeKind ChangeType
        { get; protected set; }

    public DateTime CreationTime
        { get; protected set; }
}
```

上面仅展示核心业务字段，实际代码需要遵守项目现有的实体基类、构造函数、命名和 EF Core 约定。

### 字段规则

| 字段         | 类型     | 规则                     |
| ------------ | -------- | ------------------------ |
| Id           | long     | SQL Server IDENTITY 自增 |
| SessionId    | Guid     | 消息所属会话             |
| MessageId    | long     | 发生变更的消息           |
| ChangeType   | enum     | 消息修改、撤回等共享变更 |
| CreationTime | DateTime | 服务端变更时间           |

如果现有项目已有等价的枚举，直接复用。

不要重新定义重复的 MessageChangeKind。

### 变更范围

仅记录：

- 已发送消息修改
- 已发送消息撤回
- 未来支持的会话级共享消息状态变化

不记录：

- 普通新消息发送
- 个人删除
- 当前设备本地隐藏
- 个人会话清空
- 用户已读状态
- 普通未读数变化

除非未来有明确业务要求。

### 数据存储规则

MessageChange 采用追加模式：

```
Id = 101
MessageId = 5001
ChangeType = Updated

Id = 105
MessageId = 5001
ChangeType = Updated

Id = 120
MessageId = 5001
ChangeType = Recalled
```

允许同一条消息对应多条历史变更记录。

不需要在每次修改时删除以前的 MessageChange。

旧记录是历史流水。

**客户端最终只需要获取该消息最后一次有效变更。**

## TASK-02：扩展 Message

在现有 Message 实体中新增：

```
public virtual long? MessageChangeId
    { get; protected set; }
```

它代表：

**当前 Message 最后一次共享变更所对应的 MessageChange.Id。**

关系：

```
MessageChange.Id
       ↑
       │ 最新变更引用
       │
Message.MessageChangeId
```

关系要求：

- `MessageChangeId` 可空。
- 从未发生共享变更的 Message，值为 null。
- 每次有效修改或撤回，更新为最新 MessageChange.Id。
- 只能指向当前消息自身对应的变更记录。
- 不能因为某个成员个人删除消息而修改该字段。
- 不能破坏当前 Message.Id、SessionId 和 SessionSeq。

使用 EF Core Fluent API 配置关系。

建议：

```
builder.HasOne<MessageChange>()
    .WithMany()
    .HasForeignKey(x => x.MessageChangeId)
    .OnDelete(DeleteBehavior.Restrict);
```

实际使用项目已有的实体映射扩展方法，不要重复注册同一实体配置。

如果 MessageChange 也配置了到 Message 的物理外键，需要检查双向关系、插入顺序和循环依赖。

本次优先只将 `Message.MessageChangeId` 配置为实际外键；MessageChange.MessageId 保留为业务关联字段，并在写入时校验对应消息。

### 索引

使用 EF Core HasIndex 配置：

```
builder.HasIndex(x => x.MessageChangeId);
```

MessageChange 推荐：

```
builder.HasIndex(x => new
{
    x.SessionId,
    x.Id
});
```

是否需要其他覆盖索引，结合实际 EF Core 查询和 SQL Server 执行计划决定。

不能手写 CREATE INDEX SQL。

## TASK-03：新增 MessageChangeCursor

这是本次新增的设备同步进度实体。

名称必须使用：

```
MessageChangeCursor
```

不要改成：

```
DeviceSyncCursor
SessionMessageChangeCursor
MessageSyncCheckpoint
```

### 核心字段

```
public class MessageChangeCursor : AggregateRoot<Guid>
{
    public Guid UserId { get; protected set; }

    public Guid ChatObjectId { get; protected set; }

    public Guid DeviceId { get; protected set; }

    public long LastAppliedMessageChangeId
        { get; protected set; }

    public bool IsInitialized
        { get; protected set; }

    public DateTime? LastSyncTime
        { get; protected set; }
}
```

实际实现可使用项目现有适合的实体基类。

### 各字段作用

| 字段                       | 说明                                        |
| -------------------------- | ------------------------------------------- |
| Id                         | 游标记录主键                                |
| UserId                     | ABP 登录用户                                |
| ChatObjectId               | 当前聊天身份                                |
| DeviceId                   | 设备安装实例                                |
| LastAppliedMessageChangeId | 该设备在该身份下已成功应用的最大共享变更 ID |
| IsInitialized              | 是否已完成同步基线初始化                    |
| LastSyncTime               | 最后成功确认时间                            |

如果项目支持多租户，继承现有多租户契约，并确保唯一范围包含租户身份。

### 唯一约束

```
UserId + ChatObjectId + DeviceId
```

使用 EF Core HasIndex 配置唯一索引。

必须考虑：

**同一台设备可以登录多个用户，并且同一个用户可能切换多个 ChatObject。**

例如：

```
Device D001
│
├── User A
│   ├── ChatObject A1 → Cursor 120
│   └── ChatObject A2 → Cursor 90
│
└── User B
    └── ChatObject B1 → Cursor 75
```

这三条是独立的同步游标记录。

不能因为 DeviceId 相同就共享游标。

### LastAppliedMessageChangeId 语义

它不是：

```
服务器已经返回过的最大 ID
```

而是：

```
客户端已经成功应用并提交到本地 SQLite
的最大安全同步位置
```

客户端收到变更数据，并不代表已经完成同步。

只有本地 SQLite 事务成功提交，才可以 ACK 新游标。

### 更新规则

MessageChangeCursor 只允许向前推进。

例如：

```
当前：120

ACK 125 → 更新125
ACK 122 → 保持125
ACK 125 → 幂等成功
```

不能自动倒退。

但客户端重装、清空本地数据库、纪元重建时，应使用明确的重新初始化流程，不能继续使用旧设备 ACK 直接跳过同步。

### 不需要的关系

MessageChangeCursor 不需要为每个 SessionUnit 建一条记录。

本次共享变更采用全局 MessageChangeId。

每个 ChatObject 在当前设备只有一个共享变更游标。

# 四、全局 MessageChangeId 的并发安全

## TASK-04：保证全局 ID 可以作为安全同步游标

这是必须完成的底层要求。

新方案中：

```
MessageChange.Id = long IDENTITY
```

客户端使用该 ID 作为增量位置。

但是 SQL Server IDENTITY 的分配顺序，不保证事务提交顺序。

例如：

```
实例 A
  MessageChange.Id = 101
  尚未提交

实例 B
  MessageChange.Id = 102
  已经提交

客户端拉取并确认102

实例 A随后提交101

客户端以后只拉取 >102
结果101永久遗漏
```

这种情况不能发生。

### 解决要求

所有共享消息变更写入必须经过统一的事务性全局写入协调机制。

优先检查当前项目是否已有可以复用的全局事务协调实体或锁行。

若没有，需要增加一个最小化的基础设施实体，例如：

```
MessageChangeWriteGate
```

该实体只负责全局变更写入协调，不属于消息业务流水，也不属于设备同步游标。

可包含：

```
Id
Revision
Epoch
LastCommittedChangeId
```

使用 EF Core 创建并映射该实体。

初始化一条固定的协调记录。

每次共享变更时：

1. 开启当前 SQL Server 事务。
2. 使用 EF Core 对固定协调记录执行原子更新。
3. 通过更新锁实现全局写入串行化。
4. 创建 MessageChange。
5. SaveChanges 获取 IDENTITY ID。
6. 更新 Message.MessageChangeId。
7. 更新全局已提交变更位置。
8. 同一个事务提交。
9. 事务提交之后才能通知客户端。

实现可以使用 EF Core：

```
ExecuteUpdateAsync(...)
```

不允许直接编写 SQL 锁语句。

协调记录更新必须参与同一个数据库事务，锁必须保持到事务结束。

如果没有现成的全局锁实体，不能省略这一步，直接使用 `Max(Id)` 或 IDENTITY 值宣称可以安全同步。

### 性能要求

全局写入协调只用于共享消息修改、撤回等 MessageChange 操作。

普通新消息发送不进入此锁。

因此普通消息吞吐不应受到变更游标写入锁的直接影响。

需要测试多实例同一时间修改不同 Session 时的锁等待时间。

如果已有同等可靠的全局提交顺序机制，可以复用，但必须通过并发测试证明。

# 五、消息变更写入流程

## TASK-05：修改现有 MessageManager

当前项目已经存在撤回消息、消息保存、分布式通知等功能。

不得重新实现原有业务权限判断。

重点复用：

```
MessageManager
MessageRepository
MessageSenderAppService
现有 UnitOfWork
```

### 撤回消息流程

```
POST /api/chat/message-sender/rollback/{messageId}
    ↓
现有应用服务
    ↓
现有 MessageManager
    ↓
验证发送人和撤回条件
    ↓
开始 SQL Server 事务
    ↓
获取全局变更写入协调锁
    ↓
设置 Message.IsRollbacked = true
    ↓
创建 MessageChange
    ↓
EF Core SaveChanges
    ↓
获得 MessageChange.Id
    ↓
更新 Message.MessageChangeId
    ↓
EF Core SaveChanges
    ↓
COMMIT
    ↓
现有分布式事件
    ↓
现有 SignalR
```

两次 SaveChanges 仍必须在同一个事务内。

第一次 SaveChanges 不是提交事务。

必须保证：

- Message 变更、MessageChange、最新指针一起提交。
- 任意一步失败，整个事务回滚。
- 重复撤回保持幂等。
- 不额外产生重复的 MessageChange。
- 旧事务和当前 EF Core ChangeTracker 状态保持正确。
- 不因为多次 SaveChanges 产生重复的业务推送。

如果项目现有 ABP UnitOfWork 支持该事务，直接复用。

不允许在事务提交后的异步事件中才补写 MessageChange。

### 修改消息流程

当前项目如果没有已发送消息正文编辑功能，不要求新增编辑 UI 或 API。

但需要预留一致的变更记录入口：

```
修改 Message
    ↓
MessageChange(Updated)
    ↓
更新 Message.MessageChangeId
```

后续新增编辑功能时统一使用。

### 不记录新消息

普通发送：

```
创建 Message
```

不生成 MessageChange。

因为新消息已有现有的 `history/latest`、SessionSeq 和 SignalR 同步机制。

MessageChange 只负责已存在消息的共享状态变化。

# 六、MessageChange 的查询规则

## TASK-06：只返回最新有效变更

服务端虽然保留多条变更流水，但客户端不需要重放同一消息的全部中间状态。

例如：

```
MessageChange

Id  MessageId  ChangeType
101 5001       Updated
105 5001       Updated
110 5002       Recalled
120 5001       Recalled
```

Message 表中：

```
MessageId  MessageChangeId
5001       120
5002       110
```

客户端当前游标为100。

服务端应该返回：

```
110 → Message 5002
120 → Message 5001
```

不返回：

```
101 → Message 5001
105 → Message 5001
```

因为这两条已经不是该消息的最后有效状态。

### 查询依据

从 MessageChange 查询当前用户有权访问的会话变更。

通过 Message.MessageChangeId 判断当前记录是否为最新：

```
Message.MessageChangeId == MessageChange.Id
```

只返回满足条件的变更。

### 会话授权

请求指定：

```
ChatObjectId
DeviceId
AfterMessageChangeId
```

服务端通过当前登录用户和 ChatObjectId，确定当前身份有权访问的 Session。

只返回这些 Session 的共享变更。

不能直接把全局 MessageChange 表的所有记录返回给任意用户。

### 过滤后的游标处理

例如：

```
客户端游标 = 100

101～150：其他用户的变更
151：当前用户可见
```

服务端可以经过授权范围查询后返回151。

如果100～200之间全部是其他用户的变更，并且已经完整检查这个安全范围，仍然应该允许推进扫描游标。

不能因为没有可见消息，就永远反复查询同一段数据。

### 分页

必须返回：

```
Items
NextCursor
HasMore
HeadChangeId
Epoch
```

并确保：

- NextCursor 是已经安全扫描的进度。
- 不一定等于最后一条可见变更的 ID。
- 不要求 ID 连续。
- 不允许越过尚未提交的变更。
- 每次请求有限制的最大返回数量。
- 消息在分页期间再次变化，后续仍能取得最新状态。

查询必须使用 EF Core LINQ、Repository 等现有数据访问能力。

不能编写手动 SQL。

# 七、API 契约

优先扩展现有：

```
MessageAppService
IMessageAppService
```

遵守 ABP 自动 API、Application.Contracts 和 DTO 规范。

如果项目已有同名接口，需要检查后复用。

## TASK-07：获取同步 Head

候选接口：

```
POST /api/chat/message/changes/head
```

请求：

```
{
  "chatObjectId": "CHAT_OBJECT_A",
  "deviceId": "DEVICE_001"
}
```

响应：

```
{
  "epoch": "CURRENT_EPOCH",
  "headChangeId": "150",
  "lastAppliedChangeId": "120",
  "initialized": true
}
```

说明：

- `headChangeId`：服务端安全提交的全局变更位置。
- `lastAppliedChangeId`：MessageChangeCursor 记录的服务器确认位置。
- `initialized`：服务端该设备身份是否已有确认记录。
- `epoch`：变更流的当前纪元。

注意：返回服务器已确认的120，不代表客户端当前 SQLite 一定有120对应的数据。

不能直接使用服务端游标覆盖客户端真实进度。

## TASK-08：增量获取共享消息变更

候选接口：

```
POST /api/chat/message/changes/pull
```

请求：

```
{
  "chatObjectId": "CHAT_OBJECT_A",
  "deviceId": "DEVICE_001",
  "epoch": "CURRENT_EPOCH",
  "afterMessageChangeId": "120",
  "maxResultCount": 100
}
```

响应：

```
{
  "epoch": "CURRENT_EPOCH",
  "headChangeId": "150",
  "nextCursor": "150",
  "hasMore": false,
  "items": [
    {
      "messageChangeId": "130",
      "sessionId": "SESSION_A",
      "sessionUnitId": "SESSION_UNIT_A",
      "messageId": "5001",
      "changeType": "Recalled"
    },
    {
      "messageChangeId": "145",
      "sessionId": "SESSION_B",
      "sessionUnitId": "SESSION_UNIT_B",
      "messageId": "5002",
      "changeType": "Updated"
    }
  ]
}
```

这里：

- MessageChange 表只存 SessionId。
- 响应中的 SessionUnitId 根据当前 ChatObject 与 Session 的成员关系解析。
- 不要往 MessageChange 表增加 SessionUnitId。
- 所有 long/bigint 在 HTTP JSON 中使用十进制字符串，避免 TypeScript 精度损失。

## TASK-09：批量获取消息最新内容

优先复用现有批量消息查询、Redis 消息缓存及 DTO 构造逻辑。

如没有可直接使用的 API，可扩展：

```
POST /api/chat/message/by-ids
```

请求：

```
{
  "chatObjectId": "CHAT_OBJECT_A",
  "requests": [
    {
      "sessionUnitId": "SESSION_UNIT_A",
      "messageIds": ["5001", "5002"]
    }
  ]
}
```

服务端返回当前权威消息状态。

包括：

- 最新消息内容
- 是否撤回
- EntityVersion
- MessageChangeId
- 原有消息 DTO 必要信息

必须执行现有权限、消息可见性及删除过滤。

对于已撤回的消息，需要返回明确撤回状态。

个人删除继续按照现有 DeletedRecorder 规则处理。

## TASK-10：确认设备同步进度

候选接口：

```
POST /api/chat/message/changes/ack
```

请求：

```
{
  "chatObjectId": "CHAT_OBJECT_A",
  "deviceId": "DEVICE_001",
  "epoch": "CURRENT_EPOCH",
  "lastAppliedMessageChangeId": "150"
}
```

处理：

1. 根据 CurrentUser.Id 获取 UserId。
2. 验证 ChatObjectId 归属。
3. 验证设备身份。
4. 查找 MessageChangeCursor。
5. 如果不存在，创建。
6. 如果新游标大于旧游标，更新。
7. 相同或较小游标幂等处理。
8. 不允许确认超过当前安全提交 Head 的游标。
9. 更新 LastSyncTime。

ACK 仅用于服务端确认和设备管理。

本地 SQLite 游标依然是终端真正已经应用的状态。

# 八、首次加载规则

## TASK-11：首次同步初始化

这是必须明确实现的业务规则。

如果当前设备、当前用户、当前 ChatObject 从未同步过消息：

**不应该从 MessageChangeId = 0 开始下载全部历史变化。**

应该：

```
1. 确认当前本地同步状态未初始化
2. 获取服务端安全 Head = H
3. 按需从现有 history/latest 获取最新权威消息
4. SQLite 保存消息及初始化状态
5. 建立 LastAppliedMessageChangeId = H
6. 继续查询 MessageChangeId > H
7. 成功后 ACK
```

注意：

Head 必须在消息快照之前取得，或者通过同一个一致性快照获得。

不能先下载消息，再读取一个更晚的 Head 并跳过期间发生的变更。

### 全局游标与按需加载会话

MessageChangeCursor 是 ChatObject 级别的全局共享变更游标。

但是 Electron 可能只加载当前打开的一个会话。

因此必须建立以下规则：

- 尚未加载过的会话，将来打开时从服务端读取权威最新消息。
- 已经缓存旧消息的会话，不能因为全局游标初始化而跳过旧状态校准。
- 对已缓存会话的变更，必须更新本地消息，或者持久化待校准标记及必要墓碑。
- 只有本地状态已得到正确处理，才允许推进全局游标。
- 不能仅凭当前窗口消息数量为0，判断整个 ChatObject 可以重新初始化。

如果 Electron 已有历史消息和旧同步状态，必须执行一次可靠的 V2 迁移校准。

不能直接清空旧游标并设置为服务器最新 Head。

# 九、Electron 客户端实现

## TASK-12：复用现有 SQLite SyncCursor

重点检查：

```
MessageLocalService.ts
SyncAndCoverageRepository.ts
MainChatSignalRService.ts
AbpChatRemoteApiAdapter.ts
AuthSessionService.ts
```

共享消息变更游标建议作用域：

```
AccountId
ChatObjectId
Stream = MessageChangeV2
```

设备 ID 来自现有安装实例 DeviceContext。

如果同一个 SQLite 数据库可能保存多个服务器环境或租户的数据，还要加入对应隔离范围。

### 同步流程

```
SignalR 通知 / 登录 / 重连 / 主动检查
    ↓
读取本地 MessageChangeId
    ↓
调用 changes/pull
    ↓
获取最新有效 MessageId
    ↓
按需调用 message/by-ids
    ↓
SQLite Transaction
    ├── 更新本地消息
    ├── 更新撤回状态
    ├── 保存必要墓碑或待校准状态
    └── 保存 LastAppliedMessageChangeId
    ↓
提交成功
    ↓
调用 changes/ack
    ↓
通知 Renderer
```

### 必须处理

- 当前用户切换账号
- 当前 ChatObject 切换身份
- SignalR 重连
- 旧请求晚到
- 401 自动重试
- 多窗口共享同步
- 重复 SignalR 事件
- 消息多次修改
- 本地消息不存在
- 历史分页与变更同步同时发生
- 网络中断
- SQLite 事务失败

尤其是：

```
本地没有 MessageId
```

不能简单忽略变更并错误推进游标。

应按当前本地缓存范围决定：

- 保存变更墓碑；
- 保存待校准标记；
- 或通过权威消息查询确认最新状态。

确保之后历史消息加载不会重新显示已撤回内容。

# 十、个人删除继续独立同步

## TASK-13：保留 DeletedRecorder 机制

本次 MessageChange 只负责共享消息变更。

DeletedRecorder 负责个人删除。

不允许：

```
个人删除
    ↓
新增共享 MessageChange
```

正确流程：

```
用户 A 在 Windows 删除消息
    ↓
DeletedRecorder
    ↓
通过现有个人删除通知或增量机制
    ↓
用户 A 的其他设备同步个人删除
```

必须检查现有 DeletedRecorder 是否已经具备长期离线增量恢复机制。

如果没有，需要在 DeletedRecorder 业务范围内完善其独立游标或同步机制。

但不允许借用共享 MessageChangeId 表示个人删除进度。

同样，清空当前用户会话属于个人可见状态，不写入共享 MessageChange。

# 十一、EF Core 两次数据库迁移

## TASK-14：第一次迁移——移除旧 MessageChange

**必须按照指定的两次迁移执行，不允许直接修改旧表主键。**

原有 MessageChange 的 Id 为 Guid，新方案采用 long IDENTITY。

因此先移除旧实体，生成一次 EF Core Migration。

### 第一步：检查当前项目

检查：

- 原 MessageChange 实体
- DbContext DbSet
- IEntityTypeConfiguration / Fluent API
- Repository
- DomainManager
- ApplicationService
- 事件处理器
- DTO
- 关联实体
- EF Core Migration 历史
- ModelSnapshot

检查当前 Git 状态，保护其他未提交代码。

确认数据库连接和执行环境。

### 第二步：移除旧实体映射

暂时从 EF Core 模型中移除原 MessageChange。

需要同步处理依赖旧实体的代码，使项目可以编译并生成迁移。

不要删除无关的 Message、Session、SessionUnit 或 DeletedRecorder 数据。

对于 `SessionMessageChangeState`，必须先检查是否仍有其他现有业务使用。

不要因为删除 MessageChange，就盲目删除它。

### 第三步：生成第一次 Migration

命名建议：

```
RemoveLegacyMessageChange
```

使用当前项目已有的 EF Core 工具、DbContext 和启动项目。

例如：

```
Add-Migration RemoveLegacyMessageChange
```

具体命令必须根据实际 DbContext、Migrations 项目和启动项目调整。

检查 Migration。

预期只删除旧 MessageChange 表及必要的关联对象。

不允许包含无关业务表的 DropTable。

### 第四步：应用第一次 Migration

只允许在明确的开发或隔离测试数据库执行。

执行前：

- 检查数据库备份。
- 确认旧 MessageChange 数据是否需要归档。
- 确认旧 V1 变更接口的迁移影响。
- 确认当前环境允许删除旧表。

然后通过 EF Core：

```
Update-Database
```

确认旧表删除成功，EF Core 迁移历史正确。

**原有数据在 DropTable 后会被删除。**

这属于明确的数据破坏性迁移，不能默认在生产数据库执行。

如果存在仍依赖 V1 的旧终端，必须在部署计划中明确升级或停用方案。

## TASK-15：第二次迁移——重新创建 MessageChange

第一次迁移完成后：

1. 创建新的 MessageChange 实体。
2. 使用 `Entity<long>`。
3. 配置 long IDENTITY 主键。
4. 配置 SessionId、MessageId、ChangeType。
5. 扩展 Message.MessageChangeId。
6. 创建 MessageChangeCursor。
7. 配置唯一索引和外键。
8. 配置必要的全局写入协调机制。
9. 更新 DbContext。
10. 确认项目可编译。

生成第二次 Migration：

```
CreateMessageChangeV2
```

通过 EF Core 生成：

```
Add-Migration CreateMessageChangeV2
```

检查 Migration 后，在隔离数据库执行：

```
Update-Database
```

确认：

- MessageChange.Id 为 bigint IDENTITY。
- Message.MessageChangeId 可空。
- 关系和索引正确。
- MessageChangeCursor 已创建。
- 全局提交顺序协调结构已建立。
- 原有 Message、Session、SessionUnit 数据保留。
- EF Core ModelSnapshot 与实际模型一致。

### EF Core 限制

本次明确禁止：

- 手写 CREATE TABLE SQL。
- 手写 DROP TABLE SQL。
- 手写 ALTER TABLE SQL。
- 手写 CREATE INDEX SQL。
- 使用 ExecuteSqlRaw 执行业务变更。
- 使用 FromSqlRaw 编写变更同步查询。
- 绕过 EF Core 直接修改数据库结构。
- 手动删除或伪造 EF Core 迁移历史。

允许：

- EF Core Migration。
- EF Core Fluent API。
- EF Core LINQ。
- EF Core Repository。
- SaveChangesAsync。
- ExecuteUpdateAsync。
- EF Core 生成的迁移操作，例如 DropTable、CreateTable、CreateIndex。

所有数据库结构变更由 EF Core 管理。

# 十二、旧协议与历史数据处理

## TASK-16：处理原有 V1 兼容问题

这次要求删除旧 MessageChange 表，再创建新的不同结构，因此不能假装旧协议天然兼容。

必须分析旧接口：

```
/api/chat/message/changes/query
/api/chat/message/sync-items
```

如果原接口依赖旧 Guid MessageChange 和 Session Version，必须明确处理。

要求：

1. 不允许旧接口静默返回错误数据。
2. 不允许旧 Cursor 被当成新的 long MessageChangeId。
3. V2 使用独立客户端同步 Scope。
4. 原来客户端已有的本地消息不能因为游标重建而丢失。
5. 旧本地消息必须进行必要的权威状态校准。
6. 对不兼容的旧客户端返回明确升级或协议不支持结果。
7. 只有验证旧数据兼容时才允许继续启用 V1 入口。

若旧日志被删除，过去的变更不能假设可从新空表直接恢复。

应通过现有权威 Message 状态和 DeletedRecorder 进行一次初始化或校准。

# 十三、Redis 与 SignalR

## TASK-17：复用现有 Redis

本次不重新开发普通消息缓存。

继续使用现有：

- Session 消息缓存
- Redis ZSET
- ReadWindow
- SQL Server 回源
- MessageCacheManager
- 消息删除过滤
- 消息正文缓存

MessageChange V2 第一版直接通过 EF Core 查询 SQL Server。

不要为了本次重构强制新建 Redis Change ZSET/HASH。

如果后续性能测试证明必要，再单独优化。

## TASK-18：复用现有分布式事件与 SignalR

现有项目已经具备分布式事件推送。

不创建新的 SignalR Hub。

业务事务提交后，继续使用现有通知链路。

通知内容可以增加：

```
SessionId
MessageChangeId
MessageId
ChangeType
```

推送是提示，不是同步的唯一可靠依据。

客户端收到通知后调用 changes/pull。

断线重连时根据本地游标补偿。

由于现有事件总线可能采用 Redis Pub/Sub，客户端必须允许丢通知，并能通过主动增量检查恢复。

# 十四、完整测试任务

所有功能实现完成后统一执行测试。

允许在开发过程中进行必要编译检查，但不要每修改一个小功能就执行完整端到端测试。

## 服务端

- MessageChange 自增 ID。
- MessageChangeCursor 唯一约束。
- 修改消息新增变更。
- 撤回消息新增变更。
- 重复撤回不新增无意义变更。
- Message.MessageChangeId 指向最新记录。
- 事务失败时指针与日志同时回滚。
- 多实例并发提交顺序安全。
- 同一消息连续修改10次。
- 增量查询只返回最后一次有效状态。
- 按 ChatObject 授权过滤。
- 不泄露其他会话消息。
- 空结果游标正常推进。
- 增量分页。
- 数据库迁移执行。
- Epoch 重建。
- 个人删除不产生共享 MessageChange。
- 旧协议迁移处理正确。

## Electron

- 首次安装。
- 首次登录。
- 首次打开会话。
- 已有消息增量同步。
- 多个会话共享一个全局游标。
- 同一消息多次变化。
- 修改后撤回。
- 本地不存在的旧消息发生撤回。
- 历史消息按需加载。
- 多窗口同步。
- 同一设备切换用户。
- 同一用户切换 ChatObject。
- SQLite 写入失败。
- ACK 失败。
- SignalR 断线。
- SignalR 丢失通知。
- 重新登录。
- 清空 SQLite 后重新建立基线。
- 长期离线恢复。

## 性能

测试：

```
100个会话，没有共享变更
100个会话，只有2个会话发生变化
同一条消息连续修改10次
多个设备同时同步
多个实例同时撤回不同会话消息
```

期望：

- 不再全量提交客户端消息 ID。
- 不再每次重连重复拉取所有会话 history。
- 只返回当前有效的最新变更。
- 同一 ChatObject 只维护一个共享变更游标。
- 无变更时可以快速返回。
- 全局写入协调的锁等待在可接受范围内。

# 十五、实施顺序

必须按照以下依赖顺序开发，但最终作为一个完整功能交付。

| 顺序 | 任务                                  |
| ---- | ------------------------------------- |
| 1    | 检查现有代码、实体与未提交修改        |
| 2    | 移除旧 MessageChange 实体及映射       |
| 3    | 生成 RemoveLegacyMessageChange 迁移   |
| 4    | 在隔离数据库应用第一次迁移            |
| 5    | 新建 MessageChange(long IDENTITY)     |
| 6    | 扩展 Message.MessageChangeId          |
| 7    | 新建 MessageChangeCursor              |
| 8    | 配置 EF Core 关系、索引和全局写入协调 |
| 9    | 生成 CreateMessageChangeV2 迁移       |
| 10   | 在隔离数据库应用第二次迁移            |
| 11   | 修改 MessageManager 和 Repository     |
| 12   | 实现 Head / Pull / Ack API            |
| 13   | 扩展批量查询消息最新状态              |
| 14   | 对接现有分布式事件和 SignalR          |
| 15   | 修改 Electron SQLite 游标及同步服务   |
| 16   | 完成首次初始化及历史状态校准          |
| 17   | 保持个人删除独立处理                  |
| 18   | 完成旧协议迁移及异常处理              |
| 19   | 统一执行全部测试                      |
| 20   | 修复问题，输出交付报告                |

# 十六、开发限制

本任务必须遵循：

1. 使用现有项目，不创建新的聊天项目。
2. 使用 ABP 原有分层架构。
3. 优先复用原有 Service、Manager 和 Repository。
4. 使用 ABP 自动 API 和现有 Application.Contracts。
5. 业务状态更新使用 UnitOfWork。
6. 所有数据库操作通过 EF Core。
7. 不允许手写 SQL。
8. 数据迁移分两次执行。
9. 不允许直接将旧 Guid 主键强行修改为 long。
10. 不允许直接删除全部 Migration 重建数据库。
11. 不允许修改普通消息发送和 SessionSeq 的原有机制。
12. 个人删除继续使用 DeletedRecorder。
13. MessageChange 不包含 SessionUnitId 和 DeviceId。
14. MessageChangeCursor 必须包含 UserId、ChatObjectId、DeviceId。
15. 同一账号多个设备游标独立。
16. 同一设备多个用户游标独立。
17. 不允许在本地事务成功之前 ACK。
18. 不允许出现全局 ID 提交乱序导致漏同步。
19. 不重新开发已有 Redis、分布式事件与 SignalR。
20. 不覆盖其他任务已有的未提交代码。
21. 不在生产数据库自动执行破坏性迁移。
22. 不新增与任务无关的后台管理页面。
23. 不擅自改变消息历史可见性及权限规则。
24. 不能只完成服务端而遗漏 Electron 客户端。
25. 最终必须保证全部必需功能完成后再统一验收。

# 十七、最终交付要求

Codex 完成后必须输出：

### 1. 实体变更报告

包括：

- MessageChange
- Message.MessageChangeId
- MessageChangeCursor
- 必需的全局写入协调结构
- 所有 EF Core 索引与关系

### 2. EF Core 迁移报告

包括：

- 第一次迁移名称
- 第二次迁移名称
- 删除旧表的范围
- 新表与新字段
- 开发数据库执行结果
- 数据备份、兼容及恢复要求

### 3. API 契约

列出：

- 接口地址
- HTTP Method
- 请求 DTO
- 响应 DTO
- 参数含义
- 鉴权规则
- 游标推进规则
- 错误处理

### 4. 客户端调用链

提供完整：

```
Head
 ↓
Pull
 ↓
按需批量获取消息最新内容
 ↓
SQLite Transaction
 ↓
ACK
```

以及首次加载、断线恢复、多账号切换流程。

### 5. 测试报告

列出：

- 已通过测试
- 失败及已修复测试
- 未执行及原因
- 并发测试结果
- 性能测试结果
- 遗留风险

不得把未执行测试标记为已通过。

# 十八、最终验收标准

本次最终设计必须达到：

**一个 ChatObject，在一台设备上，只需要维护一个共享消息变更游标。**

服务端：

```
MessageChange
    全局自增 ID
    记录所有 Session 的共享消息变化

Message
    MessageChangeId
    指向该消息的最后一次共享变更

MessageChangeCursor
    UserId + ChatObjectId + DeviceId
    保存服务端已确认的同步位置

DeletedRecorder
    继续独立处理个人删除
```

客户端：

```
获取本地游标
    ↓
增量获取当前 ChatObject 可见的共享消息变更
    ↓
获取发生变化的消息最新状态
    ↓
更新 SQLite
    ↓
保存本地最大已应用游标
    ↓
服务端 ACK
```

任何时刻发生网络故障、客户端退出、SignalR 丢通知、数据库事务回滚，都不允许造成永久漏同步。

**请按以上设计直接开始修改现有三个项目，不要重新生成其他架构方案，也不要只输出分析或计划。**

如果现有代码存在阻碍，请在保持上述设计目标不变的前提下解决兼容问题，并在最终报告中说明真实改动及原因。