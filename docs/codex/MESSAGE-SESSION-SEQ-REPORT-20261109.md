# SessionSeq 回填执行报告

日期：2026-11-09（Asia/Shanghai）。目标为用户确认的本机开发库 `DESKTOP-VISKG40 / Chat_Module_v3`。API 和 Worker 在回填及索引迁移期间停止，复查时目标库无其他用户会话。

## 备份

停写后建立 `database/backups/Chat_Module_v3_pre_sessionseq_20261109.bak`，大小 31,370,055,680 字节。SQL Server 记录备份时间为 18:33:21–18:48:58，启用了备份校验和；`RESTORE VERIFYONLY WITH CHECKSUM` 返回备份集有效。尚未做完整还原演练。

## 数据与回填

`20261109093244_MessageSessionSeq` 已在迁移历史中；原迁移删除的设备相关七表经用户确认已迁移，无需恢复。`Chat_Message.SessionId` 为可空 uniqueidentifier，SessionSeq 为非空 bigint；Session.Id 为 uniqueidentifier、LastMessageSeq 为非空 bigint。回填采用物理表并覆盖所有记录，消息表和会话表没有启用的触发器。

回填前后 Message 均为 5,594,647 行，Session 均为 3,101,160 行。回填前 5,594,647 条序号为 0，正值、负值及空会话引用为 0；会话计数均为 0。最大会话含 2,480,777 条，物理消息包括 32 条撤回消息和 0 条标记软删除消息。

共提交 560 批、5,594,647 条，耗时 00:06:16，重试 0 次。完成后的非正序号、重复键、孤儿、按 Message.Id 排序异常、会话计数异常均为 0；消息总行数和会话总行数变化为 0。每会话消息数指纹前后均为 1487787811，选定字段与正文指纹均为 -806311958。指纹只是辅助检测，不是无碰撞证明；回填语句仅更新两张表的序号列。程序的批次及验收输出保存在同目录 `MESSAGE-SESSION-SEQ-APPLY-20261109.log`（日志文件受 gitignore 排除）。

## 索引

已应用 `20261109112254_AddMessageSessionSeqUniqueIndex`。实际数据库索引 `IX_Chat_Message_SessionId_SessionSeq` 已启用、唯一、无过滤，键依次为 SessionId、SessionSeq；SessionSeq 非空，默认约束已移除。迁移 SQL 为 `MESSAGE-SESSION-SEQ-UNIQUE-INDEX.sql`。索引执行前再次运行完整只读验收并通过。

## 测试和部署

- SchemaMigrator、Host 和受影响项目构建成功；EF 模型没有相对最新迁移的待处理变更。
- 会话序号仓储与消息可见性共 6 项针对性测试通过。临时表回填自测覆盖多会话、空会话、跨批次、回滚、中断重扫和重复运行。
- SQL Server 临时表测试使用 3 个独立 EF 上下文，跨 2 个会话提交 120 条，验证序号唯一、回滚与重复键拒绝。此测试不等同于实际 MessageManager 全链路压测，未模拟生产代码的死锁和超时重试。
- 使用 `rebuild-chat-api.ps1 -BuildOnly` 和 `rebuild-chat-worker.ps1 -BuildOnly` 构建镜像，再通过 compose 重建并启动两个容器。启动后 API 根路径、Swagger JSON 和 ABP 配置接口返回 HTTP 200。

## 遗留风险与恢复

现有发送事务在提交前仍执行 Redis 缓存操作；回滚时缓存一致性和延长行锁持有时间的风险尚未通过实际业务链路测试解决。未完成实际消息发送的多实例 100 条压测、死锁与超时重试测试，不能将临时表测试视为这些验收项通过。

线上执行仍需独立维护窗口、经验证的当前备份、容量评估与全部写入方停机；不要恢复不认识 SessionSeq 的旧实例。需要恢复时保持停写，先将备份及适用的后续日志还原到独立库验证，再由数据库管理员执行正式恢复；不要回滚已应用的首个迁移或删除消息。
