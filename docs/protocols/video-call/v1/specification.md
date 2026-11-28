# GotoIM 音视频通话与屏幕共享协议规范（v1 冻结候选稿）

> 适用仓库：`IczpNet.Chat`（领域与接口契约）、`Rctea.IM`（宿主与部署）、`gotoim-electron`（Electron 桌面端）、`gotoim-flutter`（Flutter 跨端）。  
> 本版已收口：SQL 过滤唯一并发锁、群接听条件更新与容量上限、双域 `stateVersion` 与 `active` 对账清理、幂等重签发、`DeviceSessionId` 强绑定、1v1 计时与单方断线终结、`CallRoomLifecycle` 每代房间关闭闭环、LiveKit 内置 TURN 唯一凭证来源、合并转发向后兼容及 `ListRooms` API 校准。

---

## 1. 核心架构与不可变原则

1. **SQL 为通话权威真相源，Redis 仅作加速缓存**：
   - 通话主状态（`CallRecord`）、参与者占线/媒体态（`CallParticipant`）、每代房间生命周期（`CallRoomLifecycle`）、幂等操作凭证（`CallIdempotencyRecord`）、Webhook 去重（`CallWebhookEvent`）均持久化在 SQL Server。
   - 跨通话占线、同群单活与同参与者接听抢占全部由 **SQL 过滤唯一索引 + 条件原子更新** 保证，在多后端实例并发下得出完全一致的结果。
2. **信令面与媒体面分离**：
   - **呼叫信令面**：复用 `ChatHub`（`/signalr-hubs/chat`）+ HTTP REST API（`/api/chat/call/*`），使用**专用分布式事件** `SendCallSignalToClientDistributedEto` 与专用 `SendCallSignalToClientDistributedEventHandler` 定向投递，绝不套用 `SendMessageToClientDistributedEventHandler` 默认广播分支。
   - **媒体传输面**：自建 LiveKit SFU + 内置 TURN；服务端配置 `room.auto_create: false` 并显式创建房间；TURN 中继配置与动态凭证**完全由 LiveKit 在建连握手时统一下发**，Chat 后端仅下发 `rtcUrl`、`token`（TTL `180s`）与 `iceTransportPolicy`。
3. **安全与精度红线**：
   - SignalR 广播中**严禁携带** `token`；普通窗口状态订阅**严禁下发** `token`。
   - 客户端请求中的 `deviceId` 不可信；设备身份与设备会话唯一由服务端从已验证令牌 Claims 提取 `"device_id"` 与 `DeviceSessionTicket.SessionClaim`（`DeviceSessionId`），并经 `ITokenRevocationChecker` + `IChatDeviceSessionValidator` 校验。
   - 大整数 ID（`MessageId`、`SummaryMessageId`、`StateVersion`）跨 JSON/JS 边界统一以 `string` 编解码。

---

## 2. 枚举与 SQL 持久化模型（`IczpNet.Chat`）

### 2.1 消息类型与转发限制
- `src/IczpNet.Chat.Domain.Shared/Enums/MessageTypes.cs`：新增 `Call = 20`（`[Description("音视频通话")]`）。
- **双路径禁转（保持现有历史消息行为兼容）**：
  1. **单条/批量转发（`MessageManager.ForwardAsync`）**：`MessageExtentions.DisabledForwardList` 加入 `MessageTypes.Call`。
  2. **合并转发（`MessageSenderAppService.SendHistoryAsync`）**：**保留当前仅拦截 `Template` 的存量语义，在此基础上精确追加 `MessageTypes.Call`**（不引入 `IsDisabledForward()` 以免误伤存量 `Cmd`/`RedEnvelope`/`Html` 的合并转发）：
     ```csharp
     Assert.If(
         selectedMessageList.Any(x => x.MessageType == MessageTypes.Template || x.MessageType == MessageTypes.Call),
         "Template and Call messages cannot be forwarded in chat history.");
     ```

### 2.2 通话核心枚举与容量常量

- **群房容量上限常量**：`CallConsts.MaxGroupParticipants = 16`（可通过 `LiveKitOptions` 配置但默认冻结为 `16`；`Direct` 模式固定 `MaxParticipants = 2`）。

| 枚举名 | 值定义 | 说明 |
| --- | --- | --- |
| `CallMode` | `1 = Direct`（1v1 单聊）、`2 = Group`（群组多人） | 决定呼叫、重连与终态策略 |
| `CallMediaType` | `1 = Audio`（语音）、`2 = Video`（视频） | 初始媒体模式 |
| `CallSessionState` | `1 = Ringing`（振铃中）、`2 = Connecting`（已接听等待媒体就绪）、`3 = Connected`（媒体通话中）、`4 = Ended`（已结束） | 1v1 需**双方**媒体入房才进 `Connected` |
| `CallParticipantStatus` | `1 = Invited`（振铃中）、`2 = Accepted`（已点接听）、`3 = Connecting`（媒体建连中）、`4 = Connected`（媒体在房中）、`5 = Reconnecting`（异常断线宽限期）、`6 = Rejected`（已拒绝）、`7 = Timeout`（未接超时）、`8 = Left`（已离开）、`9 = Revoked`（安全强制移除） | 严格区分「已接受」「建连中」「媒体已连接」「断线重连中」 |
| `CallEndReason` | `1 = Normal`（正常挂断）、`2 = Canceled`（主叫取消）、`3 = Rejected`（对方拒绝）、`4 = Busy`（对方忙线）、`5 = Timeout`（无人接听超时）、`6 = NetworkError`（网络/建连超时/断线超窗）、`7 = Revoked`（安全撤销终止） | 终态结算原因 |

### 2.3 SQL 持久化实体与并发约束（需生成 EF Core Migration）

#### (1) `CallRecord`（表：`Chat_CallRecord`）
| 字段 | 类型 | 约束/索引 | 说明 |
| --- | --- | --- | --- |
| `Id` | `Guid` | PK | 即 `CallId` |
| `SessionId` | `Guid` | 索引；**过滤唯一索引** `UX_Chat_CallRecord_ActiveGroupSession` ON `(SessionId)` WHERE `CallMode = 2 AND State <> 4` | **DB 级保证同群同时最多 1 个活跃群通话** |
| `CallMode` | `CallMode` | NotNull | `Direct` / `Group` |
| `MediaType` | `CallMediaType` | NotNull | 初始 `Audio` / `Video` |
| `State` | `CallSessionState` | 索引 `(State, NextCheckAt)` | 通话全局状态 |
| `StateVersion` | `long` | ConcurrencyToken | **单调递增版本号**，每次状态或成员变化严格 `+1` |
| `InitiatorUnitId` | `Guid` | NotNull | 发起人 `SessionUnitId` |
| `InitiatorOwnerId` | `long` | NotNull | 发起人 `ChatObjectId` |
| `InitiatorDeviceId` | `string` | MaxLen 128 | 发起人已验证设备 ID |
| `RoomEpoch` | `int` | Default 1 | 当前有效房间代数 |
| `RoomName` | `string` | MaxLen 128 | `gotoim_call_{CallId:N}_v{RoomEpoch}` |
| `LiveKitRoomSid` | `string` | Nullable, MaxLen 64 | 当前代显式 `CreateRoom` 返回的 SID |
| `RingExpireAt` | `DateTime` | NotNull | 振铃超时截止时刻（UTC） |
| `ConnectExpireAt` | `DateTime?` | Nullable | `Connecting` 建连超时截止时刻（如 `+20s`） |
| `LastActiveAt` | `DateTime` | NotNull | 最近活动/心跳时间 |
| `NextCheckAt` | `DateTime?` | 索引 | Worker 下一次到期检查时间 |
| `ConnectedAt` | `DateTime?` | Nullable | **计时起点**：1v1 为双方均达 `Connected` 时刻；Group 为首位被叫达 `Connected` 时刻 |
| `EndedAt` | `DateTime?` | Nullable | 结束时刻 |
| `EndReason` | `CallEndReason?` | Nullable | 结束原因 |
| `DurationSeconds` | `int` | Default 0 | 仅当 `ConnectedAt != null` 时计算 `EndedAt - ConnectedAt`，否则固定为 `0` |
| `HadScreenShare` | `bool` | Default false | 是否发生过屏幕共享 |
| `MaxParticipants` | `int` | Default 0 | 峰值在房人数 |
| `SummaryClientMessageId` | `string` | NotNull, MaxLen 26, Unique | **固定 24 字符幂等消息键**：`"c_" + Base64Url(CallId 16B)`，满足 `MessageManager.cs:369` `<= 26` 字符限制 |
| `SummaryMessageId` | `long?` | Nullable, Unique Filtered Index | 关联已落库 `Message.Id` |
| `SummaryDispatchStatus` | `byte` | `0=None, 1=Pending, 2=Completed` | 同事务 Outbox 标记；Worker 扫描 `State == Ended && SummaryDispatchStatus == Pending` 补偿 |

#### (2) `CallParticipant`（表：`Chat_CallParticipant`）
| 字段 | 类型 | 约束/索引 | 说明 |
| --- | --- | --- | --- |
| `Id` | `Guid` | PK | — |
| `CallId` | `Guid` | 联合唯一 `(CallId, SessionUnitId)` | 所属通话 |
| `SessionUnitId` | `Guid` | 索引 | 参与者会话单元 |
| `OwnerId` | `long` | **过滤唯一索引** `UX_Chat_CallParticipant_ActiveOwner` ON `(OwnerId)` WHERE `IsActiveBusy = 1` | **DB 级保证跨通话占线互斥**：同一 `OwnerId` 任何时刻最多仅 1 条活跃占线记录 |
| `IsActiveBusy` | `bool` | NotNull | 当 `Status` 属于 `Invited(1)/Accepted(2)/Connecting(3)/Connected(4)/Reconnecting(5)` 时置 `1`，终态 `Rejected/Timeout/Left/Revoked` 时置 `0` |
| `Status` | `CallParticipantStatus` | NotNull | 当前参与者状态 |
| `DeviceId` | `string` | Nullable, MaxLen 128 | 绑定已验证 `device_id` |
| `DeviceSessionId` | `string` | Nullable, MaxLen 128 | 绑定已验证设备会话 ID（`refresh-token` / 重放均必须核对） |
| `LiveKitParticipantSid` | `string` | Nullable, MaxLen 64 | 当前媒体连接 SID |
| `RingExpireAt` | `DateTime?` | Nullable | 个人振铃超时时刻 |
| `DisconnectDeadlineAt` | `DateTime?` | Nullable | 异常断线重连宽限截止时刻（断线时刻 `+20s`） |
| `LastHeartbeatAt` | `DateTime?` | Nullable | 最后心跳时刻 |
| `AcceptedAt` / `ConnectedAt` / `LeftAt` | `DateTime?` | Nullable | 状态时间戳 |

#### (3) `CallRoomLifecycle`（表：`Chat_CallRoomLifecycle` — 每代房间持久工作队列）
| 字段 | 类型 | 约束/索引 | 说明 |
| --- | --- | --- | --- |
| `Id` | `Guid` | PK | — |
| `CallId` | `Guid` | 联合唯一 `(CallId, RoomEpoch)` | 所属通话与代数 |
| `RoomEpoch` | `int` | NotNull | 房间代数 |
| `RoomName` | `string` | MaxLen 128 | 房间名 |
| `LiveKitRoomSid` | `string` | Nullable, MaxLen 64 | LiveKit 返回 SID |
| `ActionState` | `byte` | 索引 `(ActionState, NextRetryAt)` | `1=Creating` / `2=Active` / `3=Closing` / `4=Closed` / `5=CreateFailed` |
| `CloseReason` | `string` | Nullable, MaxLen 64 | `CallEnded` / `RoomRotated` / `CreateAborted` |
| `RetryCount` | `int` | Default 0 | `DeleteRoom` 重试次数 |
| `NextRetryAt` | `DateTime?` | Nullable | 退避下次重试时间 |
| `ClosedAt` | `DateTime?` | Nullable | 确认关闭完成时刻 |

#### (4) `CallIdempotencyRecord` & `CallWebhookEvent`
- `CallIdempotencyRecord`：唯一键 `(SessionUnitId, DeviceId, Action, RequestId)`，保留 24 小时。**仅存储业务结果事实（`CallId`、`ResultCode`），绝不缓存含 `token` 的旧响应 JSON。**
- `CallWebhookEvent`：主键 `EventId`（LiveKit Webhook `event.id`），记录 `RoomName`、`RoomSid`、`EventType`、`ProcessedAt`，实现 Webhook 严格去重。

### 2.4 `MessageTypes.Call = 20` 内容快照契约（`CallContentInfo`）
存储于 `Message.ContentJson`（由 `Message.GetContentDto()` 反序列化），结构体与 `MessageTypes.Template` 同级轻量快照：
```json
{
  "callId": "019372a8-1b2c-7d3e-8f4a-5b6c7d8e9f00",
  "callMode": 1,
  "mediaType": 2,
  "endReason": 1,
  "durationSeconds": 185,
  "initiatorUnitId": "3a123456-0000-0000-0000-000000000001",
  "participantUnitIds": [
    "3a123456-0000-0000-0000-000000000001",
    "3a123456-0000-0000-0000-000000000002"
  ],
  "maxParticipants": 2,
  "hadScreenShare": true,
  "startedAt": 1795836000000,
  "endedAt": 1795836185000
}
```
- **旧版客户端兼容**：遇到未知 `MessageType = 20` 时降级展示安全占位 `[当前版本不支持通话记录，请升级]`，不中断 `MessageChange` 增量拉取。

---

## 3. SignalR 瞬态信令契约与双域版本控制

### 3.1 `CommandConsts` 与专用定向路由
新增 8 个瞬态信令常量，经独立的 **`SendCallSignalToClientDistributedEto` + `SendCallSignalToClientDistributedEventHandler`** 投递，严格按 `TargetSessionUnitIds` / `TargetOwnerIds` / `ExcludeDeviceIds` 过滤，绝不走普通消息默认广播分支：

| 常量名 | 指令值 | 所属版本域 | 目标精确范围 |
| --- | --- | --- | --- |
| `CallInvited` | `call.invited@rtc` | `personal` | 仅本次受邀 `TargetUnitIds` 在线设备（排除发起设备） |
| `CallAccepted` | `call.accepted@rtc` | `personal` | 主叫设备 / 已在房成员设备 |
| `CallRejected` | `call.rejected@rtc` | `personal` | 1v1：主叫设备；群聊：在房成员 |
| `CallCanceled` | `call.canceled@rtc` | `personal` | 仍在 `Invited` 状态的被叫设备 |
| `CallHandledElsewhere` | `call.handled@rtc` | `personal` | 同 `OwnerId` 除操作设备外的**其他在线设备** |
| `CallRoomRotated` | `call.room_rotated@rtc` | `personal` | 当前仍合法在房/重连中的绑定设备 |
| `CallEnded` | `call.ended@rtc` | `personal` + `banner` | 本场通话相关所有在线设备 + 群横幅收起 |
| `CallRoomUpdated` | `call.room_updated@rtc` | **`banner`** | 目标 `SessionId` 在线成员（仅刷新群聊顶部横幅） |

### 3.2 同版本多事件乱序与去重规则（解决同版本误丢）
由于同一次群发起操作（如 `stateVersion = "1"`）会同时下发 `call.invited@rtc`（给受邀人）与 `call.room_updated@rtc`（给全群横幅），客户端**严禁**混用同一个全局版本号来互丢事件：
1. **双域隔离游标**：客户端对每个 `callId` 分别维护：
   - `personalStateVersion`（控制个人振铃/接听/通话/换房/结束状态机）；
   - `bannerStateVersion`（控制群聊顶部活跃横幅，仅由 `call.room_updated@rtc`、`call.ended@rtc` 与 `active.sessionGroupCall` 更新）。
   `call.room_updated@rtc` **不得修改** `personalStateVersion`！
2. **同域内版本与去重判定**：
   - `incoming.stateVersion < domainLastVersion` → 严格丢弃（迟到旧状态）；
   - `incoming.stateVersion > domainLastVersion` → 更新本地版本并应用；
   - `incoming.stateVersion == domainLastVersion` → 按 `(callId, command, stateVersion)` 做命令幂等去重；其中若本设备本地对 `callId` 已进入过 `Accepted / Rejected / Handled / Ended`（记录 `dismissedCallIds`），**永不再因任何 `call.invited@rtc` 重新响铃**。

### 3.3 冻结信令 Payload Schema（`CallSignalPayload`）
```json
{
  "callId": "019372a8-1b2c-7d3e-8f4a-5b6c7d8e9f00",
  "sessionId": "3a120000-0000-0000-0000-000000000099",
  "stateVersion": "1",
  "callMode": 2,
  "mediaType": 2,
  "state": 1,
  "roomEpoch": 1,
  "initiatorUnitId": "3a123456-0000-0000-0000-000000000001",
  "initiatorName": "张三",
  "initiatorAvatar": "https://...",
  "actorUnitId": "3a123456-0000-0000-0000-000000000002",
  "targetUnitIds": [
    "3a123456-0000-0000-0000-000000000002"
  ],
  "activeCount": 1,
  "endReason": null,
  "durationSeconds": 0,
  "expireAt": 1795836045000,
  "timestamp": 1795836000000
}
```

---

## 4. HTTP REST API 契约与统一错误码（`CallAppService`）

### 4.1 统一错误码
| 错误码 | HTTP | 触发场景 |
| --- | --- | --- |
| `Call:DeviceNotAuthorized` | `403` | 缺失有效 `device_id`/`device_session` Claim、设备会话已撤销或 `DeviceSessionId` 不匹配 |
| `Call:NotSessionMember` | `403` | 非目标会话成员、已被移出群、被禁言或拉黑 |
| `Call:AlreadyBusy` | `409` | 触发 `UX_Chat_CallParticipant_ActiveOwner` 或校验对端占线 |
| `Call:GroupCallAlreadyActive` | `409` | 触发 `UX_Chat_CallRecord_ActiveGroupSession`（`data.callId` 返回活跃通话 ID） |
| `Call:RoomFull` | `409` | 群通话活跃人数达到上限 `MaxGroupParticipants`（默认 `16`） |
| `Call:AlreadyHandled` | `409` | 1v1 已被他端处理，或群 `accept` 条件更新 `WHERE Status = Invited` 影响行数为 `0` |
| `Call:CallEnded` | `410` | 通话已进入终态 `Ended` |
| `Call:InvalidStateTransition` | `409` | 非法状态跃迁或参与者当前状态不允许操作 |
| `Call:RoomCreateFailed` | `502` | LiveKit 显式 `CreateRoom` 失败 |

### 4.2 端点详细契约

1. **`POST /api/chat/call/invite`**（需传 `requestId`）
   - 发起 1v1、新建群通话，或对已有 `callId` 群通话追加邀请（`inviteMore`）。
   - **SQL 并发控制**：事务内写入 `CallRecord`（触发群单活唯一索引）、写入主叫与被叫 `CallParticipant`（`IsActiveBusy = 1`，触发 `OwnerId` 占线唯一索引）、校验群人数上限 `<= MaxGroupParticipants`、插入 `CallRoomLifecycle(Epoch=1, State=Creating)`；提交后显式调 LiveKit `CreateRoom(roomName, emptyTimeout: 30, maxParticipants)`（`auto_create: false`），成功则更新 `CallRoomLifecycle(Active, RoomSid)`，失败则立即补偿关闭并回退。
   - **幂等重放规则**：命中 `CallIdempotencyRecord` 后，**不返回缓存旧 Token**；重新校验当前设备会话、`DeviceId/DeviceSessionId` 匹配与房间当前 `RoomEpoch`，校验通过后用当前 `RoomName` 现场签发新 180s Token。

2. **`POST /api/chat/call/accept`**（需传 `requestId`）
   - **SQL 原子条件更新（防第二台设备覆盖首次绑定）**：
     - `Direct` 模式：条件更新 `CallRecord`（`State == Ringing` → `Connecting`, `ConnectExpireAt = Now + 20s`, `StateVersion++`）**且**条件更新 `CallParticipant`（`WHERE CallId = @callId AND SessionUnitId = @unitId AND Status = Invited`，置为 `Accepted` 并绑定 `(DeviceId, DeviceSessionId)`）。任一更新影响行数为 `0`，立即抛 `Call:AlreadyHandled (409)`。
     - `Group` 模式：校验 `CallRecord.State != Ended` 与当前容量未超限后，**仅条件更新** `CallParticipant WHERE CallId = @callId AND SessionUnitId = @unitId AND Status = Invited`（置为 `Accepted` 并绑定 `(DeviceId, DeviceSessionId)`，`ConnectExpireAt = Now + 20s`）。若已被同账号另一设备抢先接听，影响行数为 `0`，直接返回 `Call:AlreadyHandled (409)`，**绝不覆盖首次接听的设备绑定**。
   - **幂等重放**：同 `(SessionUnitId, DeviceId, 'accept', requestId)` 重放时，重新校验 `DeviceSessionId`、成员资格与 `CallRecord.State != Ended` 后，基于当前 `RoomEpoch` 重签发新 Token；若期间该通话已结束或设备已失效，返回对应错误码。

3. **`POST /api/chat/call/join`**（需传 `requestId`）
   - 群聊成员点击横幅加入活跃群通话（`CallMode == Group && State != Ended`）：
     - 校验当前群成员资格与当前活跃容量 `< MaxGroupParticipants`；
     - 若该成员已有 `CallParticipant` 行且 `Status IN (Rejected, Timeout, Left)`，通过条件更新将其重新激活（`Status = Accepted`, `IsActiveBusy = 1`, 绑定当前 `DeviceId/DeviceSessionId`）；若无记录则插入新行（受 `UX_Chat_CallParticipant_ActiveOwner` 约束）；若已在其它活跃状态且为不同设备，抛 `Call:AlreadyBusy (409)`。

4. **`POST /api/chat/call/reject` / `cancel` / `hangup`**（需传 `requestId`）
   - `hangup` 为主动挂断：条件更新 `Status = Left, IsActiveBusy = 0`；1v1 直接进入 `Ended(Normal)` 并在同事务登记 `CallRoomLifecycle.ActionState = Closing` 与 `SummaryDispatchStatus = Pending`；群聊若在房与待接（`Invited/Accepted/Connecting/Connected/Reconnecting`）人数归零则同步进入 `Ended(Normal)` 登记关房。

5. **`POST /api/chat/call/heartbeat`**
   - 客户端通话中每 30s 上报：校验 `CallParticipant.DeviceId == currentDeviceId && CallParticipant.DeviceSessionId == currentDeviceSessionId && Status IN (Connecting, Connected, Reconnecting)`，更新 `LastHeartbeatAt` 与 `CallRecord.LastActiveAt`。

6. **`POST /api/chat/call/refresh-token`**
   - **五重资格校验**：
     1. 令牌未撤销 & 设备会话有效；
     2. 仍为 `SessionId` 有效成员；
     3. `CallRecord.State != Ended`；
     4. `CallParticipant.Status IN (Accepted, Connecting, Connected, Reconnecting)`；
     5. **`CallParticipant.DeviceId == currentDeviceId && CallParticipant.DeviceSessionId == currentDeviceSessionId`**（两者必须同时严格匹配）。
   - 校验通过后基于当前 `CallRecord.RoomName`（当前 `RoomEpoch`）签发新 180s Token。

7. **`GET /api/chat/call/active`**（可选 `?sessionId=...`）
   - 传 `sessionId` 时必须校验当前用户持有有效 `SessionUnit`，否则 `403 Call:NotSessionMember`。
   - 返回 `ActiveCallSnapshotDto`（含 `serverTime`、`incomingCalls`、`currentCall`、`sessionGroupCall`，均携带各自域的 `stateVersion` 且不含任何 `token`）。
   - **客户端对账清理硬规则**：
     - 本地处于响铃态但不在 `incomingCalls`（且入铃早于 `serverTime` 安全窗口）→ 立即停铃清理；
     - 本地处于通话中但不在 `currentCall`（或 `currentCall.boundDeviceId != 本机 deviceId`）→ 立即断开本地 LiveKit 连接并退出通话 UI；
     - 当前群聊页展示着横幅但 `sessionGroupCall == null` 或 `callId` 变更 → 立即收起/替换横幅。

8. **`CallJoinResultDto`（`invite` / `accept` / `join` / `refresh-token` 共用响应）**
   ```json
   {
     "callId": "019372a8-1b2c-7d3e-8f4a-5b6c7d8e9f00",
     "sessionId": "guid",
     "stateVersion": "2",
     "callMode": 1,
     "mediaType": 2,
     "roomName": "gotoim_call_019372a81b2c7d3e8f4a5b6c7d8e9f00_v1",
     "roomEpoch": 1,
     "rtcUrl": "ws://10.0.5.20:7880",
     "token": "<LiveKit JWT TTL=180s>",
     "iceTransportPolicy": "all",
     "expireAt": 1795836045000
   }
   ```
   > **唯一 TURN 来源约定**：响应中**不再由 Chat 拼装 `iceServers`**；LiveKit Server 在连接握手时自动下发由其内置 TURN 生成的 STUN/TURN 列表与动态凭据，客户端 SDK 自动使用，Chat 仅通过 `iceTransportPolicy` 控制是否强制 `relay`。  
   > **URL 约定**：`rtcUrl` 仅传基础基址（如 `ws://10.0.5.20:7880` 或经 Caddy 时 `ws://10.0.5.20:8044` / `wss://域名`），**不带 `/rtc`**（JS/Dart SDK 自动追加）。

---

## 5. 房间闭环、断线恢复、安全撤销与 Worker 补偿

### 5.1 LiveKit Webhook 处理（`POST /api/chat/call/livekit-webhook`）
1. 验证 LiveKit Webhook JWT 与 Body SHA-256 哈希；
2. 插入 `CallWebhookEvent(EventId)` 去重；
3. 严格比对 `event.room.name == callRecord.RoomName && event.room.sid == callRecord.LiveKitRoomSid`（不匹配旧代/异房直接忽略）；
4. 事件处理：
   - `participant_joined`：解析 Metadata `sessionUnitId`、`deviceId`，校验该参与者仍合法后置 `Status = Connected`、`LiveKitParticipantSid = participant.sid`、清空 `DisconnectDeadlineAt`。
     - **1v1 计时规则**：仅当 1v1 双方均达到 `Connected`（`Count == 2`）时，`CallRecord.State` 才从 `Connecting` 迁移为 `Connected` 并首次写入 `ConnectedAt`；
     - **群组计时规则**：首位被叫达到 `Connected` 时 `CallRecord` 进入 `Connected` 并写入 `ConnectedAt`。
   - `participant_left`：校验 `LiveKitParticipantSid` 匹配且当前仍为 `Connected`（未主动 `hangup`）：
     - 迁移为 `Reconnecting`，设置 `DisconnectDeadlineAt = Now + 20s`、`CallRecord.NextCheckAt = Min(NextCheckAt, DisconnectDeadlineAt)`。
   - `room_finished`：仅当确认房间已不存在且无可重连者时进入终态结算。

### 5.2 `CallCompensationWorker`（5s 周期，结合 LiveKit `ListRooms` / `ListParticipants`）
由于 LiveKit 不保证 Webhook 100% 送达，Worker 按 SQL 持久化截止时间做最终裁决（统一使用 LiveKit RoomService 官方接口 **`ListRooms(names)`** 与 **`ListParticipants(roomName)`**）：
1. **振铃 / 建连超时**：超过 `RingExpireAt` 或 `ConnectExpireAt` 未连接 → 调 `ListParticipants` 核实未在房 → 置 `Timeout`；1v1 或群无人待接则进入 `Ended`（1v1 未达双方 `Connected` 则 `DurationSeconds = 0`）。
2. **1v1 断线超时强终结规则**：在 1v1 通话中，**只要任一方** `Status == Reconnecting` 超过 `DisconnectDeadlineAt`（且 `ListParticipants` 确认该方不在房内），**无论另一方是否仍在持续上报 `heartbeat`**，立即将整场 1v1 通话终结为 `Ended(NetworkError)`，向另一方广播 `call.ended@rtc` 并登记关房！
3. **心跳失联兜底**：`Connected` 参与者超过 90s 无 `heartbeat` 且 `ListParticipants` / `ListRooms([roomName])` 确认已不在房或房间已销毁，按离开处理并触发终态检测。
4. **每代房间关闭闭环（`CallRoomLifecycle`）**：扫描 `ActionState == Closing && NextRetryAt <= UtcNow`，调用 `DeleteRoom(roomName)`，成功或返回 `NotFound` 时置 `ActionState = Closed, ClosedAt = UtcNow`；失败则退避重试直至确认关闭。
5. **摘要消息 Outbox 补偿**：扫描 `State == Ended && SummaryDispatchStatus == Pending`，使用 `SummaryClientMessageId` 幂等落库 `MessageTypes.Call = 20` 并回写 `SummaryMessageId` 与 `Completed`。

### 5.3 安全强制踢出与房间轮换闭环
触发条件：群成员被移出/退群、或设备会话被撤销/注销（`TokenRevokedEto` / 设备踢出）：
1. 在 SQL 事务中将目标 `CallParticipant` 置为 `Revoked`（`IsActiveBusy = 0`），**拒绝其后续一切 `refresh-token`/`join`**；
2. 若为 **1v1**：立即将 `CallRecord` 置为 `Ended(Revoked)`，登记当前 `CallRoomLifecycle` 为 `Closing` 并销毁房间；
3. 若为 **群通话**：执行 **房间轮换**：
   - 将当前代 `CallRoomLifecycle(RoomEpoch)` 置为 **`Closing`**；
   - `RoomEpoch++` 并生成新 `RoomName`，插入新代 `CallRoomLifecycle(RoomEpoch, Creating)`，显式调用 `CreateRoom` 获取新 `LiveKitRoomSid` 更新为 `Active`；
   - 调用 `RemoveParticipant(oldRoomName, targetIdentity)` 并立即调用 `DeleteRoom(oldRoomName)`，确认关闭后将旧代 `CallRoomLifecycle` 置为 `Closed`（若关旧房失败，由 `CallCompensationWorker` 持续重试直到确认旧房关闭——**旧房未 `Closed` 前不标记该次撤销闭环完成**）；
   - 向其余合法在房设备推送 `call.room_rotated@rtc`（携带新 `stateVersion`），合法设备静默调用 `refresh-token` 领取新房 Token 并迁入新房间。
