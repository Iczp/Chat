# Chat 管理功能开发文档

本项目统一交付 Chat 管理 API、业务治理能力和 Vben「即时通讯管理」。**第一阶段同时交付 `IczpNet.Chat.Admin.Application.Contracts` 和 `IczpNet.Chat.Admin.Application`，完成后端及 HTTP 契约验收。第二阶段开发 Vben Admin，逐页迁移现有管理功能并接入新增治理流程。** 所有管理 API 由 Chat Host 通过 ABP 自动生成至 `/api/chat/admin/...`，所有管理业务方法在 `IczpNet.Chat.Domain`。

## 代码边界

| 层 | 职责 |
| --- | --- |
| `IczpNet.Chat.Domain` | Chat 业务规则、资源范围判定、状态转换、业务查询和幂等。优先复用 `IChatObjectManager`、`IRoomManager`、`ISessionManager`、`ISessionUnitManager`、`IMessageManager` 及现有领域服务。 |
| `IczpNet.Chat.EntityFrameworkCore` | 实现 Domain 定义的仓储查询，负责索引、投影、分页及数据库操作。 |
| `IczpNet.Chat.Admin.Application.Contracts`（必建） | 定义管理 AppService 接口、输入输出 DTO、ABP 权限常量、权限定义和本地化接入。使用 ABP Application.Contracts 与 Chat Domain.Shared 中必要的共享定义。 |
| `IczpNet.Chat.Admin.Application`（必建） | 管理 API 的唯一实现项目。实现管理 Contracts 的 AppService 接口，处理 ABP 权限、请求校验、DTO 映射、Unit of Work 边界和调用 Domain。 |
| `IczpNet.Chat.Admin.HttpApi`（按需） | 管理 API 的 Swagger 分组、文档过滤器或其他 HTTP 配置需要独立复用时，承载这些传输层配置，按需引用管理 Contracts。当前单 Host 方案由 Chat ApiHost 直接注册管理程序集。 |
| `IczpNet.Chat.HttpApi.Host` | 组合 `ChatAdminApplicationModule`，注册自动 API、认证与 Swagger。当前部署的 `chat-api` 由 Rctea.IM 的部署脚本构建此 Host。 |
| Vben `apps/web-antdv-next` | 通过现有请求客户端调用管理 API，按 ABP 授权呈现菜单和按钮，负责交互、分页和错误展示。 |

### 项目依赖与启用条件

```text
IczpNet.Chat.Admin.Application.Contracts → IczpNet.Chat.Domain.Shared
IczpNet.Chat.Admin.Application           → IczpNet.Chat.Admin.Application.Contracts
                                         → IczpNet.Chat.Domain
IczpNet.Chat.Admin.HttpApi（可选）        → IczpNet.Chat.Admin.Application.Contracts
IczpNet.Chat.HttpApi.Host                → IczpNet.Chat.Admin.Application
                                         → IczpNet.Chat.Admin.HttpApi（启用时）
```

- 后端首批新建 Contracts 项目并加入解决方案，定义 `ChatAdminApplicationContractsModule`。`ChatAdminApplicationModule` 通过项目引用和 ABP `[DependsOn]` 接入该模块。Contracts 对业务模型的依赖限于 Domain.Shared 的共享类型。
- `Admin.HttpApi` 启用时由 ApiHost 组合。ApiHost 负责为 `ChatAdminApplicationModule` 的程序集配置 `ConventionalControllers` 和 `/api/chat/admin/...`。管理接口仍通过该程序集的 AppService 自动生成，`Admin.HttpApi` 禁止增加手写管理 Controller、转发端点或业务实现。

`IczpNet.Chat.AdminApplication` 原有骨架沿用为带点的 `IczpNet.Chat.Admin.Application`。从旧 Application、ManagementApplication 和 Controller 盘点现有功能与调用方，复用或提炼其中的领域能力。普通 Chat AppService 和管理 AppService 通过各自的入口直接调用公共 Domain 方法。

## 管理 API 约定

- Host 使用 `ConventionalControllers.Create(typeof(ChatAdminApplicationModule).Assembly, ...)`，设置 `RootPath = "chat/admin"` 与 `RemoteServiceName = "ChatAdmin"`。管理 AppService 按 ABP 约定生成 `/api/chat/admin/...`。内部方法保持非公开，需要时使用 `[RemoteService(false)]`。管理功能禁止手写 Controller、Minimal API、Host 转发或旧路由别名。 [ABP 自动 API 控制器](https://abp.io/docs/10.7/framework/api-development/auto-controllers?LanguageCode=en)
- 旧 `/api/chat/...` 非 Admin 接口只作为迁移盘点依据。管理后端禁止调用旧接口或旧 AppService，Vben 的 Chat 管理请求禁止直连、通过代理转发或失败后回退到旧接口。列表、详情、选择器、关联查询、写操作和批处理都必须使用 ChatAdmin。
- 每批交付实际 HTTP 方法、路径、请求参数来源、DTO、分页契约、权限名和错误语义。验收运行中的 Swagger 与 `/api/abp/api-definition`，逐项检查管理接口的程序集、`ChatAdmin` 服务归属、路径前缀和路由唯一性。部署前通过 Vben 使用的代理访问管理路由，并核对页面实际请求。
- 权限沿用 ABP Permission Definition Provider 与本地化机制。各资源的读取、写入及高影响操作独立授权。复用既有权限时先确认语义和数据范围等价，新增权限时记录授予对象和租户侧别。用户传入的租户、资源 ID、筛选条件只能在授权范围内进一步缩小结果。
- Domain 校验目标归属和可执行状态，并在写入时处理重复请求、并发和缓存失效。高影响动作记录操作者、租户、目标、原因及结果，沿用 ABP 审计和已有业务审计机制，裁剪消息正文、令牌、私有附件数据与其他敏感字段。
- 消息、会话、成员和附件查询按真实数据量设计。使用受限筛选和仓储投影，大表优先使用稳定游标。需要用户视角时沿用 SessionUnit / DeletedRecorder 规则。管理诊断可见范围单独授权和验证。消息 `long` ID 在浏览器链路验证精度，管理 DTO 需要精确保留时采用字符串。
- 统计沿用持久化来源与现有落库流程。新增统计须写清数据库基数、Redis 待刷增量、Worker 幂等落库、恢复与降级机制。需要数据库持久事件或其他可重放的持久化来源来恢复 Redis 清空前尚未刷盘的增量。在线连接操作沿用分布式连接状态，任务重试沿用现有 Worker 的认领及重试机制。

### 旧业务逻辑提炼顺序

1. 逐项定位旧 API 的真实实现和现有 Domain 能力，核对查询范围、状态转换、缓存和事件副作用，以及当前调用方。
2. 已有领域方法直接复用。旧 AppService 或 Controller 中的业务查询、资源判断、状态变化、幂等与副作用协调，提炼为现有 Domain Manager 或领域服务的具名方法。确需查询优化时，在 Domain 定义仓储契约，由 EntityFrameworkCore 实现投影、索引查询和分页。
3. 在 `IczpNet.Chat.Admin.Application.Contracts` 定义独立管理契约与权限，在 `IczpNet.Chat.Admin.Application` 实现薄 AppService。由 ABP 校验权限，委托 Domain 验证目标范围并执行业务，最后映射管理 DTO。仍有客户端调用的旧入口直接复用提炼后的领域方法，并按原契约执行回归测试。

## 业务对象和事实来源

从实体的实际用途、关系和读写路径定义管理用例。实施前核对查询、写入、过滤器及正在生效的迁移。 [ChatDbContext](../../src/IczpNet.Chat.EntityFrameworkCore/EntityFrameworkCore/ChatDbContext.cs) · [实体映射](../../src/IczpNet.Chat.EntityFrameworkCore/EntityFrameworkCore/ChatDbContextModelCreatingExtensions.cs)

| 实体用途 | 管理时如何取证 |
| --- | --- |
| `ChatObject` 是聊天身份。其账号关联、对象类型、分类和扩展属性分别承担不同用途。 | 从当前账号与聊天对象的实际关联验证归属。分类树节点、类型、对象分类绑定和属性字典各按自己的适用范围解释。 |
| `Session` 表示会话。`SessionUnit` 表示某个聊天对象参与该会话的身份和视角。 | 会话级策略看 `Session`，成员视角看 `SessionUnit`、`SessionUnitSetting` 及处置记录。已读、最后消息、未读和排序需同时标明落库状态与缓存状态。 |
| `Message` 保存消息身份与会话顺序。正文有多种内容实体，引用、转发、接收范围与个人状态分别有自己的关系。 | 从会话和指定成员或工单进入查询，核对消息类型到内容的映射以及当前可见逻辑。读取路径实际采用的规则决定可见性。 |
| `MessageChange` 和 `MessageChangeCursor` 分别记录变更与设备应用进度。 | 分开显示全局消息 ID、会话 `SessionSeq`、变更 ID、设备确认位置和留存窗口。每个序号只在自己的范围内比较。 |
| `Blob` 是逻辑附件，`BlobObject` 是存储对象，`BlobMessageRef` 记录附件与消息间的引用。 | 关联逻辑附件、授权上下文、物理对象和任务，分别核对消息引用数与逻辑对象引用数。存储域用于定位物理数据，访问资格按消息及资源归属判断。 |
| 角色、权限定义和授予记录保留权限来源。报表、计数、Redis 和连接池分别服务统计、加速与实时状态。 | 有效权限由 Domain 的检查链计算。报表、计数和在线数据都标明范围、观察时间、来源及待落库状态。 |

部分实体的基类声明未实现 `IMultiTenant`，而公告、模板、游标等也可能采用不同的直接或间接归属方式。每个管理查询先确认目标实体的真实归属链与过滤器。缺少可验证归属的历史记录只能进入明确授权、脱敏的异常核对入口，待补齐证据后再执行资源操作。 [实体基类](../../src/IczpNet.Chat.Domain/BaseEntities/BaseEntity.cs)

## 待开发的 Chat 管理业务模块

以下模块按业务资源组织管理 Contracts 和 Application，由各 AppService 调用 `IczpNet.Chat.Domain`。批次一至四是本轮管理端基线，覆盖全部 13 个现有 Vben 菜单和新增的 Chat 核心业务管理。批次五为后续扩展。表中功能均为交付目标，括号中的路径为现有 Vben 菜单，领域类和实体是实现定位线索。以 [Vben Chat 路由](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/router/routes/modules/chat.ts)、[Chat Domain](../../src/IczpNet.Chat.Domain) 和 [旧管理应用服务](../../src/IczpNet.Chat.ManagementApplication) 作为盘点依据。

### 批次一 · 聊天身份与基础资料

| 管理模块 | 要开发的功能 | Domain 依据与验收重点 |
| --- | --- | --- |
| 聊天对象与身份（`/chat/chat-object`） | 按聊天号、名称、类型、分类查询列表和详情，查看账号关联与名下聊天身份，维护昵称、头像资料、验证方式及经核实的创建、变更和停用操作。提供可复用的发送者、成员、对象选择查询。 | `ChatObjectManager`、`AppUsers`、`ChatObjectEntryValues`。验证一账号多身份、归属范围、受保护对象和历史关联。头像展示沿用已授权的资源访问。 |
| 对象分类与类型（`/chat/category-type`） | 分类树列表与详情，新增、编辑、移动分类，维护对象类型及允许的分类关系，查看引用后处理停用或删除。 | `ChatObjectCategoryManager`、`ChatObjectTypeManager`、`ChatObjectCategoryUnits`。验证树根、子级、循环引用、系统类型及在用引用。 |
| 动态属性与数据字典（`/chat/dictionary`） | 管理字段定义树、字典项、选项值、类型和适用范围，在聊天对象详情查看动态属性。 | `EntryNameManager`、`EntryValueManager`。验证重名、树层级、选项引用及敏感属性裁剪。 |
| 好友与联系人 | 查询对象之间的好友关系、好友验证流程及联系人标签。支持按授权诊断申请状态和双向会话关系，关系变动使用已验证的领域操作。 | `FriendsManager`、`SessionRequestManager`、`SessionUnitContactTags`。支持人员按目标查证，私人标签、备注和关系明细单独授权。 |
| 邀请码（`/chat/invitation-code`） | 查询、详情、创建、维护归属和失效时间，停用或作废并核对当前状态。 | `InvitationCode`。验证归属、过期及已投放码的修改约束，列表遮盖完整码值。次数限制依据相关二维码场景或后续明确的产品需求实现。 |

#### 实体治理用例

| 用例 | 实体及用途 | 要开发的管理功能与验收 |
| --- | --- | --- |
| 身份归属核对 | `ChatObject.AppUserId`、对象类型与实际账号关联 | 按账号或聊天对象定位身份，说明主体类型、归属和启停状态。检查账号关联缺失、默认身份异常和受保护身份的在用关系。系统身份、机器人及匿名身份按各自规则判断。 |
| 身份停用影响预检 | `ChatObject`、`SessionUnit`、`ConnectionChatObject` | 停用前显示会话、群主身份、在线节点及专项资源依赖，按领域规则确认可以执行的动作。验收停用后的登录或聊天入口、历史档案和连接状态。 |
| 分类树与类型引用治理 | `ChatObjectCategory`、`ChatObjectCategoryUnit`、`ChatObjectType` | 诊断缺失父节点、层级循环、引用已停用分类以及对象与类型配置不一致。移动、合并或停用前展示波及对象，保存前重验树规则及引用变化。 |
| 属性字典与残留值 | `EntryName`、`EntryValue`、`ChatObjectEntryValue`、`SessionUnitEntryValue` | 查询字段在哪些对象或会话成员上使用，发现字典项缺失、类型不匹配和存量孤立值。提供停用预检和受限迁移计划，敏感字段只显示诊断标记。 |
| 联系关系排查 | `SessionRequest`、好友关系、`ContactTag`、`SessionUnitContactTag`、`Follow` | 按目标和时间串起申请、通过后的会话关联及互动设置，区分好友验证、私人标签和关注。只在授权对象范围内显示关系，建议重走原有流程的情况由工单跟进。 |
| 邀请码、二维码和处理流水 | `InvitationCode`、`QrCode`、`ScanCode`、`ScanHandler` | 查看归属、场景、启停和过期状态，串起扫码处理耗时及失败环节。次数限制和用量以 `QrCode` 已有字段为准，邀请码次数能力需先核对产品需求与持久化设计。遮盖码值、载荷和处理响应。邀请码治理归批次一，Chat 二维码及扫码处理随批次四接入。 |

### 批次二 · 会话、群组与授权

| 管理模块 | 要开发的功能 | Domain 依据与验收重点 |
| --- | --- | --- |
| 会话档案 | 查询单聊、群聊及其他对象类型的会话，按 `SessionId`、参与对象和状态定位。查看关联成员、最后消息元数据与关键时间，提供关联消息、群组和工单入口。 | `SessionManager`、`ISessionRepository`。明确会话与个人会话单元的身份和作用范围，保护私聊内容。 |
| 群组与群主治理 | 群列表、详情、群资料、群主和管理员查询。交付建群、编辑资料、邀请、管理员任免及具备权限的转让、无主认领或解散流程。 | `RoomManager`、`SessionUnitSetting`。验证群主目标保护、群系统单元排除、角色边界、解散通知以及重复请求。平台治理与群内治理各自授权。 |
| 会话成员与成员状态 | 按群、聊天对象、成员身份、组织和状态查询，查看入退群、群昵称、禁言、踢出状态。交付受控的邀请、成员移出、禁言和解除禁言。 | `SessionUnitManager`、`SessionUnitSettingManager`。验证跨群目标、保护群主与管理员、批次上限及推送一致性。 |
| 会话设置与处置记录 | 查看成员视角的入群历史可见范围、置顶、免打扰和退出或删除状态，查询处置时间线与重入状态。按已有业务能力处理会话恢复或纠错。 | `SessionUnitSettingManager`、`SessionUnitDisposalManager`、`BoxManager`。验证退群与删除的状态语义、重新激活及客户端同步。 |
| 入群与好友申请 | 按申请类型、会话、发起人、时间和状态查询，查看审批记录，处理允许审批的同意或拒绝，以及已过期申请。 | `SessionRequestManager`、`SessionRequestNotificationJob`。核对审批人归属，验证过期、重复审批与通知幂等。 |
| 群组织架构 | 组织树、未分组及多组织成员查询，新增、编辑、移动及删除空组织，受控批量调整成员归属和组织角色绑定。 | `SessionOrganizationManager`、`SessionUnitOrganization`、`SessionOrganizationRole`。验证循环、成员范围与提权保护。 |
| 群角色与授权 | 维护群角色和成员角色，查看直接授权、个人直授和组织角色来源，按允许范围授予或收回，并展示有效权限。 | `SessionRoleManager`、`SessionPermissionChecker`、`SessionPermissionRoleGrant`、`SessionPermissionUnitGrant`。验证内置管理员角色、群主保护和多来源授权。 |
| 群权限目录 | 查看权限组、权限定义、适用范围和启用状态，维护允许配置的目录或状态。展示定义变更影响的授权范围。 | `SessionPermissionGroupManager`、`ISessionPermissionDefinitionRepository`。逐项核对系统内置定义、旧服务的未实现方法和 Host/租户授予边界，以实际定义方式决定可写功能。 |
| 群标签 | 群内标签列表、增改、停用或删除，查看标签成员和批量增删归属。 | `SessionTagManager`、`SessionUnitTags`。验证标签和成员同属目标群、变更上限及缓存同步。 |

群管理按现有群主、成员、组织和角色规则验收。会话处置沿用已定义的客户端同步语义。 [群管理与组织授权设计](../../../Rctea.IM/docs/codex/GROUP-MANAGEMENT-ARCHITECTURE.md) · [会话单元处置规范](../../../Rctea.IM/docs/codex/SESSION-UNIT-DISPOSAL-SPEC.md)

#### 实体治理用例

| 用例 | 实体及用途 | 要开发的管理功能与验收 |
| --- | --- | --- |
| 会话与成员拓扑核对 | `Session`、`SessionUnit`、`ChatObject` | 按会话核对成员主体、目标对象、会话类型和所属范围，识别缺失配置、失效引用和违反唯一约束的历史异常。空设置、系统单元、历史状态与真正异常分别判定。 |
| 群主、管理员与继任 | `SessionUnitSetting`、`SessionRole`、`SessionUnitRole` | 定位无有效群主、多个疑似群主、离群后仍有管理员来源等情形，展示继任候选和动作资格。通过群主转让或认领等现有业务命令处置，并核对审计及群通知。 |
| 群策略与成员可见窗口 | `Session`、`SessionUnitSetting` | 查看邀请策略、禁言策略、成员收发状态和入群历史窗口。用于排查无法进群、无法发言或无法查看历史的真实原因，校验离群、禁言到期和不同入群时点。 |
| 组织结构及跨范围关联 | `SessionOrganization`、`SessionUnitOrganization`、`SessionOrganizationRole`、`SessionUnitTag` | 查看组织树、标签与角色的作用范围，检测跨会话绑定、失效组织引用、异常角色和批量调整影响。执行前重新计算受影响成员的授权。 |
| 有效权限解释与审查 | `SessionPermissionDefinition`、`SessionPermissionGroup`、`SessionPermissionRoleGrant`、`SessionPermissionUnitGrant` 和组织、角色关系 | 输入成员与权限，显示 Domain 判定结论及实际来源，支持检查失效授权、禁用定义和敏感能力波及范围。核对群主保护、内置角色、多来源和禁用成员，平台权限另行检查。 |
| 退群、删除与恢复时间线 | `SessionUnitDisposal`、`SessionUnitSetting`、`DeletedRecorder` | 关联处置原因、操作者、清空边界和会话重入记录，解释会话列表变化和历史可见性。按已有领域恢复语义处理，通过离线设备回放验证最终状态。 |
| 会话盒子、排序和归属 | `Box`、`SessionUnit.BoxId`、排序字段和现有列表缓存 | 按特定对象核对盒子归属、移动前后位置、列表分组和置顶表现，定位数据库与缓存差异。支持指定会话的重建或恢复预检，验证未改变成员的可见范围。 |
| 申请待办与超时 | `SessionRequest`、已有通知作业 | 对照申请类型、审批资格、目标状态和通知结果，定位积压、重复处理或目标已失效。用原审批命令处理可办理记录，验证并发只能产生一次有效结果。 |

群系统单元的识别、群主判定和组织角色边界沿用现有领域规则。 [群管理结构](../../../Rctea.IM/docs/codex/GROUP-MANAGEMENT-ARCHITECTURE.md) · [会话单元处置规范](../../../Rctea.IM/docs/codex/SESSION-UNIT-DISPOSAL-SPEC.md)

### 批次三 · 消息、内容运营与治理

| 管理模块 | 要开发的功能 | Domain 依据与验收重点 |
| --- | --- | --- |
| 消息档案与定向查询 | 按授权范围使用会话、消息 ID、发送者、类型、时间和状态定位消息，查看引用、回复、转发来源和附件元数据。默认只返回必要元数据，按工单单独授权查看所需内容。 | `MessageManager`、`IMessageRepository`。验证稳定游标、长整型精度、历史可见性和越权目标。 |
| 消息处置与审计 | 查看撤回和删除记录，按现有领域行为执行允许的全局撤回或用户视角处置。记录操作范围、原因、结果和关联工单。 | `MessageManager`、`DeletedRecorders`、`MessageChange`。验收撤回与单方删除语义、多设备推送、离线增量同步、重复处置和附件引用状态。 |
| 消息回执、互动与未读诊断 | 查看打开、已读、收藏及未读、提及、特别关注的汇总或异常，按独立权限查询最小必要明细。 | `OpenedRecorders`、`ReadedRecorders`、`FavoritedRecorders`、`SessionUnitCounterManager`。对照实际口径，验证权限裁剪、数据时点和重复事件。 |
| 群公告与群通知 | 查公告列表、版本、置顶、审核和定时发布状态。按权限执行审核、发布、撤销、置顶和提醒，查看已查看、已确认及待确认统计。 | `SessionAnnouncementManager`、`SessionAnnouncementReceipt`。验证会话范围、版本与回执对应、发布幂等及通知状态。 |
| 文章消息与定时发送（`/chat/article-message`） | 选择授权的消息身份与目标会话，查看历史，执行立即发送、定时发送、调度查询、取消及撤回。外部文章内容通过已有 CMS 契约获取。 | `ArticleProvider`、`ArticleScheduleManager`、`MessageManager`。验证发送身份、历史关联、调度状态机和恢复重复执行。 |
| 自定义模板消息（`/chat/message-template`） | 模板草稿、编辑、预览、发布、启停、版本对比及授权试发。提供发送身份和目标会话选择。 | `MessageTemplateManager`、`MessageTemplateRevision`。验证草稿与已发消息的版本隔离、变量校验和试发权限。 |
| URL 风控规则（`/chat/url-security-rule`） | 规则查询、详情、创建、调整、启停、删除，以及授权的判定测试、缓存刷新和命中原因展示。 | `UrlSecurityRule`、`UrlSecurityManager`。验证规则优先级、域名匹配、缓存版本和规则回归用例。 |
| 违规举报工单（`/chat/security-report`） | 查询待办与历史，按对象、消息、链接、原因和状态筛选，查看授权证据，执行受理、处理、驳回并记录处置结果。通过关联用例发起后续处置。 | `SecurityReport`、`UrlSecurityManager`。验证工单并发处理、状态流转及举报人身份保护，工单权限与消息处置权限分别检查。 |

公告管理复用现有公告领域与发布流程。 [群通知与群公告设计](SESSION-ANNOUNCEMENT-DESIGN.md)

#### 实体治理用例

| 用例 | 实体及用途 | 要开发的管理功能与验收 |
| --- | --- | --- |
| 消息定位与发送幂等核对 | `Message` 的 `Id`、`SessionId`、`SessionSeq`、`ClientMessageId`、`EntityVersion` | 给定会话、发送身份或客户端消息号，在实际幂等范围内定位原始发送。检查版本异常和序号冲突，序号缺口结合历史留存、授权可见性和数据来源解释。 |
| 消息类型与正文关系检查 | `MessageContent` 及文本、HTML、媒体、链接、位置、名片、指令等具体内容映射 | 发现类型与结构不匹配、正文或关联缺失，输出内容类型和原因码。内容预览按类型裁剪，位置、名片和 HTML 按最小权限安全呈现。发现问题后通过工单关联原消息和修复验证结果。 |
| 引用、合并转发和来源追踪 | 消息引用与转发关系、`HistoryContent`、`HistoryMessage` | 按指定消息查看有限深度的来源链，定位目标撤回、源记录缺失或访问失效，分别验证当前层和来源层权限。合法的跨会话转发按消息接收规则解释。 |
| 消息可见性解释 | `Message` 的私有与范围标志、`Scoped`、`SessionUnitMessage`、`SessionUnitSetting`、`DeletedRecorder` | 输入消息 ID 和指定成员，解释发送方、接收方、加入窗口、撤回及单方删除如何影响该消息的可见性。实际读取链路决定各关系表的用途，诊断接口仅返回最小必要的结论与原因。 |
| 已读、打开、收藏、删除与提醒对账 | 各类 `Recorder`、消息计数、`MessageReminder`、`MessageFollower`、`SessionUnitCounter` | 区分事实记录、状态投影和缓存，按消息或成员对账，发现重复事件、落库延迟、遗漏或旧状态覆盖。验证私聊、全员提醒、特定成员提醒和关注计数口径。 |
| 内容风险线索与复核 | `Word`、`MessageWord`、`TextContentWord`、`UrlSecurityRule`、`SecurityReport` | 管理词条及关联线索，核对链接规则命中、规则到期、举报证据和处理结论。词与消息的关联记录作为线索。自动拦截及增加内容审核规则须另定判定、复核和误判处置条件。 |
| 工单取证、处置及复核 | `SecurityReport`、`Message`、目标会话或聊天对象、ABP 审计 | 将证据、处置原因、受理人员、授权范围和结果关联到工单。分别校验取证和处置权限，只调用可适用的 Domain 命令，例如消息撤回、成员处置和规则变更。复核按目标状态、客户端同步和审计记录进行。 |
| 公告、模板与文章版本核对 | `SessionAnnouncementRevision`、`SessionAnnouncementReceipt`、`MessageTemplateRevision`、`TemplateContent`、`ArticleMessage`、`ScheduledArticleMessage` | 核对发布版本、消息快照、确认回执和任务输出，定位误发、旧版本显示和同一调度重复投递。撤销与补发分别使用领域允许的业务动作，验证风险范围及历史版本一致性。 |

### 批次四 · 服务运营与故障诊断

| 管理模块 | 要开发的功能 | Domain 依据与验收重点 |
| --- | --- | --- |
| Chat 运营总览 | 展示授权范围内的聊天对象、会话、消息、在线连接、待处理举报及任务概况。展示数据时间，跳转到带筛选条件的模块详情。 | `MessageReportManager`、`ConnectionStatManager`、`SecurityReport`、`AiRunManager`。复用已核对的聚合口径和有界查询，验收权限裁剪和刷新成本。 |
| 在线连接与 Chat 节点（`/chat/online`） | 按节点、用户、聊天对象、会话和设备查看连接、最后在线及聚合状态。受控断开单连接、移除残留记录或在已批准的节点范围内执行批量处理。 | `OnlineManager`、`ConnectionPoolManager`、`ServerHostManager`。验证 Redis 分布式状态、多设备、重连与重复处理。 |
| 消息报表（`/chat/message-report`） | 提供按日期、消息类型、会话或对象的统计明细、汇总和筛选项，查看统计时点与落库状态。授权操作员可提交统计刷盘。 | `MessageReportManager`、`MessageReportFlushWorker`。核对持久化口径、时区、Redis 增量、分批落库及任务重试。 |
| 附件与对象存储（`/chat/blob`） | 查看附件和物理对象元数据、引用、上传和过期状态，诊断孤儿、GIF 衍生件与清理积压。按已有作业能力提交预检、重试或受控修复。 | `BlobManager`、`BlobAccessAuthorizationService`、`VideoGifPropagationOutboxService`、`TempBlobCleanupService`。验证私有访问、共享引用、预检时点和清理边界。 |
| 消息同步与会话缓存诊断 | 按明确目标核对消息游标、变更记录、同步窗口、会话最后消息或未读，以及 Dirty 和处理积压。需要修复时经预检提交现有批处理。 | `IMessageChangeRepository`、`SessionUnitCacheManager`、`SessionUnitFlushWorker`。验证 Redis 回源、多实例认领、重试和乱序，以及旧客户端增量同步。 |
| 聊天二维码与扫描流水（`/chat/scan-code`） | 查询 Chat 二维码用途、状态、过期及扫描事件，提供明确归属范围的停用、重置或诊断入口。 | `QrCodeManager`、`ScanCodeManager`。验证来源、用途、可用状态和隐私遮盖。扫码登录、授权登录及其联调流程由 ScanLogin 模块承载。 |
| AI 任务执行记录（`/chat/ai-run`） | 按会话、源消息和状态查询任务，查看故障原因、耗时及尝试历史。对符合状态条件的任务执行重试或取消，按留存规则处理记录。 | `AiRunManager`、`AiJob`、`RedisAiRunStateStore`。验证重复执行、运行中取消、回执和提示词或凭据裁剪。 |

附件诊断和受控修复按现有附件方案实施，管理调用统一使用 ChatAdmin。 [附件与对象存储实施方案](MESSAGE-ATTACHMENT-VBEN-ADMIN-PLAN.md)

#### 实体治理用例

| 用例 | 实体及用途 | 要开发的管理功能与验收 |
| --- | --- | --- |
| 消息变更与设备游标诊断 | `MessageChange`、`MessageChangeCursor`、`Message.MessageChangeId` | 按租户、账号、聊天对象和设备查看已确认位置、最后同步和留存边界。区分旧设备离线、窗口过期、变更缺失和客户端未确认，优先引导既有重同步流程。 |
| 未读、最后消息与缓存对账 | `SessionUnit`、`SessionUnitSetting`、`SessionUnitCounter`、Redis 及 Dirty/Processing | 显示数据库基线、缓存观测和待刷状态，识别未读反弹、排序偏移或落库积压。可计算的目标按批次重算或刷新，验证前后差异、多实例并发和旧事件重放。 |
| 附件引用与对象一致性 | `Blob`、`BlobObject`、`BlobMessageRef`、`BlobContent` | 从逻辑附件追踪上传来源、消息有效引用、转发共享和物理对象状态。按各自事实来源核对引用数，发现对象缺失、残留引用、过期未清理和可见性异常。清理前再次确认引用及对象状态。 |
| 媒体派生件与复用任务治理 | `AttachmentProcessingResult`、`AttachmentProcessingWaiter`、`BlobReuseRecord`、`VideoGifPropagationOutbox` | 按指定附件查看处理状态、配方版本标识、租约、等待者、复用凭证状态及传播进度。重试走现有认领与重试入口，验证过期租约、新任务抢占、已完成等待者及共享结果保护。 |
| 在线节点及连接对账 | `ServerHost`、`Connection`、`ConnectionChatObject`、Redis 在线状态 | 按连接、节点、账号与聊天身份定位残留、重连和最近心跳。对比数据库记录与实时状态，断开前重新核对当前连接身份与节点。 |
| 跨业务作业查询与跟踪 | `AiRun`、文章调度、附件任务、统计或会话刷盘 | 展示任务来源、状态、进度、时间、错误类别和关联输出。按类型选择允许重试或取消的动作，复用已有作业逻辑。验收仍在执行、已处理、重试超限和重试后产生输出的情况。 |
| 报表口径、刷盘与差异 | `MessageReportHour`、`MessageReportDay`、`MessageReportMonth` 与原始消息 | 标注粒度、时区、类型、时间范围和刷新点。定位聚合差异和超期未落库，重算受限时间窗口并核对持久化基数与 Redis 增量。 |
| 留存、清理任务与访问审计 | `MessageChange`、会话处置记录、临时附件、扫码和外呼记录及现有 Worker | 查看各类记录的留存配置、最近扫描边界、可清理量、仍引用或存在待办的跳过条件。依已确认留存策略执行预检与作业，验证离线增量、数据访问审计和清理结果。 |

状态与诊断只读入口优先实施，后续按权限开放可以验证结果的写操作。 [消息会话序号运行手册](MESSAGE-SESSION-SEQ-RUNBOOK.md) · [附件管理方案](MESSAGE-ATTACHMENT-VBEN-ADMIN-PLAN.md)

### 批次五 · 专项业务扩展

| 管理模块 | 要开发的功能 | Domain 依据与上线条件 |
| --- | --- | --- |
| 公众号、服务号与菜单 | 管理服务号档案、订阅状态、菜单树和点击统计。按服务号归属授予配置权限。 | `OfficialManager`、`MenuManager`、`MenuStatisticManager`。验收订阅状态、菜单规则及统计落库。 |
| 聊天广场 | 管理广场对象目录、可见范围、排序和公告入口，按实际领域能力交付运营维护。 | `Squares`、`ChatObjects`、`SessionSections/Announcements`。核对广场与群会话授权边界，确认所需状态和发布流程。 |
| 机器人与 AI 配置 | 管理机器人档案、归属、可用范围和能力配置，关联任务状态、失败记录及可用性诊断。 | `Robots`、`Ai`、`AiRuns`。模型凭据沿用宿主机密配置，后台仅展示脱敏配置和状态。 |
| 客服、掌柜与店小二 | 管理商户与坐席绑定、可接待状态、排队和分配，诊断转接与多端状态。按客服权限执行转接或取消排队。 | `ShopKeeperManager`、`ShopWaiterManager`、`CallCenterManager`。验证商户隔离、坐席权限、并发分配和状态通知。 |
| 开发者与回调 | 管理开发者接入状态及回调配置，查看事件投递、失败和重试记录。按权限提交已有投递重试或凭据轮换流程。 | `DeveloperManager`、`DeveloperJob`、`HttpRequests`。验收域名和目标约束、凭据遮盖及幂等投递。 |
| 钱包、账务订单与红包 | 查看钱包、订单、交易流水、红包发放和领取状态，支持异常核对。资金变更类动作按单独授权和审批流程设计验收。 | `WalletManager`、`WalletOrderManager`、`WalletRecorders`、`RedPacketManager`。以持久流水校验金额与状态，验证审计及重试不重复记账。 |
| 个性签名、词条与文字模板 | 管理签名资料、词条及文字模板资源，核对使用范围和关联引用。内容规则的扩展按审核需求另行设计。 | `MottoManager`、`WordManager`、`TextTemplates`。按资料和配置授权，逐项核对在用引用和内容可见范围。 |
| Chat 客户端配置与服务状态 | 查看 Chat 专用配置和服务状态，按租户、客户端范围和配置版本操作，记录变更及生效情况。 | `ClientConfigManager`、`ServiceStateManager`、`ChatSettings`。令牌、版本发布和设备管理通过各自模块处理，Chat 管理端可显示关联的 Chat 连接诊断。 |

#### 实体治理用例

| 用例 | 实体及用途 | 要开发的管理功能与验收 |
| --- | --- | --- |
| 公众号菜单与订阅核对 | 公众号聊天对象、`SessionUnit`、`Menu` 和菜单统计 | 核对服务号归属、订阅状态、菜单树发布范围及点击统计。发布与停用前检查在用入口和历史关联。 |
| 广场可见性和配置影响 | 广场聊天对象、`Session`、公告与当前可见设置 | 预览可见对象、排序、公告投放范围和受影响的租户或会话。交付时以实际读模型和配置范围验收。 |
| 客服分配、排队和转接 | `CallCenterConfiguration`、`CallCenterAssignment`、商户和坐席 | 按商户核对归属、坐席、会话活动和分配状态，定位无人领取、分配到已停用坐席或队列异常。仅读取有授权的客户和会话信息。 |
| 开发者事件投递核对 | `Developer`、`HttpRequest`、`HttpResponse`、现有回调作业 | 按开发者和事件追踪调用、失败、耗时及重试，脱敏 URL 参数、请求头、响应与凭据。重试使用当前验证通过的回调配置及业务约束。 |
| 钱包到账和订单对账 | `Wallet`、`WalletOrder`、`WalletRequest`、`WalletRecorder`、`WalletBusiness`、`PaymentPlatform` | 核对业务单、入账请求、账变前后金额和渠道状态，标记未入账、重复记账或余额不符。先交付只读差异清单，资金处置按单独业务审批及记账动作验收。 |
| 红包领取和回退 | `RedEnvelopeContent`、`RedEnvelopeUnit`、钱包流水 | 核对已领取、未领取、已回退及对应资金流水，按原活动和收发主体限制查看。处理差异后对照金额、订单及流水，验证重试幂等。 |
| 机器人及客户端配置漂移 | 机器人聊天对象、`AiRun`、`ClientConfig`、`ChatSettings` | 按身份、平台、设备或服务范围对照生效配置，定位版本差异、不可用提供方和回退失败。显示脱敏配置、变更审计及生效结果。 |

### 模块权限与资源范围

以下是待实现的权限能力。由 ABP 权限定义和本地化发布实际 policy，Domain 负责每次调用的租户、对象、会话或业务归属校验。

| 管理场景 | 权限设计与范围 |
| --- | --- |
| 聊天对象、分类、字典、邀请码 | 各资源分别设置读取与维护权限。涉及身份停用、码作废或系统项变更时明确目标范围和审计记录。 |
| 会话、群组、成员、申请、标签 | 查看档案、查看成员、审批、成员处置和群主操作分别授权。成员操作核对所属群，转让、认领和解散单独授权。 |
| 群组织、角色、权限目录 | 查询有效权限、组织维护、成员绑定、角色授权和平台目录管理分别检查。平台后台角色和群内角色各自通过对应的权限校验。 |
| 消息、文章、模板、公告、举报 | 普通元数据、工单证据或消息内容、发送、发布审核和撤回或删除分别授权。查看举报工单时按个案核查证据范围，处理操作记录案件和已执行动作。 |
| 在线、报表、附件、同步、扫码、AI 任务 | 按资源分配只读诊断与操作权限。断开连接、刷盘、修复、重试或取消按目标、范围或任务状态限制，并记录相关对象 ID。 |
| 专项业务 | 公众号、商户与坐席、机器人、开发者和钱包按归属授权。凭据变更、资金动作和全局配置单独走相应的审批和审计。 |

### 模块交付清单

- 批次一至四的每个模块都要登记所需页面、默认只读字段、高影响动作、ABP 权限、资源范围、Domain 入口和跨模块依赖。批次五按实际启用的产品能力和权限流程分批实施。遇到只有实体、旧壳或未实现方法的功能，在清单里记录缺失的领域能力和验收前置条件。
- 迁移现有页面时从 `apps/web-antdv-next/src/api/chat/` 补查详情、弹窗、选择器、联动、批量操作及共用组件请求。Vben 的 Chat 管理请求全部使用本项目的 ABP 自动 API。通用认证、扫码登录、设备管理与外部 CMS 依其业务归属走现有契约。
- 实施时逐项填写交付登记。权限名、HTTP 方法和路径使用 Host 实测值，后端和 Vben 各自记录验收结果。

| 模块 | 页面或操作 | 旧 API | Domain 入口 | ABP 权限与资源范围 | 实际 ChatAdmin API | 后端验收 | Vben 切换 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 聊天对象与身份 | 列表、详情、对象选择 | 现有 `/api/chat/chat-object` 系列 | `ChatObjectManager`，补齐管理查询 | 按租户和授权对象范围读取 | 以首批 Swagger 为准 | 待开发 | 后端验收后 |

跨模块验收至少覆盖聊天对象选人后建立群或关联会话、组织绑定后校验有效权限、从举报工单执行授权的消息处置并检查客户端同步、按消息定位附件或 AI 任务。每个链路同时验证操作者对查询目标和后续动作的权限。

## 治理操作和 Vben 工作台

1. **发现和定位。** 从资源详情跳转到关联异常。从治理待办按类型、状态、时间和授权范围筛选，展示目标 ID、判定依据、观测时点、影响范围和建议处置。计划扫描使用有界时间窗或游标，单次任务记录扫描范围和质量规则版本。
2. **定向检查。** 展示足以复核的最小证据，说明当前观察到的状态、预期状态、来源和无法确认的原因。支持查看已有业务工单或审计记录。写入前由授权人员确认目标和业务动作。
3. **预检与执行。** 预检保存或关联经脱敏的目标清单、预计影响、状态版本或等效的并发校验值和有效期。实际执行时重新验证权限、当前状态、引用及有效期。按任务身份及业务幂等键防止重复操作，限定批量大小。较长操作复用现有作业机制，返回可查询的任务和逐目标处置结果。
4. **复核与关闭。** 核对持久化状态、必要缓存、业务事件或客户端可见性。记录成功、跳过、预检失效、失败与待复核原因。可逆动作按既有业务流程恢复，涉及不可逆处置或资金的动作按各自确认及复核规则完成。

Vben 开发资源详情的关联视图、治理待办、只读诊断、预检与结果页，并按 ABP policy 控制各入口。复用举报工单、ABP 审计及现有作业的关联信息。新增持久化前先查现有模型能否完整记录目标、请求人、执行人、原因、动作、关联 ID、时间和结果。审计载荷使用脱敏字段，消息正文、用户令牌、回调凭据、私有对象存储键和完整二维码载荷留在各自受控访问路径。

## 整体规划与扩展方式

**基础接入 → 聊天对象只读用例 → 批次一至四后端交付 → Vben 管理端交付 → 专项扩展。** 先用聊天对象列表、详情走通授权、资源范围和 Swagger。会话与群组提供成员、权限及发送目标等公共查询，随后交付消息运营和服务诊断。批次一至四后端通过验收后，Vben 迁移现有菜单并新增会话群组、消息治理等页面。批次五每个专项继续按后端验收、Vben 接入的顺序实施。

| 扩展场景 | 实现方式 |
| --- | --- |
| 新增管理资源 | 在管理 Contracts 按资源组织接口、DTO 与权限，在管理 Application 实现 AppService。先找 Domain 中可复用的能力，再补明确的领域方法。新增服务沿用同一个 Host 注册，按实际 Swagger 更新 Vben 封装。 |
| 新增读模型或诊断 | Domain 定义查询范围和语义，仓储实现裁剪字段、投影、索引和分页。按资源独立定义响应，旧接口按原契约维护。 |
| 新增写操作、批量操作或长任务 | AppService 调用具名领域命令。Domain 处理状态、并发和幂等。长任务复用现有作业设施，按需返回任务 ID 供管理 API 查询进度，记录处理上限和失败恢复方式。 |
| 新增权限或版本 | 使用 ABP 权限定义与本地化，按资源和动作扩展。契约优先使用兼容性的新增字段与操作。确有不兼容需求时登记旧调用方、发布次序、并行兼容窗口和下线条件。 |
| 管理用例需要其他模块能力 | 先确认业务归属。Chat 领域需要协作时，使用依赖方向正确的领域契约或事件；由所属模块实现其业务规则。记录跨模块失败如何处理。 |

每批评审核对四项证据。第一，领域规则及实现的复用点。第二，实际 API 契约和资源权限矩阵。第三，数据库、Redis、任务及并发影响。第四，Vben 切换清单和前后端发布顺序。只有出现可复用场景或技术依赖，才扩展公共服务、基类或新项目。

## 第一阶段 · Admin.Application 后端

1. 新建并编译 `IczpNet.Chat.Admin.Application.Contracts`，让现有管理 Application 引用其项目与 ABP 模块。使用 Chat ApiHost 的自动控制器注册，按项目依赖与启用条件评估 `Admin.HttpApi`。首个真实用例交付后确认管理远程服务出现在 ABP API 定义中。
2. 盘点当批页面与调用方，按上文顺序复用、提炼或补足 Domain 方法。对涉及缓存、作业、消息同步或外部副作用的变更列明重试和一致性处理。
3. 在 Contracts 定义接口、DTO 与权限，在 Application 实现薄 AppService 并发布测试环境。基于实际 Swagger/API 定义登记契约，覆盖空列表、边界分页、错误输入、未登录、读写授权、跨租户或跨资源访问、高影响动作审计和已有客户端回归。涉及写操作时补验并发和重试。
4. 根据改动构建对应项目和 Chat Host，执行相关 Domain、Application 或 EF Core 测试。迁移期间保持原客户端契约。发生实体、索引变更时记录迁移影响，由获授权的发布流程执行。

**进入 Vben 的条件**为批次一至四的后端交付项均已验收。交接记录包含管理 API 的实测 Swagger、请求和响应样例、权限及资源范围、分页与错误约定，以及治理动作的审计、幂等和恢复结果。

## 第二阶段 · Vben Admin

1. 后端验收后，优先在现有 `src/api/chat/` 封装内更新请求和 DTO。复用 `src/api/request.ts` 的认证、租户、反伪造与统一错误处理。按实际 base URL 追加管理路径，避免重复 `/api`。服务或 DTO 若由 OpenAPI 生成，按 Vben 仓库规则放入生成目录，并由业务 API 层调用。
2. 按后端发布的 ABP `grantedPolicies` 配置路由与按钮，接入分页或游标和必要的确认交互。保持已有菜单路径，便于按页发布和回退。禁止在请求失败时转调旧管理接口。
3. 运行 `pnpm --dir apps/web-antdv-next run typecheck`，用浏览器网络请求逐项核对 Chat 管理调用均命中 `/api/chat/admin/...`。用无权、只读和可写账号联调，并验证失败时无旧接口回退，以及分页、大整数 ID、错误提示和重复提交。
4. 当批先发布后端，再发布 Vben。记录接口与权限变更、联调结果和回退版本。需要回退时回退 Vben 发布版本，保持各已部署版本对应的后端契约，核查调用方之后再决定旧入口的下线计划。

## 完成标准

每个纳入范围的 Vben Chat 管理请求都有通过真实 HTTP 验收的 ChatAdmin 接口及权限记录。仓库间检查确认业务逻辑在 Domain，Host 只负责注册，Vben 管理页面没有旧 Chat 管理请求或运行时回退。每项治理用例验收实体归属、证据来源和异常判断，涉及写入时验收预检、幂等、处置结果和事后复核。记录构建、测试、代理验证、回退演练及待补事项。

### 治理用例验收

- 批次一至四按上文的模块排期接入定向查询、异常诊断和明确可用的业务处置，依次通过后端契约和 Vben 验收。批次五按专项功能启用情况排期。需要补充领域命令、后台扫描或持久化证据的用例在实施登记中标出依赖。
- 每条规则验证正常数据、历史数据、授权范围外的数据和至少一条真实异常；对可写动作验证状态变化、预检过期、重复提交、部分失败、重试与事后复核。对异步链路验证多实例认领与故障恢复。
- 对会话、消息、成员、附件和记录大表用现有索引做目标查询和游标分页。有界批处理先在代表性数据量中核对查询计划及耗时。统计重算基于可追溯的持久数据，Redis 清空、超时或落库滞后时有明确的回源与诊断结果。
- 每条用例登记实体关系、权威读写路径、作用范围、权限、证据字段、动作、副作用、预检或回退条件、后端实测 API 和 Vben 入口。管理应用层统一由 `IczpNet.Chat.Admin.Application` 调用 Domain 并通过 ABP 自动生成 API。
