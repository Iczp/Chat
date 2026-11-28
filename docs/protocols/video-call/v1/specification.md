# GotoIM 音视频通话与屏幕共享协议规范（v1）

> 适用仓库：`IczpNet.Chat`（领域与接口契约）、`Rctea.IM`（宿主与部署）、`gotoim-electron`（Electron 桌面端）、`gotoim-flutter`（Flutter 跨端）。
> 状态：V1 冻结基线（自建 LiveKit + TURN，支持 1v1 音视频、群组多人通话、屏幕共享，`MessageTypes.Call = 20`）。

---

## 1. 核心架构约定

1. **信令与媒体分离**：
   - **呼叫信令面**：复用现有 SignalR `ChatHub`（`/signalr-hubs/chat`）与 HTTP REST API（`/api/chat/call/*`），经 Redis DistributedEventBus 推送。
   - **媒体传输面**：采用自建 LiveKit SFU + TURN；客户端在授权通过后通过 HTTP API 领取短期 `livekitToken` 并建立媒体连接。
2. **安全红线**：
   - **SignalR 广播中禁止下发 `livekitToken`**，所有房间 Token 仅通过鉴权后的 `invite` / `accept` / `join` / `refresh-token` HTTP 接口返回给当前合法设备。
   - 所有大整数 ID（`MessageId`、`ChatObjectId` 如有大数序列化）在跨 JSON/JS 边界时使用 `string` 传输/解析，避免精度丢失。
3. **跨端兼容原则**：
   - 新增持久化消息枚举 `MessageTypes.Call = 20`。
   - 瞬态通话指令沿用 `CommandConsts`（`*@rtc`）与 `SendMessageToClientDistributedEto`（`useOutbox: false`），不写消息表；仅在通话结束结算时幂等写入 1 条持久化 `Message`。

---

## 2. 枚举与数据模型契约

### 2.1 消息类型枚举（`MessageTypes`）

文件：`src/IczpNet.Chat.Domain.Shared/Enums/MessageTypes.cs`

```csharp
/// <summary>
/// 音视频通话记录
/// </summary>
[Description("音视频通话")]
Call = 20,
```

### 2.2 通话核心枚举

| 枚举名 | 值定义 | 说明 |
| --- | --- | --- |
| `CallMode` | `1 = Direct`（1v1 单聊）、`2 = Group`（群组多人） | 决定呼叫控制与结束条件 |
| `CallMediaType` | `1 = Audio`（语音）、`2 = Video`（视频） | 发起呼叫时的初始媒体模式（通话中可动态开关摄像头或共享屏幕） |
| `CallEndReason` | `1 = Normal`（正常挂断）、`2 = Canceled`（主叫取消）、`3 = Rejected`（对方拒绝）、`4 = Busy`（对方忙线）、`5 = Timeout`（无人接听超时）、`6 = NetworkError`（网络/房间异常结束） | 用于服务端结算与双端消息文案展示 |
| `CallSessionState` | `1 = Ringing`（振铃中）、`2 = Connecting`（已接听建连中）、`3 = Connected`（通话中）、`4 = Ended`（已结束） | Redis 会话状态 |
| `CallParticipantStatus` | `1 = Invited`（已邀请/振铃中）、`2 = Joined`（在房中）、`3 = Rejected`（已拒绝）、`4 = Timeout`（未接超时）、`5 = Left`（已离开） | 群组多人通话每名成员独立状态 |

### 2.3 通话消息内容契约（`CallContentInfo`）

`Message.MessageType == MessageTypes.Call (20)` 时，消息体 JSON 结构如下：

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

- **转发限制**：`MessageExtentions.DisabledForwardList` 必须包含 `MessageTypes.Call`（通话记录不允许转发）。
- **幂等约束**：每场通话 `callId` 仅允许生成 **1 条** 持久化 `Message`（建议以 `ClientMessageId = $"call:{callId}"` 或 `CallRecord.MessageId` 唯一约束保证幂等）。

---

## 3. SignalR 瞬态信令契约（`CommandConsts`）

文件：`src/IczpNet.Chat.Domain.Shared/Commands/CommandConsts.cs`

| 常量名 | 指令值 (`Command`) | 接收范围 | 说明 |
| --- | --- | --- | --- |
| `CallInvited` | `call.invited@rtc` | 被叫指定 `SessionUnit` 的所有在线设备（排除主叫发起设备） | 来电振铃通知 |
| `CallAccepted` | `call.accepted@rtc` | 会话内主叫发起设备 / 在房成员 | 对方已接听 |
| `CallRejected` | `call.rejected@rtc` | 1v1：主叫发起设备；群聊：在房成员 | 某被叫拒绝 |
| `CallCanceled` | `call.canceled@rtc` | 被叫所有振铃设备 | 主叫在接通前取消 |
| `CallHandledElsewhere` | `call.handled@rtc` | 执行 `Accept/Reject` 的同一 `OwnerId` 的**其他在线设备** | 同账号多端协同：其余设备立即停止响铃 |
| `CallRoomUpdated` | `call.room_updated@rtc` | 目标 `SessionId` 下所有在线成员 | 群通话人数变化 / 横幅刷新（`activeCount == 0` 时收起横幅） |
| `CallEnded` | `call.ended@rtc` | 房间相关所有在线设备 | 整场通话结束通知 |

### 信令 Payload 结构（`CallSignalPayload`）

```json
{
  "callId": "019372a8-1b2c-7d3e-8f4a-5b6c7d8e9f00",
  "sessionId": "3a120000-0000-0000-0000-000000000099",
  "callMode": 2,
  "mediaType": 2,
  "state": 1,
  "initiatorUnitId": "3a123456-0000-0000-0000-000000000001",
  "initiatorName": "张三",
  "initiatorAvatar": "https://...",
  "actorUnitId": "3a123456-0000-0000-0000-000000000002",
  "actorDeviceId": "win-device-01",
  "targetUnitIds": [
    "3a123456-0000-0000-0000-000000000002",
    "3a123456-0000-0000-0000-000000000003"
  ],
  "activeCount": 2,
  "endReason": null,
  "durationSeconds": 0,
  "expireAt": 1795836045000,
  "timestamp": 1795836000000
}
```

> 注：利用 `CommandPayload.Scopes` 与现有 `SendMessageToClientDistributedEto` 路由模型时，需支持“按指定 `TargetSessionUnitIds` + 可选排除 `IgnoreDeviceId` / `IgnoreConnections`”精准推送，确保同账号多端不互相误触发或漏停铃。

---

## 4. HTTP REST API 契约（`CallAppService`）

路由前缀：`/api/chat/call`（基于 ABP ConventionalControllers 自动暴露，需登录态且校验当前 `CurrentUser` 对 `senderSessionUnitId` 的归属权）。

### 4.1 `POST /api/chat/call/invite`
发起 1v1 通话、新建群通话，或向已有群通话追加邀请成员。
- **请求**：
  ```json
  {
    "sessionId": "guid",
    "senderSessionUnitId": "guid",
    "callMode": 1,
    "mediaType": 2,
    "targetSessionUnitIds": ["guid"],
    "callId": null,
    "deviceId": "string"
  }
  ```
- **核心校验**：
  - 校验 `senderSessionUnitId` 属于当前登录用户且隶属 `sessionId`，未被禁言/拉黑/移出会话。
  - 若 `callMode == 1 (Direct)`：`targetSessionUnitIds` 必须恰有 1 名对端成员；校验主叫与被叫均未占线（`call:busy:{chatObjectId}`）。若被叫占线，按约定直接返回错误码或写入“对方忙线”记录。
  - 若 `callMode == 2 (Group)` 且 `callId == null`：使用 Redis `SET NX` 锁定 `call:group:session:{sessionId}`；若该群已存在活跃通话，返回业务冲突或引导客户端调用 `join`。
- **响应（`CallJoinResultDto`）**：
  ```json
  {
    "callId": "string",
    "roomName": "gotoim_call_019372a81b2c7d3e",
    "rtcUrl": "ws://10.0.5.20:7880",
    "token": "<LiveKit JWT>",
    "iceServers": [
      { "urls": ["stun:10.0.5.20:3478"] },
      { "urls": ["turn:10.0.5.20:3478?transport=udp", "turn:10.0.5.20:3478?transport=tcp"], "username": "...", "credential": "..." }
    ],
    "iceTransportPolicy": "all",
    "callMode": 1,
    "mediaType": 2,
    "expireAt": 1795836045000
  }
  ```

### 4.2 `POST /api/chat/call/accept`
被叫接听通话并领取房间 Token。
- **请求**：`{ "callId": "string", "sessionUnitId": "guid", "deviceId": "string" }`
- **原子抢占规则**：
  - `Direct` 模式：Redis Lua 脚本校验 `State == Ringing` 且操作方为被叫，原子迁移至 `Connecting` 并记录 `AcceptedDeviceId`，广播 `call.accepted@rtc` 给主叫，广播 `call.handled@rtc` 给被叫其余设备。若已被其他设备接听/取消，返回明确业务错误 `CallAlreadyHandled`。
  - `Group` 模式：原子校验参与者记录后将其标记为 `Joined`，若同账号其他设备也在振铃，向其他设备发送 `call.handled@rtc` 停铃；同时发送 `call.room_updated@rtc` 更新群横幅人数。
- **响应**：同 `CallJoinResultDto`。

### 4.3 `POST /api/chat/call/join`
群组通话中，群成员点击顶部横幅“加入通话”。
- **请求**：`{ "callId": "string", "sessionUnitId": "guid", "deviceId": "string" }`
- **校验**：确认该用户是 `sessionId` 的有效成员、当前无其它通话占线、房间仍活跃。
- **响应**：同 `CallJoinResultDto`。

### 4.4 `POST /api/chat/call/reject` / `cancel` / `hangup`
- `reject`（被叫拒绝）：`{ "callId": "string", "sessionUnitId": "guid", "deviceId": "string", "reason": "rejected" | "busy" }`
  - `Direct` 模式：立即结算通话为 `Rejected`（或 `Busy`），广播 `call.rejected@rtc` + `call.handled@rtc` + `call.ended@rtc`，写入 `MessageTypes.Call = 20`。
  - `Group` 模式：仅将该成员标记为 `Rejected` 并对同账号其他设备停铃；若房间无人在房且所有被叫均已拒绝，则结束房间。
- `cancel`（主叫接通前取消）：`{ "callId": "string", "sessionUnitId": "guid" }`
  - `Direct` 模式：结算为 `Canceled`，通知被叫所有设备停铃并写消息。
  - `Group` 模式：若房间内无其他已加入成员，结束房间并结算为 `Canceled`。
- `hangup`（通话中离开/挂断）：`{ "callId": "string", "sessionUnitId": "guid", "deviceId": "string" }`
  - `Direct` 模式：任一方挂断即触发整场结束结算（`Normal`）。
  - `Group` 模式：仅当前成员退出并释放其 `call:busy` 锁，通知 LiveKit 移除或等待其断连；当在房人数归 `0` 且无振铃待接人员时结算结束。

### 4.5 `GET /api/chat/call/active`
用于客户端冷启动、SignalR 断线重连后恢复来电状态与群聊活跃横幅。
- **参数**：`sessionId`（可选，查询指定群聊活跃横幅）
- **返回**：当前用户待处理的来电列表 + 当前正在参与的通话 + 指定群的活跃通话摘要。

### 4.6 `POST /api/chat/call/refresh-token`
通话时长超过 JWT TTL（默认 10 分钟）前由客户端自动续签房间 Token。

### 4.7 `POST /api/chat/call/livekit-webhook`
接收 LiveKit 服务端签名回调（`Authorization` JWT 校验）：
- `participant_joined`：校正 `ConnectedAt`（首次有第 2 人加入时开始正式计费/计时）、更新 `maxParticipants`。
- `participant_left`：清理断线未调用 `hangup` 的成员状态与 `call:busy` 锁；若 `Direct` 对端已离开或 `Group` 房间人数归 `0` 且无待接者，触发结算。
- `room_finished`：最终兜底结算，确保不会残留僵尸房间与丢失通话记录消息。

---

## 5. LiveKit Token 与 Room 规范

- **RoomName 命名**：`gotoim_call_{callId}`
- **Participant Identity**：`{chatObjectId}:{deviceId}`（保证同一人多设备不会在 LiveKit 内因同 `identity` 互踢出错；客户端展示时通过 `Metadata` 中的 `sessionUnitId` / `chatObjectId` / `displayName` 关联群成员信息）。
- **Participant Metadata（JSON）**：
  ```json
  {
    "sessionUnitId": "guid",
    "chatObjectId": "10001",
    "deviceId": "win-device-01",
    "displayName": "张三"
  }
  ```
- **VideoGrants**：`RoomJoin = true`, `Room = roomName`, `CanPublish = true`, `CanSubscribe = true`, `CanPublishData = true`；`ttl = 600s`。

---

## 6. Redis Key 与状态机规则

| Key | 类型 | TTL | 说明 |
| --- | --- | --- | --- |
| `call:session:{callId}` | Hash | 振铃 90s / 通话中 4h 续期 | 房间主状态，所有迁移走 Lua 脚本 |
| `call:participants:{callId}` | Hash | 同上 | `sessionUnitId -> json(status, deviceId, joinedAt)` |
| `call:busy:{chatObjectId}` | String (`callId`) | 振铃 60s / 通话中续期 120s | 单账号单身份占线锁，防互呼与多通冲突 |
| `call:group:session:{sessionId}` | String (`callId`) | 同房间生命周期 | 群会话单活跃通话锁 |
| `call:settled:{callId}` | String (`1`) | 24h | 幂等锁（`SET NX`），保证写 `MessageTypes.Call = 20` 绝对只执行一次 |
