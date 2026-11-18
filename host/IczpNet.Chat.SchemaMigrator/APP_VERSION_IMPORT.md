# Chat 版本数据导入 AppDevice

版本管理的运行时数据保存在独立的 AppDevice 数据库。Chat 的旧版本表仍用于历史数据导入；移除 EF 映射不会删除这些表。AppDevice 已生成版本相关 Migration，尚须由部署人员检查并通过独立迁移工具执行。

1. 备份并确认源 Chat 和目标 AppDevice 数据库。由部署人员先为 AppDevice 创建并应用所需的数据库架构，确认包含 `AppDevice_AppVersion`、`AppDevice_AppVersionDevice`、`AppDevice_AppVersionDeviceGroup` 、`AppDevice_LegacyLink` 和 `AppDevice_LegacyDataArchive` 表。
2. 先完成现有 `--import-devices` 导入，确保设备和分组的历史 ID 映射已建立。旧 Chat 表没有租户列：每次导入会把整个旧库的数据归到所选的 `--tenant=<guid>` 或 `--host`，不能对同一混合租户源库逐租户重复导入；须先离线确认和拆分数据归属。
3. 配置环境变量 `ConnectionStrings__Chat`、`ConnectionStrings__AppDevice`、`DeviceImport__ExpectedSourceDatabase`、`AppDevice__ExpectedDatabase` 和 `DeviceImport__DefaultAppId`。连接字符串通过部署环境的安全配置提供。可选设置 `DeviceImport__VersionCheckpointFile`；版本导入使用独立于设备导入的检查点。
4. 先运行只读预检，例如 `dotnet run --project host/IczpNet.Chat.SchemaMigrator -- --import-app-versions --host`。检查输出的版本、设备关系、分组关系数量；租户模式用 `--tenant=<guid>` 替换 `--host`。
5. 停止源 Chat 设备及版本数据的写入，并停止目标 AppDevice 对相关数据的写入；确认实际已冻结后设置 `DeviceImport__SourceFrozen=true` 和 `DeviceImport__TargetFrozen=true`。在相同参数末尾加 `--apply` 执行导入，保持冻结直到切换验证完成。
6. 导入按批提交并写入检查点；中断后使用相同数据库、租户和默认应用参数重跑。每批对已导入记录和关系进行幂等检查，并保留版本及关系的审计和扩展属性；若需要完整重查，使用新的版本检查点路径。目标已存在冲突数据或缺少设备、分组映射时，导入会报错，需要先处理冲突，不能通过覆盖目标数据消除错误。
7. 对比预检数量和目标数据，验证版本查询、灰度分组及设备关系，再安排切换。保留源表和备份，直到业务确认迁移完成。

历史记录若同时没有安装包和页面地址，应先由业务确认处理方式。确认需要保留但禁用某条记录时，可在预检和执行时均设置 `DeviceImport__DisableVersionId` 为该记录 ID；导入保留原始归档并将目标记录设为禁用，源数据不变。该选项只接受没有任何更新地址的记录，且绑定到检查点指纹；默认仍拒绝两个地址均为空的记录。

预检不会写入目标数据库或检查点；它不能代替对实际导入结果和客户端行为的验证。

## 历史设备及关系归档

旧设备、分组、用户绑定、分组成员、版本及版本关系记录按来源、原始键、应用与租户归档到 `AppDevice_LegacyDataArchive`，包含历史设备资料、状态及审计与扩展属性。归档不覆盖已存在目标设备或分组的运行时状态；同一源记录重复导入不会生成重复归档。`Kind` 区分 Device、Group、Binding、Member、Version、VersionDevice、VersionGroup；单实体记录的两个键均为原始 ID，关系记录的键分别为原始设备或版本 ID 和用户、设备或分组 ID。版本设备及版本分组关系的审计字段直接写入其目标关系表。

如果在升级代码前已经完成 `--import-devices`，原检查点会显示完成，无法补写这些归档；应保持源及目标相关写入冻结，分别使用新的 `DeviceImport__CheckpointFile` 和 `DeviceImport__VersionCheckpointFile` 对原源库执行设备和版本导入的预检及幂等重放，再核对归档数量。旧源表及备份必须保留至数据核对完成。
