# Chat 管理功能开发文档

Vben「即时通讯管理」的 Chat 业务接口统一迁移到 `IczpNet.Chat.Admin.Application`，由 Chat Host 通过 ABP ConventionalControllers 发布。**先完成本轮管理后端和 HTTP 契约验收，再进入 Vben 开发阶段。** 后端按资源分批实现，Vben 按页面分批切换。已切换页面只使用管理 API。

## 代码边界

| 层 | 职责 |
| --- | --- |
| `IczpNet.Chat.Domain` | Chat 业务规则、资源范围判定、状态转换、业务查询和幂等。优先复用 `IChatObjectManager`、`IRoomManager`、`ISessionManager`、`ISessionUnitManager`、`IMessageManager` 及现有领域服务。 |
| `IczpNet.Chat.EntityFrameworkCore` | 实现 Domain 定义的仓储查询，负责索引、投影、分页及数据库操作。 |
| 管理契约 | 为真实管理用例定义接口、输入输出 DTO、权限和本地化。首批需要共享契约时建立 `IczpNet.Chat.Admin.Application.Contracts`，仅引用所需共享定义。 |
| `IczpNet.Chat.Admin.Application` | 处理 ABP 权限、传输输入校验、DTO 映射、Unit of Work 边界，调用 Domain 并返回结果。需要修改状态时调用具名领域方法。 |
| `IczpNet.Chat.HttpApi.Host` | 组合 `ChatAdminApplicationModule`，注册管理程序集、认证和 Swagger。当前部署的 `chat-api` 由 Rctea.IM 的部署脚本构建此 Host。 |
| Vben `apps/web-antdv-next` | 通过现有请求客户端调用管理 API，按 ABP 授权呈现菜单和按钮，负责交互、分页和错误展示。 |

`IczpNet.Chat.AdminApplication` 原有骨架沿用为带点的 `IczpNet.Chat.Admin.Application`。已有 `IczpNet.Chat.ManagementApplication` 和 `ManagementApplication.Contracts` 用于核对功能和兼容性。Chat Host 当前未启用旧管理模块；迁移时逐项核对旧代码及实际调用方。普通 Chat AppService 与管理 AppService 各自调用公共领域能力。

## 管理 API 约定

- Host 对管理程序集使用 `ConventionalControllers.Create`，`RootPath` 为 `chat/admin`，`RemoteServiceName` 为 `ChatAdmin`。生成路径位于 `/api/chat/admin/`。管理 AppService 必须使用 ABP 自动 API。类名和方法名遵循 ABP 约定，内部方法保持非公开，需要时用 `[RemoteService(false)]` 排除远程暴露。 [ABP 自动 API 控制器](https://abp.io/docs/10.7/framework/api-development/auto-controllers?LanguageCode=en)
- 每批交付实际 HTTP 方法、路径、请求参数来源、DTO、分页契约、权限名和错误语义。验收时查运行中的 Swagger 与 `/api/abp/api-definition`，由 `ChatAdmin` 查找管理服务，检查新旧路由匹配唯一且无冲突。部署前通过 Vben 使用的代理实际访问管理路由。
- 权限沿用 ABP Permission Definition Provider 与本地化机制。各资源的读取、写入及高影响操作独立授权。复用既有权限时先确认语义和数据范围等价，新增权限时记录授予对象和租户侧别。用户传入的租户、资源 ID、筛选条件只能在授权范围内进一步缩小结果。
- Domain 校验目标归属和可执行状态，并在写入时处理重复请求、并发和缓存失效。高影响动作记录操作者、租户、目标、原因及结果，沿用 ABP 审计和已有业务审计机制，裁剪消息正文、令牌、私有附件数据与其他敏感字段。
- 消息、会话、成员和附件查询按真实数据量设计。使用受限筛选和仓储投影，大表优先使用稳定游标。需要用户视角时沿用 SessionUnit / DeletedRecorder 规则。管理诊断可见范围单独授权和验证。消息 `long` ID 在浏览器链路验证精度，管理 DTO 需要精确保留时采用字符串。
- 统计沿用持久化来源与现有落库流程。新增统计须写清数据库基数、Redis 待刷增量、Worker 幂等落库、恢复与降级机制；如要求 Redis 清空后仍能恢复尚未刷盘增量，需要数据库持久事件或其他可重放的持久化来源。在线连接操作沿用分布式连接状态，任务重试沿用现有 Worker 的认领及重试机制。

## 功能盘点与迁移批次

以 Vben [Chat 路由](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/router/routes/modules/chat.ts) 和 `apps/web-antdv-next/src/api/chat/` 为初始清单。逐页搜集所有实际请求，包括弹窗、选择器、详情关联、批量操作，以及模板或复用组件发起的请求。每个请求登记旧 API、管理用例、所属模块、Domain 入口、权限、实际新 API、测试证据和切换状态。

| 次序 | 当前入口或候选功能 | 后端工作与验收关注点 |
| --- | --- | --- |
| 1 | 聊天对象、分类与类型、数据字典 | 先以聊天对象只读列表和详情验证完整链路，再补齐核实存在的管理动作。分类、类型和字典复用当前领域模型及树规则。 |
| 2 | 文章消息、自定义模板消息、邀请码 | 逐项迁移查询、发送或调度、模板维护和邀请码管理。保留发送者校验、任务状态、撤回规则和重复提交处理。 |
| 3 | 在线连接池、消息报表、附件及对象存储 | 先交付只读查询；断开连接、统计落库、附件诊断与作业操作单独授权和审计。附件按 [现有实施方案](MESSAGE-ATTACHMENT-VBEN-ADMIN-PLAN.md) 的资源保护要求实施，管理 API 入口使用本文件定义的管理层。 |
| 4 | 扫码流水、AI 任务记录、URL 风控规则、违规举报工单 | 确认各动作的领域归属。Chat 自有管理用例切换至 ChatAdmin，独立 ScanLogin 等模块的通用流程继续遵守其边界。对 AI 重试、规则刷新和工单状态变更验证多实例行为。 |
| 后续按确认范围排期 | 会话、群组、成员、角色、消息治理及旧管理模块其他资源 | 对照旧管理服务与现有 Domain 能力，逐项确定所需查询、动作和权限。群内角色和平台管理权限分别校验。 |

## 整体规划与扩展方式

**基础接入 → 首个后端只读用例 → 完成本轮管理 API → 分批切换 Vben → 扩展管理资源。** 基础接入确立项目、自动 API 和管理规则。首个只读用例走通授权、数据范围和 Swagger，再按上表交付本轮其他 API。进入 Vben 阶段后先用首个只读用例联调，再分批切换现有页面和开放读写权限。受控运维复用已有后台任务。群组、会话和消息治理等后续功能沿用同一交付顺序。

| 扩展场景 | 实现方式 |
| --- | --- |
| 新增管理资源 | 按资源组织 AppService、DTO 与权限，例如聊天对象与会话各有业务入口。先找 Domain 中可复用的能力，再补明确的领域方法。新增服务沿用同一个 Host 注册，按实际 Swagger 更新 Vben 封装。 |
| 新增读模型或诊断 | Domain 定义查询范围和语义，仓储实现裁剪字段、投影、索引和分页。按资源独立定义响应，旧接口按原契约维护。 |
| 新增写操作、批量操作或长任务 | AppService 调用具名领域命令。Domain 处理状态、并发和幂等。长任务复用现有作业设施，按需返回任务 ID 供管理 API 查询进度，记录处理上限和失败恢复方式。 |
| 新增权限或版本 | 使用 ABP 权限定义与本地化，按资源和动作扩展。契约优先使用兼容性的新增字段与操作。确有不兼容需求时登记旧调用方、发布次序、并行兼容窗口和下线条件。 |
| 管理用例需要其他模块能力 | 先确认业务归属。Chat 领域需要协作时，使用依赖方向正确的领域契约或事件；由所属模块实现其业务规则。记录跨模块失败如何处理。 |

每批评审核对四项证据。第一，领域规则及实现的复用点。第二，实际 API 契约和资源权限矩阵。第三，数据库、Redis、任务及并发影响。第四，Vben 切换清单和前后端发布顺序。只有出现可复用场景或技术依赖，才扩展公共服务、基类或新项目。

## 第一阶段 · 后端

1. 建立并编译管理项目，接入 Chat Host 的模块依赖和自动控制器注册。项目骨架阶段无需添加示例业务端点。首个真实用例交付后确认管理远程服务出现在 ABP API 定义中。
2. 盘点当批页面与调用方，为每个用例定位 Domain Manager、实体及仓储。缺少能力时优先扩展领域入口；将旧 AppService 内的业务规则提取到 Domain，再由两个应用入口复用。对涉及缓存、作业、消息同步或外部副作用的变更列明重试及一致性处理。
3. 定义管理契约和权限，实现薄 AppService 并发布测试环境。基于实际 Swagger/API 定义登记契约，覆盖空列表、边界分页、错误输入、未登录、读写授权、跨租户或跨资源访问、高影响动作审计和已有客户端回归。涉及写操作时补验并发和重试。
4. 根据改动构建对应项目和 Chat Host，执行相关 Domain、Application 或 EF Core 测试。各批通过权限、数据范围及 API 验收后登记契约，本轮全部通过后开始 Vben 开发。迁移期间保持原客户端契约。发生实体、索引变更时记录迁移影响，由获授权的发布流程执行。

## 第二阶段 · Vben

1. 后端验收后，优先在现有 `src/api/chat/` 封装内更新请求和 DTO。复用 `src/api/request.ts` 的认证、租户、反伪造与统一错误处理。按实际 base URL 追加管理路径，避免重复 `/api`。服务或 DTO 若由 OpenAPI 生成，按 Vben 仓库规则放入生成目录，并由业务 API 层调用。
2. 按后端发布的 ABP `grantedPolicies` 配置路由与按钮，接入分页或游标和必要的确认交互。保持已有菜单路径，便于按页发布和回退。禁止在请求失败时转调旧管理接口。
3. 运行 `pnpm --dir apps/web-antdv-next run typecheck`，用浏览器网络请求核对列表、详情、搜索选项和写操作均来自自动生成的管理路由。用无权、只读和可写账号联调，并验证分页、大整数 ID、错误提示和重复提交。
4. 当批先发布后端，再发布 Vben。记录接口与权限变更、联调结果和回退版本。需要回退时回退 Vben 发布版本，保持各已部署版本对应的后端契约，核查调用方之后再决定旧入口的下线计划。

## 完成标准

每个纳入范围的 Vben Chat 管理请求都有通过真实 HTTP 验收的 ChatAdmin 接口及权限记录。仓库间检查确认业务逻辑在 Domain，Host 只负责注册，Vben 管理页面没有旧 Chat 管理请求或运行时回退。记录构建、测试、代理验证、回退演练及待补事项。
