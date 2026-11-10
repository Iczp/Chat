# CODEX 任务书：IczpNet.Chat 历史消息 SessionSeq 数据回填与唯一索引

## 0. 任务定位与已知事实

本任务面向 Codex / AI 编程助手。**请直接检查真实仓库、编写代码、构建、测试，并在允许的本地开发数据库中执行历史数据回填；不要停留在设计说明。**

- 工作区：`F:\Dev\abpvnext\IczpNet.Chat`
- 启动项目：`F:\Dev\abpvnext\IczpNet.Chat\host\IczpNet.Chat.HttpApi.Host\IczpNet.Chat.HttpApi.Host.csproj`
- 技术栈：ABP vNext、EF Core、SQL Server；系统支持多实例部署。
- 现有 `Message.Id` 为全局自增消息主键；`Message.SessionId` 关联 `Session.Id`，**`Session.Id` 是 `Guid`（SQL Server `uniqueidentifier`），不是 `long`**。
- 目标：为同一 `SessionId` 内的消息按照原有 `Message.Id ASC` 确定 `SessionSeq = 1,2,3...`，并同步 `Session.LastMessageSeq`。保留所有原有 ID、外键和消息内容。
- **已执行成功的迁移：`20261109093244_MessageSessionSeq`**。用户已经执行 `Add-Migration MessageSessionSeq`、`Update-Database`，结果为 `Done`。**不要重新创建、移除或重放这个已应用迁移，不要重建消息表。**
- 目前任务顺序：① 检查实际结构和既有迁移；② 历史数据回填；③ 完整校验；④ **再**创建并应用 `SessionId + SessionSeq` 唯一索引相关的 EF Core Migration；⑤ 验证现有发送流程的多实例并发安全。
- 原迁移脚手架曾提示“An operation was scaffolded that may result in the loss of data”。必须审核原迁移 `Up/Down` 的实际变更，确认其没有意外修改/删除其他数据。

## 1. 强制执行规则

1. **先检查现有项目再动手**。真实实体名、EF 映射、表名（可能有前缀与 schema）、迁移程序集、DbContext、字段类型、字段是否可空、默认值、索引和 ABP UnitOfWork 必须以仓库与数据库为准；不要假定表名为 `dbo.Message` 或 `dbo.Session`。
2. 所有数据库**结构变更只能使用 EF Core Migration**；不能直接从 SSMS 手工 ALTER/CREATE/DELETE。历史数据计算允许使用**EF Core 管理连接和事务**的 `Database.ExecuteSqlInterpolatedAsync` / ADO.NET 命令执行参数化 SQL Server `ROW_NUMBER()`，不能通过未受控脚本绕过任务程序。
3. 绝不删除/重建历史消息或重排现有 `Message.Id`；不能把 `Session.Id (Guid)` 转成 `long`；不得使用 GUID 加 1、把 GUID 当连续整数范围或使用 C# `Guid > Guid` 运算符。
4. 历史迁移必须有**独立、显式的执行入口**，默认禁用，且仅允许**一个迁移执行者**。不能把回填放在所有 Host 的启动、`OnApplicationInitialization` 或每次应用启动时自动运行。EF Core 的 schema migration lock 不等于自定义历史回填任务的互斥锁。
5. **历史回填期间停止全部 Message 写入**：API 发送、后台任务、机器人、消费队列、其他实例等；读流量可视锁影响决定是否保留。对于尚未同步迁移的旧实例，禁止恢复发送。执行前确认数据库与备份，生产库未经明确批准不能进行破坏性操作。
6. 数据回填任务必须**分批、小事务、可重复执行（幂等）**，日志输出进度、耗时、失败位置；任何批次失败应完整回滚，允许安全重试。不能把几百万消息一次加载进 C# 内存；不建议一条超大事务更新全表。
7. 后续正常写入必须在**同一个 SQL Server 事务**内原子增加 `Session.LastMessageSeq`、读取本事务的最新值、插入 `Message`；通过 ABP UOW / EF Core 事务管理，不能采用“先 SELECT，再 `++`，再 SaveChanges”无并发控制的模式。
8. 保留现有 MessageId 游标、Redis ZSET 排序、撤回/删除、SignalR 与客户端逻辑；本任务不擅自全面切换同步游标为 SessionSeq。
9. 修改前阅读 `AGENTS.md`、相关代码规范以及已有迁移；遵循仓库约定。未经需要不改变其他模块。

## 2. 阶段 A：只读检查与基线报告

请定位并审核：

- `20261109093244_MessageSessionSeq` 文件、ModelSnapshot、实际数据库 `__EFMigrationsHistory`，核实这次迁移已经在**当前连接的目标数据库**执行。
- `Message`、`Session` 的 Domain/EF Core 实体及配置、`Id` 类型、`SessionId` 的 nullable/FK 关系、`SessionSeq` 类型/nullable/default、`LastMessageSeq` 类型/default，是否已加唯一索引，是否存在额外的意外 schema 变动。
- 数据库连接字符串对应的服务器、数据库名和环境（日志中不输出密码）。
- Message 总数、Session 总数；`SessionSeq IS NULL` / `=0` / `>0` 各有多少；Session.LastMessageSeq 当前的分布。
- 无效数据：`SessionId IS NULL`、没有对应 Session 的 Message、已存在非零或重复 SessionSeq 的历史记录、负值、软删除/过滤器造成的数据遗漏。任何异常需要先制定规则，禁止静默丢弃。
- 查看已有 `(SessionId, Id)` 等索引，评估批量 `ROW_NUMBER()` 的执行计划；先确认磁盘空间、事务日志和可用备份。

**门禁：**如果数据已存在有效 `SessionSeq > 0`、无法区分新增与历史记录、原迁移修改了非预期列、或者目标数据库是未经授权的生产环境，停止写入操作，输出诊断和修复建议，不能盲目整体重排。

## 3. 阶段 B：实现一次性历史数据回填程序

优先复用现有 ABP DbContext/DI/Repository、项目里的初始化或数据迁移基础设施；没有合适设施则在 Host 对应解决方案内新增可单独调用的迁移 Runner/命令，**必须显式参数（例如 `--backfill-message-session-seq`）才执行**，且不要启动成常驻多个副本。执行参数至少支持：`--dry-run`、`--batch-size`、可选的恢复位置/进度输出；`--dry-run` 仅作只读检查。

### 3.1 迁移规则

- 以数据库中的**全部物理 Message 记录**为准，按 `SessionId` 分区，严格按 `Message.Id ASC` 分配 `ROW_NUMBER()`，从 1 开始；不可使用 `CreationTime`、客户端时间或随机排序代替。
- 如果软删除/全局数据过滤器会隐藏部分记录，历史回填必须经评估后显式覆盖应参与编号的记录；不要使软删除消息与正常消息占用相同 SessionSeq。
- Session 无消息时 `LastMessageSeq = 0`；有消息时 `LastMessageSeq = MAX(Message.SessionSeq)`。
- 如果 SessionId 引用不存在或为空，先报告数量及示例，确认业务意义后处理；不得自动伪造 Session 或跳过有问题的消息。
- 优先按**完整 Session**分批，避免一个 Session 的消息被错误分到多个独立编号批次。每批建议 100~500 个 Session，须根据真实负载调优；超大单 Session 需特别处理事务大小。

### 3.2 GUID 批量分页（非常重要）

- `Session.Id` 为 `uniqueidentifier`；使用 SQL Server 中稳定的 `ORDER BY [Session].[Id]` 和 `WHERE [Id] > @LastGuid` 做 keyset 翻页，**`@LastGuid` 是 GUID 参数**，首次执行不加游标限制。SQL Server 支持 `uniqueidentifier` 比较运算，但其排序规则并非 GUID 文本/位模式的直观大小，**游标筛选与排序必须全部在 SQL Server 中执行**。
- 不使用 `Id BETWEEN minGuid AND maxGuid` 表示“业务上的连续 GUID”；不使用 C# `Guid` 加减或 `Guid.CompareTo` 的本地排序来构造服务器游标。
- 对选中的 SessionId 执行集合式 `ROW_NUMBER() OVER (PARTITION BY SessionId ORDER BY Message.Id)`，更新对应 Message.SessionSeq，随后在**同一个数据库事务**内更新 Session.LastMessageSeq。
- 推荐在同一批 SQL 命令中使用表变量/临时表保存本批 GUID 列表（或 EF/ADO.NET 参数化 TVP），避免直接拼接 GUID 到 SQL；所有实际 table/schema 名通过已审核的 EF 映射确定。
- 每批事务成功提交后记录：首尾 GUID、Session 数、Message 变更数、耗时、累计数；捕获 SQL Server 死锁/超时异常并使用有限重试。避免把部分成功视为整批成功。
- 必须保证**幂等性**：重复运行时相同历史数据得到相同 SessionSeq。若不引入数据库持久化 checkpoint，可安全从头重跑并重算；若实现断点续跑，checkpoint 必须在批次提交后推进，不能跳过未成功提交的批次。任何辅助进度表都必须由 EF Migration 管理，并解释是否需要保留。

**参考 SQL 逻辑（仅作为模板，不要照抄表名；实际应由 EF Core 管理事务执行）：**

```sql
-- @BatchSessions 由前一步参数化选取 GUID 并填入，例如：
-- SELECT TOP(@BatchSize) s.Id
-- FROM [实际Session表] s
-- WHERE @LastGuid IS NULL OR s.Id > @LastGuid
-- ORDER BY s.Id;

;WITH Ranked AS
(
    SELECT m.Id,
           ROW_NUMBER() OVER
           (PARTITION BY m.SessionId ORDER BY m.Id ASC) AS NewSeq
    FROM [实际Message表] m
    JOIN @BatchSessions b ON b.SessionId = m.SessionId
)
UPDATE m
SET m.SessionSeq = r.NewSeq
FROM [实际Message表] m
JOIN Ranked r ON r.Id = m.Id;

;WITH MaxSeq AS
(
    SELECT b.SessionId, COALESCE(MAX(m.SessionSeq), 0) AS LastSeq
    FROM @BatchSessions b
    LEFT JOIN [实际Message表] m ON m.SessionId = b.SessionId
    GROUP BY b.SessionId
)
UPDATE s
SET s.LastMessageSeq = q.LastSeq
FROM [实际Session表] s
JOIN MaxSeq q ON q.SessionId = s.Id;
```

> 说明：此模板假设 Message.Id 唯一。任何更新都必须基于已确认的目标表、正确参数和事务。SQL Server 的 `ROW_NUMBER()` 结果类型为 `bigint`，核对 SessionSeq 列类型与溢出边界。

## 4. 阶段 C：数据验收（未通过绝对不能加唯一索引）

请实现**可重复执行的只读校验**，输出数量及前 20 个异常示例。使用实际表名/字段：

1. `Message.SessionSeq IS NULL OR SessionSeq <= 0` 数量必须为 0（如果业务允许特例，需明确列出并由用户批准）。
2. 重复键 `GROUP BY SessionId, SessionSeq HAVING COUNT_BIG(*) > 1` 必须返回 0 行。
3. 对每个 Session，`LastMessageSeq` 必须等于对应全部 Message 的 `MAX(SessionSeq)`；无消息时为 0。
4. 校验 Message 总行数、每 Session 消息数与回填前一致；历史 `Message.Id`、`SessionId`、正文、类型和关联字段未改变。
5. 校验顺序：同一 Session 按 `Message.Id ASC` 查询，`SessionSeq` 严格递增且历史数据从 1 开始；抽样包含大群会话、跨时间段会话、已删除/撤回消息及空 Session。
6. 检查 orphan/空 SessionId、业务特殊消息、是否存在旧版本应用还在写入；校验时须保证写入仍已暂停。

提供 `dry-run` 结果和回填后结果，输出在仓库中的执行报告/日志（不包含账号密码、Token 或敏感正文）。若失败，修复原因并重新验证，不可忽略错误直接加索引。

## 5. 阶段 D：添加索引的第二个 EF Core Migration

**只能在阶段 C 全部通过后执行。**

1. 检查 `SessionSeq` 是否 nullable；若仍可空，则在新 Migration 中转换为 `NOT NULL`，但必须先证实没有 NULL。
2. 在 Message 实体的 EF Core 映射上配置唯一复合索引 `(SessionId, SessionSeq)`：

```csharp
builder.HasIndex(x => new { x.SessionId, x.SessionSeq })
       .IsUnique();
```

3. 若当前 `SessionSeq` 存在给旧记录兼容的 `DEFAULT 0`，评估并移除该默认约束，防止未来绕过生成器的插入静默写入 0；所有新消息必须显式设置非零序号。
4. 通过项目实际迁移程序集生成**新的**迁移（可命名为 `AddMessageSessionSeqUniqueIndex`），检查 `Up/Down`，确认只包含预期的列约束、默认值和索引变更；生成并检查 SQL 脚本，注意大表索引构建锁和空间需求。
5. 只在经过确认的开发/测试数据库应用。生产环境输出执行方案和脚本供批准，禁止自动执行未经授权的生产数据库变更。
6. 再查 `__EFMigrationsHistory` 和 SQL Server 系统索引元数据，确认唯一索引真实存在且未被意外过滤掉有效行。

## 6. 阶段 E：检查并修复后续消息写入的并发安全

搜索仓库中所有创建 Message 的入口（用户发送、系统/机器人发送、批量发送、队列消费者、后台任务等），统一以下规则：

```csharp
// 示例，仅展示核心原则；放入真实 ABP 事务性 UnitOfWork 内
var affected = await dbContext.Sessions
    .Where(x => x.Id == sessionId)
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(x => x.LastMessageSeq,
                     x => x.LastMessageSeq + 1), cancellationToken);
if (affected != 1) throw new BusinessException("SessionNotFound");

var nextSeq = await dbContext.Sessions
    .AsNoTracking()
    .Where(x => x.Id == sessionId)
    .Select(x => x.LastMessageSeq)
    .SingleAsync(cancellationToken);

// 在同一个 SQL Server 事务内插入 Message(SessionId, SessionSeq = nextSeq)
// 最后提交 ABP UOW；事务失败必须整体回滚
```

- 禁止无保护的 `session.LastMessageSeq++` 并发读改写。
- `ExecuteUpdateAsync` 不更新 EF ChangeTracker，避免旧的已跟踪 Session 实体在 `SaveChanges` 时覆盖刚递增的 LastMessageSeq；必要时拆分 DbContext 使用或精确更新属性。
- `ExecuteUpdateAsync`、读取 seq、`Insert Message` 必须复用**同一个事务/连接**。若 UOW 不保证事务，则显式保证，不允许先提交 seq 再另起事务写 Message。
- 不能在事务中进行耗时外部请求/上传/SignalR 推送；提交后再通过现有事件 / Outbox 推送。
- `SessionSeq` 不替代 Message.Id 全局唯一标识；业务重试要有单独幂等键，防止一次请求失败后重复创建业务消息。
- 多实例同时发送同一个 Session 会出现写锁等待是正常的；必须测试死锁、超时和有限重试，避免无上限重试。

## 7. 测试与交付验收

**编译/测试门禁：**先完成后端及数据库验收，不做前端改造。

- 目标 Host 项目 `dotnet build "F:\Dev\abpvnext\IczpNet.Chat\host\IczpNet.Chat.HttpApi.Host\IczpNet.Chat.HttpApi.Host.csproj"` 成功；其他受影响后端项目编译成功。
- 使用**实际独立数据库连接/DbContext 实例**模拟 2~3 个服务节点并发向同一 Session 写入至少 100 条消息；断言全部成功的 `(SessionId, SessionSeq)` 唯一，且最终 `LastMessageSeq = MAX(SessionSeq)`。
- 混合 2 个或更多 Session 并发发送，断言不同 Session 可分别从正确的下一序号继续。
- 模拟插入失败/事务回滚，确认 Session 计数不会单独前进；验证 SQL Server 唯一约束能拒绝重复值。
- 回填重复执行或中断重跑时数据不变；空 Session、超大 Session、历史删除/软删除记录均符合规则。
- 确认旧的 MessageId 查询、游标、Redis 缓存逻辑不受破坏；未计划内的前端改动不得进行。

**最终请输出：**

1. 已修改文件清单（相对路径）及原因。
2. 真实数据库 schema、迁移文件、已应用迁移、目标库环境的核对结果。
3. 回填前后的 Message/Session 总数、需回填数量、实际回填数量、批次数、耗时和失败重试统计。
4. 六类校验结果（非正序号、重复键、计数不一致、孤儿消息、排序异常、消息行数变化）。
5. 新增唯一索引的 Migration 名称、SQL 脚本位置以及是否已经应用。
6. 并发压测结果和事务回滚测试结果。
7. 遗留风险、线上维护窗口建议、回滚/恢复步骤。不要声称未经执行的测试已通过。

## 8. 操作边界与执行方式

- 现在的目标是**在用户已经执行第一轮 Migration 的数据库上做数据回填，然后才添加唯一索引**。
- 允许修改本地仓库和经核实的本机开发数据库；生产数据库、共享数据库、未知数据库必须先给出诊断与操作计划，不得假定用户已经授权直接更改。
- 不要执行 `Remove-Migration` / 回滚已应用的 `20261109093244_MessageSessionSeq` / `EnsureDeleted` / 清表或重建表。
- 不要在未核实实际表名、schema、数据库环境时直接执行示例 SQL。
- 优先交付可运行的迁移工具及测试；如果执行环境无法连接用户的本地 Windows 目录或数据库，明确报告这个限制并交付可以在本机直接运行的命令、代码与验收步骤，不得假装已运行。

请立即从“阶段 A：只读检查”开始逐步实施，直到可安全完成的所有阶段均已完成，并对任何阻塞给出准确证据。
