# 自定义模板消息协议规范 (v1)

## 1. 协议概述与核心概念

- **协议版本 (schemaVersion)**: 当前固定为 `1`。
- **模板版本 (templateVersion)**: 单个模板发布产生的自增版本号（从1递增）。
- **字段命名**: 所有 JSON 属性统一采用 `camelCase`。
- **标识类型**: ID 及 Snowflake 在网络传输和 JSON 交互中采用字符串（如 `"4a82b63d-e9cd-4bed-8d41-c7bd12fe506d"`），避免 JS Number 精度丢失。
- **消息类型 (MessageType)**: `Template = 13`。

---

## 2. 硬性限制边界 (v1 Limits)

| 限制项 | 上限 | 超出行为 |
| --- | --- | --- |
| 布局最大节点数 (MaxNodes) | 50 | 服务端 400 拒绝，客户端降级 |
| 根节点深度为1，最大深度 (MaxDepth) | 6 | 服务端 400 拒绝，客户端降级 |
| 字段定义最大数量 (MaxFields) | 30 | 服务端 400 拒绝 |
| 动作定义最大数量 (MaxActions) | 3 | 服务端 400 拒绝 |
| 素材图片数量 (MaxResources) | 3 | 服务端 400 拒绝 |
| 单段静态文本最大长度 (MaxStaticTextLength) | 1000 字符 | 服务端 400 拒绝 |
| 单个字段输入文本最大长度 (MaxFieldTextLength) | 由字段定义限定，且不得超过 2000 字符 | 服务端 400 拒绝 |
| 发布定义 JSON 最大大小 (MaxDefinitionBytes) | 32 KiB (UTF-8) | 服务端 400 拒绝 |
| 消息快照 JSON 最大大小 (MaxSnapshotBytes) | 64 KiB (UTF-8) | 服务端 400 拒绝 |
| FallbackText 最大长度 (MaxFallbackTextLength) | 256 字符 | 服务端截断/拒绝 |

---

## 3. 字段声明 (Field Definition)

字段为平面结构，拒绝嵌套对象与数组：
- `name`: 字符串，合法标识符（禁止 `__proto__`、`prototype`、`constructor` 等非法属性名）。
- `type`: `string` | `boolean` | `integer` | `number`。
- `required`: `boolean`。
- `maxLength`: 可选整数（仅对 string 生效，最大不得超过 2000）。
- `enum`: 可选字符串数组。
- `format`: 可选，仅支持 `text` | `url`。

---

## 4. 节点与组件白名单 (Component Whitelist)

| 节点类型 (type) | 允许属性 | 默认值 | 约束说明 |
| --- | --- | --- | --- |
| `card` | `children` | - | **必须且只能作为唯一根节点**，其子节点内禁止再次出现 `card` |
| `column` | `children`, `gap` | `gap: "medium"` | 纵向排列；`gap`: `small` \| `medium` \| `large` |
| `row` | `children`, `gap` | `gap: "medium"` | 横向排列，**最多3个子节点**；窄屏自动换行 |
| `text` | `text` 或 `bind`, `style` | `style: "body"` | `text`与`bind`二选一；`style`: `heading` \| `body` \| `secondary` |
| `field` | `label`, `bind` | - | 显示标签和绑定值。若绑定的字段缺失，**整行隐藏** |
| `badge` | `text` 或 `bind`, `tone` | `tone: "neutral"` | `tone`: `neutral` \| `success` \| `warning` \| `danger` |
| `image` | `resourceId` 或 `bind`, `aspectRatio` | `aspectRatio: "landscape"` | `aspectRatio`: `square` \| `landscape` \| `portrait`；bind必须为url字段 |
| `divider` | 无 | - | 主题分隔线 |
| `button` | `label`, `actionId` | - | 关联 actions 中的 id。**在引用模式下隐藏**；若动作绑定的字段缺失，**按钮隐藏** |

---

## 5. 动作白名单 (Action Whitelist)

`actions` 数组最多包含 3 项，`id` 在模板中唯一：
- `openUrl`:
  - `bind`: 绑定字段名，该字段类型必须为 string，且 format 为 `url`。
  - 仅接受 `http://` 或 `https://` 安全地址；禁止 `javascript:`、`data:`、`file:` 及自定义协议。
- `copyText`:
  - `bind`: 绑定字段名，点击后复制纯文本至剪贴板。

---

## 6. 消息快照结构 (Message Snapshot)

```json
{
  "schemaVersion": 1,
  "templateId": "4a82b63d-e9cd-4bed-8d41-c7bd12fe506d",
  "templateVersion": 1,
  "templateName": "维修工单",
  "fallbackText": "[维修工单]WX20261123001：待处理",
  "data": {
    "title": "空调维修",
    "orderNo": "WX20261123001",
    "status": "待处理"
  },
  "layout": {
    "type": "card",
    "children": [
      { "type": "text", "bind": "title", "style": "heading" },
      { "type": "field", "label": "工单编号", "bind": "orderNo" },
      { "type": "badge", "bind": "status", "tone": "neutral" },
      { "type": "button", "label": "查看详情", "actionId": "detail" }
    ]
  },
  "actions": [
    { "id": "detail", "type": "openUrl", "bind": "detailUrl" }
  ]
}
```

---

## 7. 降级渲染原则 (Fallback Rules)

客户端遇到以下异常情况时，一律降级渲染，不抛出未捕获异常：
1. `schemaVersion > 1`（未知未来协议版本）：降级显示为 `fallbackText`；
2. 存在未知节点类型、未知属性或结构越界（深度>6、节点数>50）：降级显示为 `fallbackText`；
3. 动作引用了不存在的 `actionId` 或绑定的字段格式非法：降级显示为 `fallbackText`；
4. 无法解析的损坏 JSON 格式：降级显示为 `[模板消息] 内容暂不可用`；
5. `fallbackText` 为空时，默认回退显示为 `[模板消息]`。

---

## 8. 统一错误码 (Error Codes)

| 错误码 | HTTP 状态码 | 业务说明 |
| --- | --- | --- |
| `TemplateNotFound` | 404 | 模板不存在或跨租户访问未公开模板 |
| `TemplateForbidden` | 403 | 无权管理、发布或使用该模板 |
| `TemplateDisabled` | 409 | 模板已被停用，禁止发送 |
| `TemplateVersionNotPublished` | 409 | 请求指定的 templateVersion 尚未发布或不存在 |
| `TemplateConcurrencyConflict` | 409 | 并发更新或发布时 concurrencyStamp 冲突 |
| `TemplateSchemaUnsupported` | 400 | schemaVersion 不受支持（当前仅支持 1） |
| `TemplateDefinitionInvalid` | 400 | 模板草稿或发布定义结构非法（超限、未知节点、属性错误） |
| `TemplateDataInvalid` | 400 | 提交的数据不符合字段定义（缺必填、枚举值不匹配、URL非法） |
| `TemplatePayloadTooLarge` | 400 | 定义超过 32 KiB 或快照超过 64 KiB 上限 |
