# 二维码业务闭环：后端待办任务与前端联动升级规范

> **文档状态**：待后端开发排期执行  
> **制定时间**：2026-10-15  
> **所属模块**：`IczpNet.Chat` (扫描与会话核心模块) / `Rctea.IM`

---

## 一、 背景与现状说明

1. **前端（Flutter & JS-SDK）现状**：
   - 前端已全面实现**本地高性能二维码异步解码器**（后台 Isolate + LRU 缓存）；
   - 前端已支持**微信同款大图预览及 H5 网页内图片长按识别**，并自动区分“用户资料”、“群聊”、“扫码登录”与“外部网址”；
   - 前端已支持长按“发送给朋友”直接唤起原生 `ForwardTargetPicker` 进行聊天会话转发；
   - 前端 WebView 与 JS-SDK 已打通 `scanCode` 扫一扫与剪贴板桥接能力。

2. **后端现状与不足**：
   - 后端目前已具备统一框架：`IScanCodeAppService.ScanAsync` (`POST /api/chat/scan-code/scan`) 与 `IScanCodeManager`；
   - 但在处理器集合（`IScanCodeHandler`）中，当前仅接入了扫码登录处理（`ScanLoginScanCodeHandler`），**缺少针对“用户个人名片二维码”和“群聊二维码”的业务验真、鉴权与时效性安全检查**；
   - 当前客户端展示的群二维码或名片二维码为静态 URL/明文 ID，缺少对齐微信的 **7 天时效性** 和 **动态签名 Ticket 防爬机制**。

---

## 二、 后端待开发任务清单（ABP DDD 架构）

严格遵循 `AGENTS.md` 规范，所有开发工作应在 `IczpNet.Chat` 模块的标准分层中实施，不擅自新增外部包。

### 1. 【Domain 层】新增 `UserScanCodeHandler` 处理器
- **代码位置**：`src/IczpNet.Chat.Domain/ScanCodes/Handlers/UserScanCodeHandler.cs`
- **实现接口**：`IScanCodeHandler`
- **核心逻辑**：
  1. 解析二维码内容（支持 `gotoim://user/{userId}` 或动态 ticket 短链接）；
  2. 校验目标用户是否存在、账号是否被注销或封禁；
  3. 查询当前操作人与目标用户的好友关系（未加好友、已是好友、黑名单）；
  4. 返回 `ScanHandlerResult`：
     - `Action = "user_profile"`
     - `Data = { userId, userName, name, portrait, isFriend, isBlocked }`
     - `Success = true`

### 2. 【Domain 层】新增 `GroupScanCodeHandler` 处理器（时效性与群安全）
- **代码位置**：`src/IczpNet.Chat.Domain/ScanCodes/Handlers/GroupScanCodeHandler.cs`
- **实现接口**：`IScanCodeHandler`
- **核心逻辑（对齐微信规则）**：
  1. 解析群二维码参数（如 `gotoim://group/join?ticket=xxx` 或 `token`）；
  2. **时效性校验**：群二维码默认 **7 天有效**（或读取系统配置），超时返回错误提示：“该群二维码已过期”；
  3. **群状态检查**：校验群是否已解散、是否开启“全员禁言”或“群主/管理员审批进群”；
  4. **成员排重**：检查当前扫码人是否已在群内；
  5. 返回 `ScanHandlerResult`：
     - `Action = "group_join_preview"`
     - `Data = { groupId, groupName, groupAvatar, memberCount, needsApproval, isAlreadyMember, inviterName }`
     - `Success = true`

### 3. 【Application 层】新增动态时效二维码生成 API
- **代码位置**：`src/IczpNet.Chat.Application/` 与 `Application.Contracts`
- **接口定义**：
  ```csharp
  // 1. 获取当前用户动态名片二维码
  Task<QrCodeOutputDto> GetMyQrCodeAsync();

  // 2. 获取指定群聊的时效二维码 (带 7 天过期时间与签名凭证)
  Task<GroupQrCodeOutputDto> GetGroupQrCodeAsync(Guid groupId);
  ```
- **输出 DTO 结构 (`QrCodeOutputDto`)**：
  ```csharp
  public class GroupQrCodeOutputDto
  {
      public string Ticket { get; set; }           // 动态凭证 short code
      public string Content { get; set; }          // 格式: gotoim://group/join?ticket=xxx
      public string QrImageUrl { get; set; }       // 生成的二维码图片直接访问地址
      public DateTime ExpiredTime { get; set; }    // 过期时间 (当前时间 + 7天)
      public string InviterName { get; set; }      // 邀请人昵称
  }
  ```

---

## 三、 ⭐ 后端开发完成后，需配套更新的前端模块清单

后端上线后，需要对以下 **4 个前端端点与模块** 进行联动改造更新：

### 1. Flutter 移动端 (`F:\Dev\GotoIM\gotoim-flutter`)

| 文件 / 模块 | 更新内容与动作 |
| :--- | :--- |
| **`lib/core/services/scan/qr_action_resolver.dart`** | **接入后端 Ticket 识别**：<br>当识别到 `gotoim://group/join?ticket=xxx` 或用户 ticket 时，调用 `POST /api/chat/scan-code/scan` 获取详细的验真结果模型（含群名、头像、是否过期），菜单精准显示为 **“加入：技术交流群 (75人)”**。 |
| **`lib/features/mine/presentation/my_qr_code_page.dart`** | **对接个人动态名片接口**：<br>从硬编码/静态展示改为调用 `GET /api/chat/my-profile/qrcode`，支持展示带安全签名的二维码与失效重刷机制。 |
| **`lib/features/group/presentation/group_qr_code_page.dart`** | **对接群时效二维码接口**：<br>展示群二维码时显示微信同款底部提示文案：*“该二维码7天内（MM月dd日前）有效，重新进入将更新”*。 |
| **`lib/core/services/scan/unified_scan_dispatcher.dart`** | **统一跳转鉴权联动**：<br>根据后端返回的 `needsApproval` 状态，自动跳转到“申请加群审批填写页”或“直接进群会话页”。 |

---

### 2. JS-SDK 独立工程 (`F:\Dev\GotoIM\gotoim-js-sdk`)

| 文件 / 模块 | 更新内容与动作 |
| :--- | :--- |
| **`src/core/types.ts`** | 新增 `ScanResultAction` 业务模型定义（对应后端返回的 `UserScanData` 与 `GroupScanData`）。 |
| **`src/modules/system.ts`** | `scanCode()` 方法返回增加 `action` 与 `data` 语义扩展字段，使得 H5 接入方无需自己写正则表达式解析 ticket。 |
| **版本发布** | 升级版本号并发布更新。 |

---

### 3. Nuxt 3 CMS / H5 移动前端 (`F:\Dev\abpvnext\IczpNet.Cms\web`)

| 文件 / 模块 | 更新内容与动作 |
| :--- | :--- |
| **`app/components/article/ArticleMeta.vue`** | **作者名片长按识别**：文章详情页底部的作者名片二维码直接绑定动态 ticket，用户长按即可直达作者个人资料卡。 |
| **`app/composables/useAppBridge.ts`** | 更新 `bridge.scanCode()` 类型声明，适配新的后端 Action 结构。 |

---

### 4. UniApp 跨端适配层 (`@gotoim/js-sdk/uni`)

| 文件 / 模块 | 更新内容与动作 |
| :--- | :--- |
| **`uni.scanCode` 适配器** | 保持与微信小程序 / UniApp 标准一致的入参和出参：<br>`res.result` 为纯文本，同时扩展 `res.scanData` 为后端解析后的用户/群模型，降低 UniApp 业务线重写成本。 |

---

## 四、 推进里程碑核对表

- [x] **前端 Milestone 1**：Flutter 本地 Isolate 纯离线解码器（已完成）
- [x] **前端 Milestone 2**：大图与 H5 网页内微信同款 ActionSheet 长按识别（已完成）
- [x] **前端 Milestone 3**：长按菜单“发送给朋友”唤起 `ForwardTargetPicker` 选人转发（已完成）
- [x] **前端 Milestone 4**：JSBridge 与 JS-SDK 扫一扫 `scanCode` 通道（已完成）
- [ ] **后端 Milestone 5**：实现 `UserScanCodeHandler` 与 `GroupScanCodeHandler`（待排期）
- [ ] **后端 Milestone 6**：实现 7 天时效群二维码与名片二维码生成 API（待排期）
- [ ] **前端联动 Milestone 7**：按上述清单完成 4 个前端模块联动升级（待后端上线后启动）
