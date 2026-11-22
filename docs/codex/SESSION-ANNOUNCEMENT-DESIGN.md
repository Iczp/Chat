# 会话公告与通知统一架构设计方案 (Session Announcement & Notice Design)

## 1. 概述与背景

在 GotoIM / Rctea.IM 系统中，**群（Room）** 与 **聊天广场（Square）** 在底层都是 `ChatObject`（`ChatObjectTypeEnums.Room = 2`，`ChatObjectTypeEnums.Square = 5`），且都对应专属唯一的 `Session`（会话），其成员以 `SessionUnit`（会话单元）进行组织与权限控制。

为了避免重复造轮子，本设计将“群通知”、“群公告”及“广场公约/公告”统一收拢为**会话级功能（Session-Level Feature）**：
- **会话公告（SessionAnnouncement）**：持久化置顶资产，支持富文本、置顶、撤下、历史归档与合规确认回执（普通群支持明细，广场支持高并发计数）。
- **会话通知（Session Notice）**：借由聊天流中的系统命令消息（`MessageType.Cmd` + `CmdContentInfo`）下发，驱动聊天流卡片渲染与客户端顶部置顶横幅（Pinned Announcement Bar）的动态刷新。

---

## 2. 领域模型与数据库结构

### 2.1 会话公告主表：`Chat_SessionAnnouncement`

| 字段 | 类型 | 说明 | 约束/索引 |
| :--- | :--- | :--- | :--- |
| `Id` | `Guid` | 主键 | PK |
| `SessionId` | `long` | 所属会话 ID | 核心索引，FK -> `Chat_Session` |
| `ChatObjectId` | `long` | 所属 ChatObject ID (群/广场) | 联合索引 `[ChatObjectId, IsPinned]` |
| `PublisherSessionUnitId` | `Guid` | 发布人在该会话的成员身份 | FK -> `Chat_SessionUnit` |
| `PublisherChatObjectId` | `long` | 发布人的实际用户主体 ID | FK -> `Chat_ChatObject` |
| `Title` | `string` | 公告标题 | `MaxLength(128)`, 必填 |
| `Content` | `string` | 公告正文 (纯文本/Markdown/HTML) | `MaxLength(10000)`, 必填 |
| `EditorType` | `EditorTypes` | 编辑器类型 (Text, Html, Markdown) | 默认 Text |
| `Status` | `AnnouncementStatus`| 状态 (0=Draft, 1=Published, 2=Revoked) | 索引 |
| `IsPinned` | `bool` | 是否在会话顶部置顶常驻 | 联合索引 `[SessionId, IsPinned]` |
| `PinnedTime` | `DateTime?` | 置顶时间 | 可空 |
| `IsPopup` | `bool` | 成员进群是否强提醒弹窗 | 默认 false |
| `RequireConfirm` | `bool` | 是否需要成员确认收到 | 默认 false |
| `ConfirmedCount` | `int` | 已确认人数聚合缓存 | 默认 0 |
| `ReadCount` | `int` | 浏览/已读人数估算 | 默认 0 |
| `LinkedMessageId` | `long?` | 关联的聊天流消息 ID | 可空 |
| `ConcurrencyStamp` | `string` | 乐观锁并发标识 | ABP 标准 |

### 2.2 公告回执明细表：`Chat_SessionAnnouncementReceipt`

| 字段 | 类型 | 说明 | 约束/索引 |
| :--- | :--- | :--- | :--- |
| `Id` | `long` | 自增主键 | PK |
| `AnnouncementId` | `Guid` | 所属公告 ID | 唯一复合索引 `[AnnouncementId, SessionUnitId]` |
| `SessionUnitId` | `Guid` | 确认成员 SessionUnitId | 索引 |
| `ChatObjectId` | `long` | 确认用户 ChatObjectId | 索引 |
| `FirstReadTime` | `DateTime` | 首次查看时间 | 必填 |
| `IsConfirmed` | `bool` | 是否点击了“确认收到” | 默认 false |
| `ConfirmedTime` | `DateTime?` | 确认收到时间 | 可空 |
| `DeviceId` | `string` | 确认时的设备标识 | 可空，`MaxLength(64)` |

---

## 3. 核心业务流程与事务边界

### 3.1 同事务原子提交发布
普通发消息入口 `MessageManager.CreateMessageAsync` 具有 `requiresNew: true` 独立事务特性，无法与公告主表处于同一事务。因此，**扩展现有的 `CreateRoomCommandInCurrentTransactionAsync`**：
1. 开启业务事务性工作单元（`uow: isTransactional = true`）；
2. 插入或更新 `SessionAnnouncement` 主记录；
3. 若勾选 `IsPinned`，原子性将同 `SessionId` 下其他原置顶公告重置为 `IsPinned = false`；
4. 在当前同一事务内，通过 `MessageManager.CreateAnnouncementCommandInCurrentTransactionAsync` 插入系统命令消息（`MessageType.Cmd`）；
5. 事务 Commit 提交；
6. 事务完成后（`uow.OnCompleted`），异步刷新 Redis 缓存 `Chat:SessionPinnedAnnouncement`，并通过 SignalR 广播。

### 3.2 聊天广场（Square）极端性能隔离
- 广场是万人甚至无界成员的公开空间，严禁加载全量成员或逐人记录明细；
- 广场公告强制设置 `RequireConfirm = false`，不写 `SessionAnnouncementReceipt` 明细表；
- 广场阅读数仅在 Redis 中做自增，异步汇总；
- SignalR 推送通过 `Clients.Group("Session_" + sessionId)` 广播，不查在线成员设备映射表。

---

## 4. 权限与多端兼容

- **权限**：在 `SessionPermissionDefinitionConsts` 扩展 `AnnouncementPermission`（Create, Pin, Delete），群主和管理员默认具备，广场仅管理员具备。
- **多端兼容**：系统消息采用 `MessageType.Cmd`（载荷 `CmdContentInfo`），旧版 Flutter / Electron / Web 显示为普通系统通知小灰字，不崩溃；新版客户端识别 `cmd == "SessionAnnouncement"` 渲染为置顶卡片并刷新顶部置顶横幅。
