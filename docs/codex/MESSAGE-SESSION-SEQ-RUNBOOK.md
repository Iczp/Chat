# SessionSeq 维护命令

`IczpNet.Chat.SchemaMigrator` 的 `--backfill-message-session-seq` 是独立、显式的维护入口；普通启动不会执行回填。配置连接的环境变量 `ConnectionStrings__Chat` 与显式目标库名 `ChatMigration__ExpectedDatabase`，不要将连接字符串或密码写入日志或版本库。

执行前必须核实环境与全部写入方，停止 API、Worker、其他实例和外部写入，创建并验证当前备份，确认磁盘及事务日志空间。先执行 `--dry-run`；有非零序号或其他异常时不得开始新任务。

```powershell
dotnet IczpNet.Chat.SchemaMigrator.dll --backfill-message-session-seq --dry-run
dotnet IczpNet.Chat.SchemaMigrator.dll --backfill-message-session-seq --self-test
dotnet IczpNet.Chat.SchemaMigrator.dll --backfill-message-session-seq --apply --source-frozen --batch-size=10000 --journal=C:\maintenance\sessionseq.json
dotnet IczpNet.Chat.SchemaMigrator.dll --backfill-message-session-seq --verify
```

日志文件路径必须为尚不存在的绝对路径，父目录需事先存在。执行日志仅记录数据库名称、数量和非敏感指纹。应用锁限制同库单执行者，但不能阻止不遵守维护窗口的其他应用写入。

回填按全局 Message.Id 升序分批，每批按会话递增编号并在同一事务更新会话计数。超大会话因此不会形成单个超大事务。每批提交后才推进进程内游标；SQL 死锁和超时最多重试三次。崩溃后保持写入暂停，使用同一日志文件和 `--resume` 从头扫描未完成记录：

```powershell
dotnet IczpNet.Chat.SchemaMigrator.dll --backfill-message-session-seq --apply --resume --source-frozen --batch-size=10000 --journal=C:\maintenance\sessionseq.json
```

恢复前程序核对目标库、消息与会话数量、每会话数量指纹、部分消息字段及正文指纹，检查已编号记录是否为严格前缀，且会话计数是否一致。指纹是检测意外变化的辅助检查，不是无碰撞证明；程序的更新语句只更改 Message.SessionSeq 和 Session.LastMessageSeq。不得删除日志文件后将未知非零数据当作可恢复进度。

`--verify` 对全部物理消息检查非正值、孤儿、重复键、按消息 ID 的严格顺序，以及会话计数；失败时输出各类最多 20 个不含正文的示例。无消息会话必须为 0。验收前不能创建唯一索引或恢复写入，旧版本服务也不能连接写入。

本机备份的 `RESTORE VERIFYONLY WITH CHECKSUM` 检查介质和校验和，不等同于完整还原演练。如需回退，先保持所有写入停止，由数据库管理员按备份及后续日志恢复策略还原到独立库验证，再决定替换目标库；不要通过删除消息或回滚已应用的首个 EF 迁移来撤销回填。
