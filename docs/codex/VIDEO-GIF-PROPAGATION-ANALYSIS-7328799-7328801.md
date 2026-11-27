# 视频消息 7328799 / 7328800 / 7328801 GIF 显示差异分析报告

分析时间：2026-11-27

## 1. 结论总览

1. **后端数据完全正确，完全符合预期流程。**
   - `7328799`、`7328800`、`7328801` 三条视频消息在 `Chat_Module_v3` 中均有完整的 4 个私有逻辑附件（`mp4`、`mp4.jpg`、`thumb_*.jpg`、`*.gif`），`IsPublic = 0`、`Status = 1 (Bound)`、`RefCount = 1`。
   - `7328800` 与 `7328801` 两条相同视频消息成功触发了**物理文件去重与逻辑凭证隔离**：两者拥有各自独立的 `BlobId`，但底层共享完全相同的 4 个 `StorageObjectId`。
   - `7328799` 与 `7328800` 发送时 GIF 尚未生成，由 `Chat Worker` 异步生成后通过 `Chat_VideoGifPropagationOutbox` 回填 `VideoContent.GifUrl`，并生成了 `Chat_MessageChange`（`ChangeType = 3`）；`7328801` 发送时间晚于前两条 GIF 生成完成时间，因此发送时直接同步返回了 `GifUrl`。
2. **Electron 未显示 `7328799` 与 `7328800` 的 GIF，属于客户端实时更新链路问题（非后端问题）。**
   - Electron 主进程 `MainChatSignalRService` 收到 `updated@message` 后，未像回补接口那样向服务端拉取最新消息正文，而是直接尝试解析实时载荷并调用 `MessageRepository.upsertMessageInTx`。当 `updated@message` 为轻量通知或被判定为占位摘要（`isPlaceholder = true`）时，`upsertMessageInTx` 中的 `preserveFullContent` 会保留本地旧 `contentJson`（此时 `gifUrl` 为空），且未触发 `syncMessageChangesV2`，导致界面不刷新 GIF。
3. **Flutter 不会出现同类问题。**
   - Flutter 的 `RealtimeSyncCoordinator` 在收到 `SignalRCommand.messageUpdated`（`updated@message`）时，会主动调用 `_messageRepository.fetchAndApplyUpdatedMessage(...)` 向服务端拉取最新完整消息并覆盖本地库与 UI；若未命中还会回退触发 `loadChanges` 增量补偿。

---

## 2. 后端数据库核查明细（`Chat_Module_v3`）

### 2.1 `VideoContent` 与 `GifUrl`

| MessageId | VideoContentId | Url | GifUrl | CreationTime | LastModificationTime |
| --- | --- | --- | --- | --- | --- |
| `7328799` | `FF5A06CD-D478-EFB0-B557-3A252D3879D8` | `/api/chat/file/04d90468-497f-5a40-8deb-3a252d37ed3a` | `/api/chat/file/14524e66-076e-d999-d064-3a252d3b6539` | `03:27:58.611` | `03:31:12.065` |
| `7328800` | `5650809A-906B-8EB7-38C6-3A252D3AA762` | `/api/chat/file/ff2a437c-3005-1576-8154-3a252d3a5a76` | `/api/chat/file/b853f88f-13a8-e2e1-525e-3a252d3b67d3` | `03:30:19.625` | `03:31:12.378` |
| `7328801` | `B53CCBE2-062A-FEA9-3966-3A252D3CAC47` | `/api/chat/file/23d550c1-2888-6e57-8f69-3a252d3cab6a` | `/api/chat/file/1d34e837-9024-c599-4632-3a252d3cabbe` | `03:32:31.952` | `03:32:31.986` |

### 2.2 物理去重与逻辑隔离（`Chat_BlobMessageRef` + `Chat_Blob`）

- `7328800` 与 `7328801` 各自拥有独立的 4 个 `BlobId`，但共享同一组 `StorageObjectId`：
  - 视频：`461A5DC0-AA8B-1E33-8E5B-3A252D3A5C1F`
  - 封面：`FE877B29-A904-2D80-9BE8-3A252D3A9DA5`
  - 缩略图：`D257ADED-A791-9700-6E66-3A252D3AA284`
  - GIF：`49DCE6C3-CD05-2B61-D2E3-3A252D3B67D9`
- 所有 Blob 均为 `IsPublic = 0`、`Status = 1 (Bound)`、`RefCount = 1`。

### 2.3 异步外箱与消息变更记录

- `Chat_VideoGifPropagationOutbox`：
  - `7328799`：`ProcessedTime = 2026-11-27 03:31:12.376`，`RetryCount = 0`，`LastError = NULL`
  - `7328800`：`ProcessedTime = 2026-11-27 03:31:12.396`，`RetryCount = 0`，`LastError = NULL`
  - `7328801`：无外箱记录（发送时已同步命中已有 GIF，无需异步传播）。
- `Chat_MessageChange`：
  - `Id = 34`：`MessageId = 7328799`，`ChangeType = 3`，`CreationTime = 2026-11-27 03:31:11.782`
  - `Id = 35`：`MessageId = 7328800`，`ChangeType = 3`，`CreationTime = 2026-11-27 03:31:11.782`

---

## 3. 端到端流程对比与根因定位

| 环节 | 实际表现 | 是否正常 |
| --- | --- | --- |
| 后端上传与去重 | 首次上传异步入队生成 GIF，二次相同视频直接复用物理对象并同步返回 `gifUrl` | 正常 |
| 后端异步传播 | `VideoGifPropagationOutboxService` 成功回写 `VideoContent.GifUrl` 与 `Chat_MessageChange` | 正常 |
| Electron 实时更新 | `MainChatSignalRService.ts` 收到 `updated@message` 后未拉取最新消息正文或触发 `syncMessageChangesV2`，`MessageRepository.ts` 的 `preserveFullContent` 保护分支保留了旧 `contentJson` | **异常（待修复）** |
| Flutter 实时更新 | `RealtimeSyncCoordinator` 收到 `updated@message` 后主动调用 `fetchAndApplyUpdatedMessage` 拉取完整消息并更新本地库与 UI | 正常 |

## 4. 后续修复建议（Electron）

在 `gotoim-electron/src/main/services/MainChatSignalRService.ts` 处理 `updated@message` 时：
- 若载荷为轻量更新或缺少完整正文，应按 `(accountId, chatObjectId, sessionId)` 触发 `syncMessageChangesV2` 或通过 `MessageChangesV2` / 单条消息查询接口拉取最新快照后再执行 `upsertMessageSnapshotInTx`，确保 `gifUrl` 等异步回填字段能实时落库并推送到 Renderer。
