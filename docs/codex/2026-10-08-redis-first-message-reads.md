# History / Latest Redis 优先查询

日期：2026-10-08。修改属于 Chat 模块的 Domain、Domain.Shared、Application 和 EntityFrameworkCore；Rctea.IM Host 未修改。没有修改数据库实体、索引、Migration、接口参数或已有 Redis Key 格式。

## 数据流程

1. Application 从已有 SessionUnit Redis Hash 加载成员，并通过已有用户聊天对象缓存检查归属和原有列表权限。
2. 并行读取成员可见性设置和共享会话消息窗口。窗口包含有界、按 ID 升序排列的轻量元数据，不包含完整消息内容。
3. 按私密参与者、Killed、Start/EndMessageId、HistoryFristTime/HistoryLastTime、ClearTime 过滤；批量 HMGET 读取候选消息的成员删除标记。
4. 返回消息 ID，通过原来的 GetOrAddManyCacheAsync 批量读取内容。
5. 窗口缺失或不覆盖游标时，按相同方向从数据库投影最多 batchSize + 1 条元数据，建立可证明完整的开放区间。删除标记只回源缺失的候选 ID。
6. 扫描预算耗尽或 Redis 故障时，保留最后扫描游标，按现有 Repository 可见性条件有界兜底；不跳过用作区间边界的额外候选。

MessageManager 通过主构造器注入 IMessageCacheManager 和 ICancellationTokenProvider。每次方法调用读取 Provider.Token，继续向数据库、缓存和冷窗口锁传递，不在构造时固定 Token。

旧共享 ZSET 没有覆盖证明，且写入发生于事务提交前，因此不能用其实际 min/max 或候选条数声明完整命中。新的读取窗口通过现有 MessageCacheManager 实现；保留旧 ZSET 和原内容缓存，避免把未提交 ZSET 条目作为返回依据。

## 缓存与并发

沿用现有 KeyPrefix + `Messages:` 前缀：

- `ReadWindow:{sessionId}:Recent`：认证到消息尾部的有界窗口。
- `ReadWindow:{sessionId}:History`：一个有界历史窗口。
- `ReadWindow:{sessionId}:Version`：阻止并发回源覆盖已提交变更的版本。
- `ReadSettings:{sessionUnitId}` / `:Version`：读可见性设置与版本。直接从现有 Repository 和 Mapper 建立，避免信任旧设置缓存在提交前写入或回滚后残留的值。
- `ReadDeleted:{sessionUnitId}` / `:Version`：候选消息删除状态 Hash 与版本；每个 Hash 最多保留 4,096 项，满时淘汰后按候选回源，不加载该成员全部删除记录。

数据窗口、设置和删除 Hash 默认 5 分钟过期。版本键故意不设置 TTL：防止回源期间版本消失后发生空版本 ABA；每个发生过相关变更的会话/成员会留下小型版本键，应计入 Redis 容量规划。

近期窗口追加和删除 Hash 补充字段保留首次建立时的剩余 TTL，不因持续发消息而无限续期，使偶发提交后同步失败仍能通过有界过期和重新回源恢复。

Message 创建事件注册 OnCompleted，事务提交后才原子更新近期窗口。普通新消息不会清掉旧历史窗口；历史区间内的迟提交使该窗口失效。近期窗口追加采用线性有序插入、去重和裁剪，ID 按字符串比较，避免 Lua double 对 long 的精度损失。

Message 更新/删除、SessionUnitSetting 更新/删除、DeletedRecorder 创建/删除在事件处理时和成功提交后失效对应缓存；所有回源写入通过 Redis Lua 比较版本，防止旧结果长期覆盖新状态。DeletedRecorder 同时失效旧 HashSet 缓存。

如果调用发生在事务型工作单元内，回源结果只用于当前调用，不写入共享读缓存，避免把本事务未提交的设置、消息或删除记录发布给其他读者。常规非事务 GET 的回填不受影响。

热读不加锁。冷窗口用现有 ABP 分布式锁防止同一请求范围并发回源，取得锁后再检查缓存。不同历史游标共用一个 History 槽，可能互相替换，故不保证所有深历史请求都热命中。

Lua 仅使用 Redis 3.2 支持的命令，HSET 每次一组 field/value。没有新增后台任务、进程内全局缓存或 NuGet 依赖。

## 分页和配置

History 使用 `Id < MaxMessageId` 倒序；Latest 使用 `Id > MinMessageId` 升序。游标无需对应一条实际存在的消息；不做 ID 加减一。保留多取一条的 HasMore 判断，NextCursorId 是实际返回最后一条的 ID。

这两个接口不再执行每页 SQL 精确 COUNT。`TotalCount` 保留字段，但表示当前查询数量下界（有下一批时通常为 pageSize + 1），不能用来计算精确总页数。应使用已有 HasMore / NextCursorId。检查到 Flutter 的消息历史加载以返回条数判断是否继续，不使用该总数；其他客户端仍需按各自实现核对。独立的 GetSessionMessageTotalCountAsync 保留精确可见数量能力，未在热分页路径调用。

沿用已有 `MessageOptions` 配置段，仅绑定本次三个配置值，避免影响既有媒体配置：

```json
{
  "MessageOptions": {
    "ReadCacheBatchSize": 128,
    "ReadCacheMaxBatches": 4,
    "ReadCacheExpiration": "00:05:00"
  }
}
```

批次大小约束为 16–512，扫描批次数为 1–16。默认单次数据库窗口查询最多 129 条元数据、最多扫描四个窗口；预算用尽后只补齐本页剩余可见 ID。

## 验证与限制

- Domain、Application、EntityFrameworkCore 和 HttpApi.Host 构建通过。
- Domain 的 24 个定向测试全部通过，包括 22 个新增 Redis 查询用例、1 个已有可见性用例及 1 个已有幂等用例。包含构造器注入及 Token 逐次读取、隔离 Redis 3.2 真正执行 Lua、空窗口、裁剪、迟提交、CAS、回滚、事务内不发布读快照、固定过期、删除失效以及超过 2^53 的 ID。
- EF Core 的 2 个可见性查询测试通过，增加了原始窗口投影及稀疏 ID 的认证边界断言。
- Application 的 10 个新增接口分页测试和已有 2 个权限测试通过，覆盖两个方向、归属检查、空页、满页、多取一条、游标和无 COUNT 查询。
- 500 次本机隔离 Redis 3.2 热缓存读取，128 条元数据及删除状态，平均 2.716 ms/次。此值只涵盖设置/窗口并行读取和批量删除状态读取，不包含 HTTP、鉴权、消息内容加载，也不代表生产并发性能。

数据正确性依赖现有 EF 实体变更事件。直接 SQL、ExecuteUpdate 或其他绕过事件的可见性变更必须显式失效相关读缓存。提交后 Redis 更新失败或进程在数据库提交后退出时，缓存可能暂时滞后直到失效/TTL；本次没有引入额外持久化事件架构。部署需要所有 API/写入实例运行本实现，旧实例不更新这些新增读键。

Redis 中的认证窗口不是长期历史存储；冷窗口、成员设置冷缓存、删除状态冷缓存或内容缓存缺失仍会发生有界数据库访问。
