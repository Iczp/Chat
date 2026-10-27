# Chat 版本数据导入 AppDevice

版本管理的运行时数据保存在独立的 AppDevice 数据库。Chat 的旧版本表仍用于历史数据导入；移除 EF 映射不会删除这些表。本次代码变更未生成或执行数据库 Migration。

1. 备份并确认源 Chat 和目标 AppDevice 数据库。由部署人员先为 AppDevice 创建并应用所需的数据库架构，确认包含 `AppDevice_AppVersion`、`AppDevice_AppVersionDevice`、`AppDevice_AppVersionDeviceGroup` 和 `AppDevice_LegacyLink` 表。
2. 对每个需要迁移的租户先完成现有 `--import-devices` 导入，确保设备和分组的历史 ID 映射已建立。租户数据分别用 `--tenant=<guid>` 指定，宿主数据用 `--host`。
3. 配置环境变量 `ConnectionStrings__Chat`、`ConnectionStrings__AppDevice`、`DeviceImport__ExpectedSourceDatabase`、`AppDevice__ExpectedDatabase` 和 `DeviceImport__DefaultAppId`。连接字符串通过部署环境的安全配置提供。可选设置 `DeviceImport__VersionCheckpointFile`；版本导入使用独立于设备导入的检查点。
4. 先运行只读预检，例如 `dotnet run --project host/IczpNet.Chat.SchemaMigrator -- --import-app-versions --host`。检查输出的版本、设备关系、分组关系数量；租户模式用 `--tenant=<guid>` 替换 `--host`。
5. 停止源 Chat 设备及版本数据的写入，并停止目标 AppDevice 对相关数据的写入；确认实际已冻结后设置 `DeviceImport__SourceFrozen=true` 和 `DeviceImport__TargetFrozen=true`。在相同参数末尾加 `--apply` 执行导入，保持冻结直到切换验证完成。
6. 导入按批提交并写入检查点；中断后使用相同数据库、租户和默认应用参数重跑。每批对已导入记录和关系进行幂等检查；若需要完整重查，使用新的版本检查点路径。目标已存在冲突数据或缺少设备、分组映射时，导入会报错，需要先处理冲突，不能通过覆盖目标数据消除错误。
7. 对比预检数量和目标数据，验证版本查询、灰度分组及设备关系，再安排切换。保留源表和备份，直到业务确认迁移完成。

预检不会写入目标数据库或检查点；它不能代替对实际导入结果和客户端行为的验证。
