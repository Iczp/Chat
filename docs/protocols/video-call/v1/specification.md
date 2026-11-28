# GotoIM 音视频通话与屏幕共享协议规范（v1 冻结基线）

> 适用仓库：`IczpNet.Chat`（领域与接口契约）、`Rctea.IM`（宿主与部署）、`gotoim-electron`（Electron 桌面端）、`gotoim-flutter`（Flutter 跨端）。  
> 本版已收口全部冻结阻塞项：SQL 行锁串行名额裁决、全员占线释放、`CallRoomLifecycle` 建房/关房/换房闭环、持久化首次/恢复邀请投递、`TryMarkParticipantConnectedAsync` Webhook/Worker 统一入房转换、`heartbeat`/`RoomDisconnected` 主动对账换房、按 `(accountId, scopeKey)` 序号与查询范围收敛的 `active` 对账、以及按 `(callId, inviteRound)` 轮次去重以支持 `inviteMore` 再次邀请。

---

## 1. 核心架构与不可变原则

1. **SQL 为通话权威真相源，Redis 仅作加速缓存**：
   - 通话主状态（`CallRecord`）、参与者占线/建连截止/邀请轮次与媒体态（`CallParticipant`）、每代房间生命周期（`CallRoomLifecycle`）、幂等操作记录（`CallIdempotencyRecord`）、Webhook 去重（`CallWebhookEvent`）均持久化在 SQL Server。
   - 跨通话占线、同群单活由 **SQL 过滤唯一索引** 保证；同一 `CallId` 下的名额校验、参与者占位/释放与 `StateVersion++` 必须在同一 SQL 事务内通过 **`CallRecord` 行级更新锁（`UPDLOCK, ROWLOCK`）串行裁决**。
2. **信令面与媒体面分离**：
   - **呼叫信令面**：复用 `ChatHub`（`/signalr-hubs/chat`）+ HTTP REST API（`/api/chat/call/*`），通过专用分布式事件 `SendCallSignalToClientDistributedEto` + `SendCallSignalToClientDistributedEventHandler` 定向投递，绝不走 `SendMessageToClientDistributedEventHandler` 默认会话广播分支。
   - **媒体传输面**：自建 LiveKit SFU + 内置 TURN；服务端配置 `room.auto_create: false` 并显式创建房间；**仅允许对 `CallRoomLifecycle.ActionState == Active` 的当前 `RoomEpoch` 签发 Token**（TTL `180s`）；内置 TURN 凭据完全由 LiveKit 握手下发。
3. **安全与精度红线**：
   - SignalR 广播与普通窗口状态订阅中**严禁携带** `token`。
   - 客户端请求中的 `deviceId` 不可信；设备身份与会话唯一由服务端从已验证令牌 Claims 提取 `"device_id"` 与 `DeviceSessionTicket.SessionClaim`（`DeviceSessionId`），并经 `ITokenRevocationChecker` + `IChatDeviceSessionValidator` 校验。
   - 大整数 ID（`MessageId`、`SummaryMessageId`、`StateVersion`）跨 JSON/JS 边界统一使用 `string` 编解码。

---

## 2. 枚举与 SQL 持久化模型（`IczpNet.Chat`）

### 2.1 消息类型与转发限制
- `src/IczpNet.Chat.Domain.Shared/Enums/MessageTypes.cs`：新增 `Call = 20`（`[Description("音视频通话")]`）。
- **双路径禁转（保持现有存量消息兼容）**：
  1. **单条/批量转发（`MessageManager.ForwardAsync`）**：`MessageExtentions.DisabledForwardList` 加入 `MessageTypes.Call`。
  2. **合并转发（`MessageSenderAppService.SendHistoryAsync`）**：保留现有 `Template` 拦截语义并精确追加 `MessageTypes.Call`（不改为 `IsDisabledForward()`，避免误伤 `Cmd`/`RedEnvelope`/`Html` 存量合并转发）：
     ```csharp
     Assert.If(
         selectedMessageList.Any(x => x.MessageType == MessageTypes.Template || x.MessageType == MessageTypes.Call),
         "Template and Call messages cannot be forwarded in chat history.");
     ```

### 2.2 通话核心枚举与容量常量
- `CallConsts.MaxGroupParticipants = 16`（群单房间最大占用人数上限，包含所有 `IsActiveBusy == 1` 成员；`Direct` 模式固定 `2`）。

| 枚举名 | 值定义 | 说明 |
| --- | --- | --- |
| `CallMode` | `1 = Direct`（1v1 单聊）、`2 = Group`（群组多人） | 决定呼叫、重连与终态策略 |
| `CallMediaType` | `1 = Audio`（语音）、`2 = Video`（视频） | 初始媒体模式 |
| `CallSessionState` | `1 = Ringing`（振铃中）、`2 = Connecting`（等待媒体就绪）、`3 = Connected`（媒体通话中）、`4 = Ended`（已结束） | 1v1 需**双方**媒体入房才进 `Connected` |
| `CallParticipantStatus` | `1 = Invited`（振铃中）、`2 = Accepted`（已接听）、`3 = Connecting`（建连中）、`4 = Connected`（媒体在房中）、`5 = Reconnecting`（断线宽限期）、`6 = Rejected`（已拒绝）、`7 = Timeout`（未接超时）、`8 = Left`（已离开）、`9 = Revoked`（安全强制移除） | 严格拆分「已接受」「建连中」「媒体已连接」「断线重连中」 |
| `CallEndReason` | `1 = Normal`（正常挂断）、`2 = Canceled`（主叫取消）、`3 = Rejected`（对方拒绝）、`4 = Busy`（对方忙线）、`5 = Timeout`（无人接听超时）、`6 = NetworkError`（网络/建连超时/断线超窗）、`7 = Revoked`（安全撤销终止） | 终态结算原因 |

### 2.3 SQL 持久化实体与并发约束（需生成 EF Core Migration）

#### (1) `CallRecord`（表：`Chat_CallRecord`）
| 字段 | 类型 | 约束/索引 | 说明 |
| --- | --- | --- | --- |
| `Id` | `Guid` | PK | 即 `CallId` |
| `SessionId` | `Guid` | **过滤唯一索引** `UX_Chat_CallRecord_ActiveGroupSession` ON `(SessionId)` WHERE `CallMode = 2 AND State <> 4` | **DB 级保证同群最多 1 个活跃群通话** |
| `CallMode` | `CallMode` | NotNull | `Direct` / `Group` |
| `MediaType` | `CallMediaType` | NotNull | 初始 `Audio` / `Video` |
| `State` | `CallSessionState` | 索引 `(State, NextCheckAt)` | 全局状态 |
| `StateVersion` | `long` | ConcurrencyToken | **单调递增版本号**（每次同 CallId 状态/成员变更严格 `+1`） |
| `InitiatorUnitId` | `Guid` | NotNull | 发起人 `SessionUnitId` |
| `InitiatorOwnerId` | `long` | NotNull | 发起人 `ChatObjectId` |
| `InitiatorDeviceId` | `string` | MaxLen 128 | 发起端已验证设备 ID |
| `RoomEpoch` | `int` | Default 1 | 当前有效房间代数 |
| `RoomName` | `string` | MaxLen 128 | `gotoim_call_{CallId:N}_v{RoomEpoch}` |
| `LiveKitRoomSid` | `string` | Nullable, MaxLen 64 | 当前代已激活房间 SID |
| `BannerNotifyState` | `byte` | Default 0 | `0=None / 1=Pending / 2=Dispatched`（群横幅通知持久化补偿标记） |
| `RingExpireAt` | `DateTime` | NotNull | 1v1 主体振铃超时截止时刻 |
| `LastActiveAt` | `DateTime` | NotNull | 最近活动/心跳时刻 |
| `NextCheckAt` | `DateTime?` | 索引 | Worker 下一次到期检查时间 |
| `ConnectedAt` | `DateTime?` | Nullable | **计时起点**：1v1 为双方均达 `Connected` 时刻；Group 为首位被叫达 `Connected` 时刻 |
| `EndedAt` | `DateTime?` | Nullable | 结束时刻 |
| `EndReason` | `CallEndReason?` | Nullable | 结束原因 |
| `DurationSeconds` | `int` | Default 0 | 仅当 `ConnectedAt != null` 时为 `EndedAt - ConnectedAt`，否则固定 `0` |
| `HadScreenShare` | `bool` | Default false | 是否发生过屏幕共享 |
| `MaxParticipants` | `int` | Default 0 | 峰值在房人数 |
| `SummaryClientMessageId` | `string` | NotNull, MaxLen 26, Unique | **固定 24 字符幂等键**：`"c_" + Base64Url(CallId 16B)`，满足 `MessageManager.cs:369` `<= 26` 字符限制 |
| `SummaryMessageId` | `long?` | Nullable, Unique Filtered Index | 关联已落库 `Message.Id` |
| `SummaryDispatchStatus` | `byte` | `0=None, 1=Pending, 2=Completed` | 同事务 Outbox 标记；Worker 扫描 `State == Ended && SummaryDispatchStatus == Pending` 补偿 |

#### (2) `CallParticipant`（表：`Chat_CallParticipant`）
| 字段 | 类型 | 约束/索引 | 说明 |
| --- | --- | --- | --- |
| `Id` | `Guid` | PK | — |
| `CallId` | `Guid` | 联合唯一 `(CallId, SessionUnitId)` | 所属通话 |
| `SessionUnitId` | `Guid` | 索引 | 参与者会话单元 |
| `OwnerId` | `long` | **过滤唯一索引** `UX_Chat_CallParticipant_ActiveOwner` ON `(OwnerId)` WHERE `IsActiveBusy = 1` | **DB 级保证跨通话占线互斥** |
| `IsActiveBusy` | `bool` | NotNull | `Invited(1)/Accepted(2)/Connecting(3)/Connected(4)/Reconnecting(5)` 时为 `1`（计入群容量占用），终态时置 `0` |
| `Status` | `CallParticipantStatus` | NotNull | 当前参与者状态 |
| `InviteRound` | `int` | Default 0 | **邀请轮次**：首次被邀为 `1`，`inviteMore` 再次邀请已拒绝/超时/离开成员时递增 `+1` |
| `InviteNotifyState` | `byte` | Default 0 | **持久化来电投递标记**：`0=None / 1=Pending / 2=Dispatched`（正常建房与 Worker 恢复建房共用投递） |
| `DeviceId` | `string` | Nullable, MaxLen 128 | 绑定已验证 `device_id` |
| `DeviceSessionId` | `string` | Nullable, MaxLen 128 | 绑定已验证设备会话 ID |
| `LiveKitParticipantSid` | `string` | Nullable, MaxLen 64 | 当前媒体连接 SID |
| `RingExpireAt` | `DateTime?` | Nullable | 个人振铃超时时刻 |
| `ConnectExpireAt` | `DateTime?` | Nullable | **个人独立建连截止时刻**（`accept`/`join` 时单独写入 `+20s`） |
| `DisconnectDeadlineAt` | `DateTime?` | Nullable | 个人异常断线重连宽限截止时刻（断线时刻 `+20s`） |
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
| `RotationNotifyState` | `byte` | Default 0 | `0=None` / `1=Pending`（换房已 `Active` 待补发 `call.room_rotated@rtc`）/ `2=Dispatched`（已投递） |
| `CloseReason` | `string` | Nullable, MaxLen 64 | `CallEnded` / `RoomRotated` / `LateCreateCleanup` |
| `RetryCount` | `int` | Default 0 | 重试次数 |
| `CreateDeadlineAt` | `DateTime?` | Nullable | `Creating` 最迟确认建房时刻（创建 `+15s`） |
| `NextRetryAt` | `DateTime?` | Nullable | 下次重试检查时刻 |
| `ClosedAt` | `DateTime?` | Nullable | 确认关房完成时刻 |

#### (4) `CallIdempotencyRecord` & `CallWebhookEvent`
- `CallIdempotencyRecord`：唯一键 `(SessionUnitId, DeviceId, Action, RequestId)`，保留 24 小时。仅记录 `(CallId, ResultCode)` 等业务事实，**绝不缓存含 `token` 的旧响应**。
- `CallWebhookEvent`：主键 `EventId`（LiveKit `event.id`），实现 Webhook 严格去重。

### 2.4 `MessageTypes.Call = 20` 内容快照契约（`CallContentInfo`）
存储于 `Message.ContentJson`（由 `Message.GetContentDto()` 反序列化）：
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
- **旧版客户端兼容**：遇到未知 `MessageType = 20` 降级展示安全文案 `[当前版本不支持通话记录，请升级]`，不中断 `MessageChange` 增量拉取。

---

## 3. SignalR 瞬态信令契约与客户端对账规则

### 3.1 `CommandConsts` 与专用定向路由
经独立的 `SendCallSignalToClientDistributedEto` + `SendCallSignalToClientDistributedEventHandler` 投递：

| 常量名 | 指令值 | 版本域 | 目标精确范围 |
| --- | --- | --- | --- |
| `CallInvited` | `call.invited@rtc` | `personal` | 仅本次受邀 `TargetUnitIds` 在线设备（排除发起设备） |
| `CallAccepted` | `call.accepted@rtc` | `personal` | 主叫设备 / 已在房成员设备 |
| `CallRejected` | `call.rejected@rtc` | `personal` | 1v1：主叫设备；群聊：在房成员 |
| `CallCanceled` | `call.canceled@rtc` | `personal` | 仍在 `Invited` 状态的被叫设备 |
| `CallHandledElsewhere` | `call.handled@rtc` | `personal` | 同 `OwnerId` 除操作设备外的**其他在线设备** |
| `CallRoomRotated` | `call.room_rotated@rtc` | `personal` | 当前仍合法在房/重连中的绑定设备 |
| `CallEnded` | `call.ended@rtc` | `personal` + `banner` | 本场通话相关所有在线设备 + 群横幅收起 |
| `CallRoomUpdated` | `call.room_updated@rtc` | **`banner`** | 目标 `SessionId` 在线成员（仅刷新群横幅） |

### 3.2 双域版本控制、轮次去重与 `active` 范围对账规则
1. **双域版本号隔离**：
   - 客户端按 `callId` 分别维护 **`personalStateVersion`** 与 **`bannerStateVersion`**。`call.room_updated@rtc` 只推进 `bannerStateVersion`，绝不推进 `personalStateVersion`。
   - 同域内：`incoming.stateVersion < domainLastVersion` 丢弃；`==` 按 `(callId, command, stateVersion, inviteRound)` 去重。
2. **按邀请轮次 `(callId, inviteRound)` 抑制响铃（支持 `inviteMore` 再次邀请）**：
   - 客户端维护已处理的邀请轮次集合 `dismissedInviteKeys`（元素格式 `${callId}:${inviteRound}`），**不再按 `callId` 永久抑制所有后续来电**；
   - 当本机对某轮来电执行过接听/拒绝/收到 `handled` 停铃/结束时，记录 `${callId}:${inviteRound}`；该轮次迟到或重发的 `call.invited@rtc` 永不再响；
   - 若群通话主持人通过 `inviteMore` 对该成员发起新一轮邀请（`inviteRound` 递增为 `2`、`3`...），属于新的 `${callId}:${inviteRound}`，正常弹出振铃。
3. **`GET /api/chat/call/active` 查询范围收敛 + 请求序号淘汰 + `localStateEpoch` 防误杀**：
   - **查询范围（`scopeKey`）严格收敛**：
     - 每次请求按 `(accountId, scopeKey)` 归档，其中 `scopeKey = 'global'`（未传 `sessionId`）或 `scopeKey = 'session:' + sessionId`（明确传入了 `sessionId`）；
     - **未传 `sessionId` 的账号级查询（`'global'`）只对账 `incomingCalls` 与 `currentCall`，严禁清理任何本地 `sessionGroupCall` 横幅！**
     - **仅当请求明确携带 `sessionId = X`** 时，才允许依据该响应对群 `X` 的横幅执行核对与消失清理。
   - **同范围请求乱序淘汰（防旧非空响应覆盖新空快照）**：
     - 对每个 `(accountId, scopeKey)` 维护单调递增序号 `lastIssuedSeq` 与 `lastAppliedSeq`；
     - 发起请求时分配 `reqSeq = ++lastIssuedSeq` 并记录发起水位 `queryStartEpoch = localStateEpoch`；
     - 响应返回时，**若 `reqSeq < lastIssuedSeq`（说明客户端已针对同一 `(accountId, scopeKey)` 发出了更新一轮请求）或 `reqSeq <= lastAppliedSeq`，直接丢弃整份旧响应**；仅当 `reqSeq == lastIssuedSeq` 时置 `lastAppliedSeq = reqSeq` 并应用。
   - **本地变更水位防误杀**：
     - 客户端每次处理 `call.*@rtc` 或本地操作时 `localStateEpoch++` 并写入对应对象的 `updatedAtEpoch`；
     - 对账清理时，**仅允许清理满足 `item.updatedAtEpoch <= queryStartEpoch` 的本地对象**（来电、当前通话与当前查询群的横幅三者统一遵循）。

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
  "inviteRound": 1,
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

## 4. HTTP REST API 完整契约与并发裁决规则（`CallAppService`）

### 4.1 统一错误码
| 错误码 | HTTP | 触发场景 |
| --- | --- | --- |
| `Call:DeviceNotAuthorized` | `403` | 缺失有效 `device_id`/`device_session` Claim、设备会话撤销或 `DeviceSessionId` 不匹配 |
| `Call:NotSessionMember` | `403` | 非目标会话成员、已被移出群、被禁言或拉黑 |
| `Call:AlreadyBusy` | `409` | 触发 `UX_Chat_CallParticipant_ActiveOwner` 或对端占线 |
| `Call:GroupCallAlreadyActive` | `409` | 触发 `UX_Chat_CallRecord_ActiveGroupSession`（`data.callId` 返回活跃通话 ID） |
| `Call:RoomFull` | `409` | 当前 `IsActiveBusy == 1` 占用名额达到上限 `16` |
| `Call:AlreadyHandled` | `409` | 1v1 已被他端处理，或群 `accept` 条件更新 `WHERE Status = Invited` 影响行数为 `0` |
| `Call:CallEnded` | `410` | 通话已进入终态 `Ended` |
| `Call:RoomNotReady` | `409` | 当前 `RoomEpoch` 的 `CallRoomLifecycle.ActionState` 尚未达到 `Active` |
| `Call:InvalidStateTransition` | `409` | 非法状态跃迁或参与者当前状态不允许操作 |
| `Call:RoomCreateFailed` | `502` | LiveKit 显式 `CreateRoom` 失败 |

### 4.2 写操作与查询完整 Request / Response Schema

#### (1) `POST /api/chat/call/invite`
- **请求体（`CallInviteInput`）**：
  ```json
  {
    "requestId": "019372a8-0000-7000-8000-000000000001",
    "sessionId": "3a120000-0000-0000-0000-000000000099",
    "senderSessionUnitId": "3a123456-0000-0000-0000-000000000001",
    "callMode": 2,
    "mediaType": 2,
    "targetSessionUnitIds": ["3a123456-0000-0000-0000-000000000002"],
    "callId": null
  }
  ```
  - **分支 A：首次发起（`callId == null`）**
    1. 幂等查 `CallIdempotencyRecord`；
    2. 校验 `senderSessionUnitId` 归属、会话权限及目标成员有效性（`Direct` 限 1 人；`Group` 含主叫总计 `<= 16` 人）；
    3. 事务内插入 `CallRecord`（`State = Ringing`、`RoomEpoch = 1`、固定 24 字符 `SummaryClientMessageId`、`BannerNotifyState = Group ? Pending : None`）、插入主叫（`Status = Accepted`、`InviteRound = 0`、`InviteNotifyState = None`、`ConnectExpireAt = Now + 20s`、`IsActiveBusy = 1`）与被叫（`Status = Invited`、`InviteRound = 1`、`InviteNotifyState = Pending`、`RingExpireAt = Now + 45s`、`IsActiveBusy = 1`）参与者行、插入 `CallRoomLifecycle(Epoch=1, ActionState=Creating, CreateDeadlineAt=Now+15s)`；
    4. 事务提交后调 LiveKit `CreateRoom(roomName, emptyTimeout: 30, maxParticipants)`：
       - **成功时**：加行锁检查 `CallRecord` 仍为 `State != Ended && RoomEpoch == 1`，置 `ActionState = Active` 并记录 `LiveKitRoomSid`，**随后调用统一的持久化投递方法 `DispatchPendingInvitesAndBannerAsync(callId)`**（投递 `InviteNotifyState == Pending` 的 `call.invited@rtc` 与 `BannerNotifyState == Pending` 的 `call.room_updated@rtc`，并将标记更新为 `Dispatched`），签发 180s Token 返回；**若建房返回时通话已 `Ended` 或换代，立即置 `Closing(LateCreateCleanup)` 并调用 `DeleteRoom`，不发放凭证**；
       - **失败时**：置 `ActionState = CreateFailed`，执行统一整场终结事务释放全体占线并抛 `Call:RoomCreateFailed (502)`。
  - **分支 B：群通话追加邀请（`callId != null`，即 `inviteMore`）**
    1. 仅允许 `callMode == 2 (Group)`，且调用方必须处于本场通话的 `Connected / Connecting / Accepted` 绑定设备；
    2. 事务内对 `CallRecord(Id = callId)` 加 `UPDLOCK, ROWLOCK`：
       - 校验 `State != Ended` 且当前代 `CallRoomLifecycle.ActionState == Active`；
       - 对每个目标成员：若已是活跃态（`IsActiveBusy == 1`）则跳过；若无行或曾处于 `Rejected/Timeout/Left`（且未触发跨通话占线约束），校验新增人数后总占用 `<= 16`（超限抛 `Call:RoomFull`），写入/激活为 `Status = Invited`、`IsActiveBusy = 1`、**`InviteRound = (existing?.InviteRound ?? 0) + 1`**、**`InviteNotifyState = Pending`**、`RingExpireAt = Now + 45s`；置 `CallRecord.BannerNotifyState = Pending`、`StateVersion++`；
    3. 提交后直接调用 **`DispatchPendingInvitesAndBannerAsync(callId)`** 仅对 `InviteNotifyState == Pending` 目标推送带新 `inviteRound` 的 `call.invited@rtc` 与全群 `call.room_updated@rtc`，并返回当前房间的 `CallJoinResultDto`。

#### (2) `POST /api/chat/call/accept`
- **请求体**：`{ "requestId": "string", "callId": "guid", "sessionUnitId": "guid", "inviteRound": 1 }`
- **SQL 串行条件裁决**：
  - 事务内对 `CallRecord` 加 `UPDLOCK, ROWLOCK`：校验 `State != Ended` 与当前代 `ActionState == Active`；
  - **严格绑定邀请轮次与有效期**：`CallParticipant` 条件更新必须同时满足 `WHERE CallId = @c AND SessionUnitId = @u AND Status = Invited AND InviteRound = @inviteRound AND (RingExpireAt IS NULL OR RingExpireAt > @utcNow)`（1v1 同步核对 `CallRecord.RingExpireAt > @utcNow`）。第 1 轮迟到请求在第 2 轮发出后到达时，因轮次不匹配影响行数为 `0`，抛 `Call:AlreadyHandled (409)`，第 2 轮仍维持 `Invited` 可正常接听。
  - `Direct`：同时条件更新 `CallRecord(Ringing -> Connecting, StateVersion++)` 与上述 `CallParticipant` 条件（置为 `Accepted`、绑定 `(DeviceId, DeviceSessionId)`、写入 `ConnectExpireAt = Now + 20s`）；任一影响行数 `0` 抛 `Call:AlreadyHandled (409)`。
  - `Group`：按上述 `CallParticipant` 条件更新为 `Accepted`、绑定 `(DeviceId, DeviceSessionId)`、写入个人 `ConnectExpireAt = Now + 20s`、`CallRecord.StateVersion++`。影响行数为 `0` 抛 `Call:AlreadyHandled (409)`。
  - 幂等重放时重新校验资格（含 `InviteRound` 未被新一轮覆盖）与 `ActionState == Active` 后现场重签新 180s Token。

#### (3) `POST /api/chat/call/join`
- **请求体**：`{ "requestId": "string", "callId": "guid", "sessionUnitId": "guid" }`
- 事务内对 `CallRecord` 加 `UPDLOCK, ROWLOCK`：校验 `Group`、`State != Ended`、`ActionState == Active`、群成员资格、容量 `< 16`；若已有 `Rejected/Timeout/Left` 行则激活置 `Accepted`、`IsActiveBusy = 1`、绑定 `(DeviceId, DeviceSessionId)` 与个人 `ConnectExpireAt = Now + 20s`；无行则插入；若已在活跃态且为他端设备则抛 `Call:AlreadyBusy (409)`。

#### (4) `POST /api/chat/call/reject` / `cancel` / `hangup`
- **`reject` 请求体**：`{ "requestId": "string", "callId": "guid", "sessionUnitId": "guid", "inviteRound": 1, "reason": "optional" }`
  - 条件更新同样严格校验 `WHERE CallId = @c AND SessionUnitId = @u AND Status = Invited AND InviteRound = @inviteRound AND (RingExpireAt IS NULL OR RingExpireAt > @utcNow)`，防止第 1 轮迟到拒接误拒第 2 轮新邀请（影响行数 `0` 抛 `Call:AlreadyHandled (409)`）。
- **`cancel` / `hangup` 请求体**：`{ "requestId": "string", "callId": "guid", "sessionUnitId": "guid", "reason": "optional" }`
- **统一终结事务铁律（`SettleCallEndedInTransaction`）**：
  - 事务内加 `UPDLOCK, ROWLOCK`，只要导致整场通话结束，**同事务批量将 `CallId` 下所有 `IsActiveBusy == 1` 参与者置为终态并统一释放 `IsActiveBusy = 0`**，置 `CallRecord.State = Ended`、`SummaryDispatchStatus = Pending`，并将活跃/建房中 `CallRoomLifecycle` 置 `Closing`。

#### (5) `POST /api/chat/call/heartbeat` & `POST /api/chat/call/refresh-token`
- **请求体**：`{ "callId": "guid", "sessionUnitId": "guid" }`
- **`heartbeat` 响应 Schema**：
  ```json
  {
    "serverTime": 1795836030000,
    "stateVersion": "4",
    "roomEpoch": 2,
    "state": 3
  }
  ```
  - 若客户端检测到响应中的 `roomEpoch > localRoomEpoch`（即使此前丢失了 `call.room_rotated@rtc`），**立即主动调用 `refresh-token` 并切至新代房间**；若 `state == 4 (Ended)` 立即结束通话。
- **`refresh-token`**：满足令牌/设备会话有效、群成员有效、`State != Ended`、当前代 `ActionState == Active`、`Status IN (Accepted, Connecting, Connected, Reconnecting)`、且 **`DeviceId == currentDeviceId && DeviceSessionId == currentDeviceSessionId`** 时返回当前代 `CallJoinResultDto`。

#### (6) `GET /api/chat/call/active`（可选 `?sessionId=guid`）
- 传 `sessionId` 时校验当前用户为该会话有效成员，否则 `403 Call:NotSessionMember`。
- **完整响应 Schema（`ActiveCallSnapshotDto`）**：
  ```json
  {
    "serverTime": 1795836000000,
    "incomingCalls": [
      {
        "callId": "019372a8-1b2c-7d3e-8f4a-5b6c7d8e9f00",
        "sessionId": "3a120000-0000-0000-0000-000000000099",
        "stateVersion": "1",
        "inviteRound": 1,
        "callMode": 1,
        "mediaType": 2,
        "state": 1,
        "initiatorUnitId": "3a123456-0000-0000-0000-000000000001",
        "initiatorName": "张三",
        "initiatorAvatar": "https://...",
        "mySessionUnitId": "3a123456-0000-0000-0000-000000000002",
        "expireAt": 1795836045000
      }
    ],
    "currentCall": {
      "callId": "019372a8-1b2c-7d3e-8f4a-5b6c7d8e9f00",
      "sessionId": "3a120000-0000-0000-0000-000000000099",
      "stateVersion": "4",
      "callMode": 2,
      "mediaType": 2,
      "state": 3,
      "roomEpoch": 1,
      "mySessionUnitId": "3a123456-0000-0000-0000-000000000002",
      "boundDeviceId": "win-device-01",
      "activeCount": 3
    },
    "sessionGroupCall": {
      "callId": "019372a8-1b2c-7d3e-8f4a-5b6c7d8e9f00",
      "sessionId": "3a120000-0000-0000-0000-000000000099",
      "stateVersion": "4",
      "mediaType": 2,
      "activeCount": 3,
      "initiatorName": "张三",
      "participantPreviews": [
        { "sessionUnitId": "3a123456-0000-0000-0000-000000000001", "displayName": "张三", "avatar": "https://..." }
      ]
    }
  }
  ```
- **空值语义**：`incomingCalls` 无来电为空数组 `[]`；`currentCall` 无正在参与通话为 `null`；`sessionGroupCall` 在未传 `sessionId` 或目标群无活跃通话时为 `null`（客户端须遵守第 3.2 节范围对账规则，未传 `sessionId` 禁止清理群横幅）。

#### (7) `CallJoinResultDto`（`invite` / `accept` / `join` / `refresh-token` 响应）
```json
{
  "callId": "019372a8-1b2c-7d3e-8f4a-5b6c7d8e9f00",
  "sessionId": "3a120000-0000-0000-0000-000000000099",
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
> `iceServers` 由 LiveKit 内置 TURN 握手自动下发；`rtcUrl` 仅传基础基址（不带 `/rtc`）。

---

## 5. 房间生命周期、统一入房转换与 Worker 补偿闭环

### 5.1 统一入房转换方法（`TryMarkParticipantConnectedAsync`）
**Webhook（`participant_joined`）与 `CallCompensationWorker`（巡检发现已在房）共用同一条领域方法**，在 `CallRecord` 行锁事务内执行：
1. 校验 `CallRecord.State != Ended`、`CallRecord.RoomEpoch == roomEpoch && CallRecord.LiveKitRoomSid == roomSid`；
2. 校验 `CallParticipant` 存在且 `Status IN (Accepted, Connecting, Connected, Reconnecting)`（禁止复活 `Rejected/Timeout/Left/Revoked`）；
3. 校验 `CallParticipant.DeviceId == matchedDeviceId`；
4. 满足后更新：`Status = Connected`、`IsActiveBusy = 1`、`LiveKitParticipantSid = participantSid`、清空 `ConnectExpireAt` 与 `DisconnectDeadlineAt`、更新 `MaxParticipants`；
5. **计时起点推进**：
   - `Direct`：当且仅当 **双方均已达 `Connected`（`Count == 2`）** 时，迁移 `CallRecord.State = Connected` 并首次写入 `ConnectedAt`；
   - `Group`：首位被叫达 `Connected` 时迁移 `CallRecord.State = Connected` 并写入 `ConnectedAt`。

### 5.2 LiveKit Webhook 处理（`POST /api/chat/call/livekit-webhook`）
1. 验证 JWT + Body SHA-256，插入 `CallWebhookEvent(EventId)` 去重；
2. 比对 `event.room.name == callRecord.RoomName && event.room.sid == callRecord.LiveKitRoomSid`，不匹配忽略；
3. `participant_joined` → 调用 `TryMarkParticipantConnectedAsync(...)`；
4. `participant_left` → SID 匹配且当前 `Status == Connected` 时转 `Reconnecting`、`DisconnectDeadlineAt = Now + 20s`；
5. `room_finished` → 无待接/重连中成员时执行 `SettleCallEndedInTransaction`。

### 5.3 `CallCompensationWorker`（5s 周期，官方 API `ListRooms` + `ListParticipants`）
1. **建房恢复（`ActionState == Creating && CreateDeadlineAt <= UtcNow`）**：
   - 调 `ListRooms([roomName])`：
     - 房间已存在：若 `CallRecord.State != Ended && CallRecord.RoomEpoch == lifecycle.RoomEpoch`，回填 `LiveKitRoomSid` 并置 `Active` → **立即调用 `DispatchPendingInvitesAndBannerAsync(callId)` 补发未投递的 `call.invited@rtc` / `call.room_updated@rtc`（若为轮换新房且 `RotationNotifyState == Pending`，同步补发 `call.room_rotated@rtc`）**；若已 `Ended` 或已换代，转 `Closing(LateCreateCleanup)`。
     - 房间不存在：第 1 代建房超时置 `CreateFailed` 并调用 `SettleCallEndedInTransaction(NetworkError)`；轮换新代失败终止通话并关闭旧房。
2. **首次邀请 / 横幅漏发补发**：扫描 `State != Ended` 且当前代 `ActionState == Active` 下仍存在 `CallParticipant.InviteNotifyState == Pending`（未超 `RingExpireAt`）或 `CallRecord.BannerNotifyState == Pending` 的记录，调用 `DispatchPendingInvitesAndBannerAsync(callId)`。
3. **建连检查与防漏计时（`Accepted/Connecting` 超 `ConnectExpireAt`）**：
   - 调 `ListParticipants(roomName)` 核实该参与者状态：
     - **媒体就绪硬条件**：仅当匹配参与者的 LiveKit **`ParticipantInfo.State == ACTIVE`**（严格排除仍处于信令握手/未通媒体的 `JOINING` 与 `JOINED`）时，说明 `participant_joined` Webhook 丢失但媒体实际已就绪：**立即调用 `TryMarkParticipantConnectedAsync(...)` 补记为 `Connected` 并推进计时**！
     - **若不在房或仍为 `JOINING / JOINED`（信令已连但媒体未通且已超 `ConnectExpireAt`）**：不得提前计时，1v1 立即执行 `SettleCallEndedInTransaction(NetworkError)`（`DurationSeconds = 0`）；群通话置 `Timeout`(`IsActiveBusy = 0`)。
4. **断线宽限到期（`Reconnecting` 超 `DisconnectDeadlineAt`）**：
   - 调 `ListParticipants(roomName)` 核实：仅当匹配参与者 **`ParticipantInfo.State == ACTIVE`** 时视为已重连成功 → 调用 `TryMarkParticipantConnectedAsync(...)` 恢复；若不在房或仅 `JOINING / JOINED` → **1v1 只要任一方超窗（即使另一方持续心跳）立即 `SettleCallEndedInTransaction(NetworkError)`**；群通话置 `Left`(`IsActiveBusy = 0`)，人数归零则结算。
5. **心跳失联与振铃超时**：`Invited` 超 `RingExpireAt` 置 `Timeout`（1v1 整场结束）；`Connected` 超 90s 无心跳且 `ListParticipants` 不在房按离开处理。
6. **关房重试（`ActionState == Closing`）**：退避调用 `DeleteRoom(roomName)` 至成功或 `NotFound` 后置 `Closed`。
7. **换房通知补偿（`ActionState == Active && RotationNotifyState == Pending`）**：补发 `call.room_rotated@rtc` 置 `Dispatched`。
8. **摘要消息 Outbox 补偿（`State == Ended && SummaryDispatchStatus == Pending`）**：以 24 字符 `SummaryClientMessageId` 幂等落库 `MessageTypes.Call = 20`。

### 5.4 安全强制踢出、房间轮换与客户端被动对账兜底
1. **服务端**：群成员移出或设备会话撤销时，置目标 `Revoked`（`IsActiveBusy = 0`）；1v1 结算终结；群通话旧代置 `Closing`、`RoomEpoch++` 建新房并置 `Active` + `RotationNotifyState = Pending`，投递 `call.room_rotated@rtc` 并重试 `DeleteRoom(oldRoomName)` 直至旧代确认 `Closed`。
2. **客户端被动兜底（防 `call.room_rotated@rtc` 丢包）**：
   - 客户端在收到 LiveKit **`RoomDisconnected`**（非本端主动挂断）或 **`heartbeat` 响应发现 `roomEpoch > localRoomEpoch`** 时，主动发起 `GET /api/chat/call/active` 对账：若 `currentCall` 仍有效且 `boundDeviceId == 本机 deviceId`（且 `roomEpoch` 推进），立即调 `refresh-token` 领取新代 Token 切入新房间；若 `currentCall == null` 或设备不匹配，立刻清理退出。

---

## 6. 跨端平台支持矩阵（V1 冻结范围）

| 平台 | 前台来电振铃 | 1v1 / 群组音视频 | 发起屏幕共享 | 观看屏幕共享 | 后台 / 杀进程锁屏来电 |
| --- | --- | --- | --- | --- | --- |
| **Electron (Windows / macOS)** | ✅ V1 必选 | ✅ V1 必选 | ✅ V1 必选（`getDisplayMedia` + `setDisplayMediaRequestHandler`） | ✅ V1 必选 | ❌ 仅桌面端运行态（托盘保活） |
| **Flutter Windows / macOS** | ✅ V1 必选 | ✅ V1 必选 | ✅ V1 必选（桌面端屏幕/窗口选择） | ✅ V1 必选 | ❌ 仅桌面端运行态 |
| **Flutter Android** | ✅ V1 必选 | ✅ V1 必选 | ✅ V1 必选（`MediaProjection` 前台服务） | ✅ V1 必选 | ⏳ **V1.1**（需补齐 FCM/厂商推送 + Telecom ConnectionService） |
| **Flutter iOS** | ✅ V1 必选 | ✅ V1 必选 | ⏳ **V1.1**（需补齐 ReplayKit `BroadcastUploadExtension`） | ✅ V1 必选 | ⏳ **V1.1**（需补齐 APNs VoIP PushKit + CallKit） |
| **Flutter Web** | ❌ 不支持 | ❌ 不支持 | ❌ 不支持 | ❌ 不支持 | ❌（`signalr_gateway_factory.dart` 当前走 `stub` 无实时长连接） |
