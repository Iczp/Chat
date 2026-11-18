# SessionSeq 真实账号并发测试

日期：2026-11-09（Asia/Shanghai）。目标为本地开发环境：Chat API 单容器、SQL Server `Chat_Module_v3`、Redis 和现有 AuthServer。使用现有真实测试账号登录，通过正常 HTTP API 创建测试对象之间的会话并发送消息；凭据和令牌不记录在本报告或测试结果中。

## 隔离与基线

使用该账号名下两个原有 Codex 测试聊天对象（6133、6134）。通过会话申请创建的会话 `D3F290E6-A41B-A217-6546-57D3EA20E685` 只有这两个成员。压力测试前包含 2 条消息，LastMessageSeq 和 MAX(SessionSeq) 均为 2。消息正文均以 `[Codex SessionSeq pressure]` 标识；测试会话、申请及消息保留在本地开发库，未执行删除。

## 测试结果

| 场景 | 请求数 / 并发 | HTTP 成功 | 耗时 | 吞吐 | p50 / p95 / p99 / 最大延迟 |
| --- | --- | --- | --- | --- | --- |
| 单会话、两个测试对象交替发送 | 120 / 6 | 120 | 5.055 s | 23.74 次/s | 202 / 380 / 677 / 814 ms |
| 同一会话、提高并发 | 200 / 16 | 200 | 7.509 s | 26.63 次/s | 551 / 703 / 1116 / 1407 ms |

第一轮的 120 条消息具有 120 个不同 ClientMessageId 和 SessionSeq，序号为 3–122；第二轮的 200 条消息具有 200 个不同 ClientMessageId 和 SessionSeq，序号为 124–323。按全局 Message.Id 排序后检查 SessionSeq，错误数为 0。

两次同时提交同一个发送者和 ClientMessageId 均返回 HTTP 200 与同一个 Message.Id；数据库只有 1 行，序号为 123。测试结束时该会话共 323 条消息，LastMessageSeq = MAX(SessionSeq) = 323。现有 history/latest 接口在 Message.Id 7318198 两侧分别返回 10 条更早/更新消息，游标方向正确。测试后 API 和 Worker 仍在运行，容器重启次数均为 0。原始 HTTP 状态和耗时记录保存在同目录 `SESSIONSEQ-REAL-LOAD-20261109.json` 和 `SESSIONSEQ-REAL-LOAD-ROUND2-20261109.json`。

## 真实测试发现的问题

尝试创建只含两个测试对象的隔离群时，正常群创建流程等待系统消息发送，客户端 60 秒后超时。等待期间 SQL Server 观察到同一 API 容器的两个 EF 连接：创建群的外层事务持锁，内层消息事务执行 UPDATE 时以 `LCK_M_U` 等待该连接；堆栈经过 `RoomManager.CreateAsync` → `MessageManager.CreateMessageAsync` → `SessionRepository.UpdateLastMessageSeqAsync`。请求取消后两个 SQL 会话的打开事务数均为 0，数据库中未保留该名称的群。

根据代码，群创建在发送系统消息前会更新 Redis/连接池，外层事务回滚后可能残留缓存状态；本次没有修改或清理这些缓存。根因是独立发送事务无法读取外层未提交的会话，需结合群创建、提交后事件和缓存一致性单独修复。此问题不能称为 SQL Server 检测到的死锁，实际观察是同请求内的阻塞与客户端超时。

## 覆盖限制

本次测试是真实账号、真实 HTTP 和真实业务消息写入，但只有一个 API 实例和一个会话；不代表多实例、跨会话或更高并发的完整压测。未故意注入发送后插入失败、数据库死锁或 SQL 超时，也未验证失败重试及 Redis/数据库回滚一致性。群创建失败场景已观察到真实阻塞，但不能替代上述注入测试。
