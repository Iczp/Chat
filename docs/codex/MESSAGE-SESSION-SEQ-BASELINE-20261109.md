# Message SessionSeq 回填前只读基线

检查时间：2026-11-09（Asia/Shanghai）。检查期间写入未暂停，因此以下数字是检查时的快照，正式回填前必须重新检查。

- 目标：本机 SQL Server `DESKTOP-VISKG40`，数据库 `Chat_Module_v3`。用户确认该库是本地开发库，并确认原迁移删除的七张设备和版本表已迁移、无需恢复。
- `__EFMigrationsHistory` 中存在 `20261109093244_MessageSessionSeq`。其 `Up` 删除七张已迁移的表并新增 `Chat_Message.SessionSeq`、`Chat_Session.LastMessageSeq`；不可重新执行或回滚该迁移。
- `Chat_Message` 5,594,647 行：SessionSeq 为 0 的 5,594,647 行，大于 0 或小于 0 的均为 0；SessionId 为 NULL 的 0 行，孤儿消息 0 行。
- `Chat_Session` 3,101,160 行，LastMessageSeq 非零的 0 行。
- 最大单会话 2,480,777 条消息；第二大 1,960 条。必须为超大单会话设计可重试的分段回填，不能假设完整会话事务规模可控。
- Message.SessionId 是可空 uniqueidentifier，已有 `IX_Chat_Message_SessionId_Id`；未发现目标唯一复合索引。
- 数据库恢复模式 FULL；数据文件已分配 158,664 MiB、使用 127,309 MiB，日志文件已分配约 103,676 MiB。检查时 D 盘剩余约 54.6 GiB、F 盘约 109.3 GiB。
- 最近完整备份记录为 2026-09-28，压缩后约 29,909 MiB；尚未创建或验证新的回填前备份。
- Chat API 和 Worker 当时均运行并连接此库。维护窗口必须停止所有写入来源，复查活动连接，并在完成所有验收前保持停止。

本次未执行回填、未建立索引、未修改数据库、未更新容器。以上统计不代表完整验收；须重新核实数据、备份、日志空间、消息字段完整性和所有写入来源。
