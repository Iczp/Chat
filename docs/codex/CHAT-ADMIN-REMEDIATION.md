# ChatAdmin 代码审查与整改开发计划

复核日期为 2026-11-27。本文以 Chat `f45bdecc` 和 Vben `1f213301` 为实施基线，汇总前后两批交付、上一轮审查结论，以及补齐后的整改与验收任务。当前建议暂缓发布，优先修复附件访问保护、资源授权及 AI 批删路由，再完成 Vben 回归和真实角色联调。

管理业务在 `IczpNet.Chat.Domain` 实现，EF Core 负责仓储投影与索引查询。管理接口、DTO 和权限放在必需的 `IczpNet.Chat.Admin.Application.Contracts`，AppService 只在 `IczpNet.Chat.Admin.Application` 实现。Chat ApiHost 以 `RootPath = "chat/admin"` 和 `RemoteServiceName = "ChatAdmin"` 注册 ABP 自动 API。可复用的 HTTP 文档配置可按需放入 `IczpNet.Chat.Admin.HttpApi`。Vben 的 Chat 管理业务请求统一走 `/api/chat/admin/...`，禁用手写管理 Controller 和旧接口回退。[开发规则](../../AGENTS.md) · [管理功能开发文档](CHAT-ADMIN-DEVELOPMENT.md)

## 交付基线

| 仓库 | 最新提交及推进关系 | 本地远程跟踪分支 |
| --- | --- | --- |
| IczpNet.Chat | `a92dc492` 建立管理 Contracts，并交付基础、风控、报表及模板消息等服务。 `f45bdecc` 补齐总览、会话、消息、申请、角色和公告，并新增交付手册。 | `github/master` 指向 `f45bdecc` |
| vue-vben-admin | `956b81ad` 迁移原有 Chat 管理接口与路由权限。 `1f213301` 新增六个管理页面、对应 API 和路由。 | `origin/dev` 指向 `1f213301` |

| 补齐模块 | Domain 与管理 AppService | 已交付能力 | Vben 菜单和 API 封装 |
| --- | --- | --- | --- |
| Chat 运营总览 | `IOverviewAdminManager`、`OverviewAdminManager`、`OverviewAppService` | `GET /api/chat/admin/overview/summary` | `/chat/overview`、`overview.ts` |
| 会话档案 | `ISessionAdminManager`、`SessionAdminManager`、`SessionAppService` | 会话列表、详情、启停 | `/chat/session`、`session.ts` |
| 入群与好友申请 | `ISessionRequestAdminManager`、`SessionRequestAdminManager`、`SessionRequestAppService` | 申请记录列表和详情 | `/chat/session-request`、`session-request.ts` |
| 群角色 | `ISessionRoleAdminManager`、`SessionRoleAdminManager`、`SessionRoleAppService` | 角色列表、详情和权限统计 | `/chat/session-role`、`session-role.ts` |
| 消息档案 | `IMessageAdminManager`、`MessageAdminManager`、`MessageAppService` | 新增 `GetListAsync`、`GetAsync`，消息档案页接入既有管理 `RollbackAsync` | `/chat/message`、`message.ts` |
| 群公告 | `ISessionAnnouncementAdminManager`、`SessionAnnouncementAdminManager`、`SessionAnnouncementAppService` | 公告列表、详情及统计 | `/chat/session-announcement`、`session-announcement.ts` |

六个页面位于 `apps/web-antdv-next/src/views/chat/<模块>/index.vue`，API 在 `src/api/chat/index.ts` 统一导出，菜单由 `src/router/routes/modules/chat.ts` 注册。后端新增 `ChatGovernancePagesAdminAppService_Tests.cs`，覆盖六个模块的基础调用和路由注册。交付手册汇总了 Mermaid 架构图、22 个管理服务、权限树、19 个菜单映射、验收记录和批次五扩展指南。[交付手册](CHAT-ADMIN-DELIVERY-HANDBOOK.md) · [Vben 路由](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/router/routes/modules/chat.ts) · [治理测试](../../test/IczpNet.Chat.Application.Tests/Admin/ChatGovernancePagesAdminAppService_Tests.cs)

## P1 发布前必修

### 附件访问保护

- [ ] 在 Domain 复用或提炼受保护附件的判定，供普通入口和管理入口共用。保护私聊附件、临时上传、派生件以及静态附件。变更公开、静态或删除状态时，在写入前核对实际引用与当前状态，并处理并行绑定和删除的竞态。
- **现状**　管理更新直接写入 `IsPublic` 和 `IsStatic`。下载授权会直接放行公开附件，删除则依赖当前静态标记和引用计数。
- **验收**　普通附件编辑权限不能公开受保护附件，也不能通过先解除静态标记再删除来规避保护。覆盖未绑定临时附件、已绑定私聊附件、静态附件、合法元数据编辑，以及并发绑定与删除。验证拒绝后原附件仍按原有权限访问。
- [管理附件写入](../../src/IczpNet.Chat.Domain/Blobs/BlobManager.cs) · [附件访问授权](../../src/IczpNet.Chat.Domain/Blobs/BlobAccessAuthorizationService.cs) · [旧入口保护](../../src/IczpNet.Chat.Application/Blobs/BlobAppService.cs)

### 消息与会话资源授权

- [ ] 在 Domain 用可信操作者身份、租户和对象关系限定范围，再应用请求筛选。统一覆盖消息档案列表、详情、会话视角历史、文章及模板发送和撤回。同步复核新会话档案的列表、详情及启停、会话单元选择与查询。消息正文取证使用独立授权和最小必要字段。
- **现状**　消息历史和发送按请求的 `SessionUnitId` 查找，撤回按消息 ID 查找。新增消息档案列表也以请求参数为筛选条件，详情映射完整 `ContentJson`。单独通过 ABP 操作权限无法确定目标属于该操作者的工作范围。
- **验收**　用两个授权范围分别验证列表、直接访问 ID、伪造发送者、撤回和启停。无范围权限的请求不得返回正文、泄漏受限计数或写入业务状态。历史保留 `SessionUnit`、`DeletedRecorder` 和消息游标语义。授权撤回须核对消息变更记录和客户端同步。新增会话档案、申请、角色、公告和总览一并检查列表、详情及聚合统计的可见范围。
- [消息领域服务](../../src/IczpNet.Chat.Domain/MessageSections/Messages/MessageAdminManager.cs) · [消息管理服务](../../src/IczpNet.Chat.Admin.Application/Messages/MessageAppService.cs) · [会话档案领域服务](../../src/IczpNet.Chat.Domain/Sessions/SessionAdminManager.cs)

### 定时文章任务范围与执行状态

- [ ] 创建时核对发送身份和会话的授权范围。列表按当前授权范围查询，改期、取消和立即执行按任务归属及发送身份复核。将 `RequestedByTenantId` 纳入真实归属检查，保留分布式锁、版本校验和事务后入队。
- **现状**　管理列表按请求参数筛选。变更按任务 ID 查找并检查发送单元状态，但未使用任务的租户归属和操作者的发送身份范围。
- **验收**　不同租户或不同发送身份范围的任务无法被查看或处置。授权方可创建、改期和取消。重复提交、立即执行与 Worker 并发、入队失败后由恢复 Worker 补偿，都应验证最终只产生一次有效发送。
- [调度管理](../../src/IczpNet.Chat.Domain/MessageSections/ArticleScheduleAdminManager.cs) · [任务执行与恢复](../../src/IczpNet.Chat.Domain/MessageSections/ArticleScheduleManager.cs)

### 模板所有者范围与 ManageAll

- [ ] 定义 `owned`、`usable`、`ManageAll` 的资源范围。由 AppService 取得显式授权，Domain 统一校验租户、模板所有者、可使用范围及目标发送身份。将判定应用到列表、详情、修订、预览、创建、编辑、发布和启停，保留现有共享权限与并发戳保护。
- **现状**　列表与按 ID 读取没有所有者范围，写操作沿用相同读取入口，`ManageAll` 没有参与范围判定。
- **验收**　同租户两个所有者的私有模板互相隔离。共享模板仅在允许的使用场景可见。具有 `ManageAll` 的人员仍受租户范围约束。验证过期并发戳和固定已发布版本试发。
- [模板领域服务](../../src/IczpNet.Chat.Domain/MessageTemplates/MessageTemplateAdminManager.cs) · [模板管理服务](../../src/IczpNet.Chat.Admin.Application/MessageTemplates/MessageTemplateAppService.cs)

### AI 任务批量删除契约

- [ ] 从当前 Host 的 Swagger、ABP API 定义和 HTTP 请求确定批删的 HTTP 方法、路径和 ID 列表绑定。优先保留 ABP 默认生成的批删端点，对齐 Vben 封装及交付手册。
- **现状**　最新基线的 ABP 测试宿主仍生成 `DELETE /api/chat/admin/ai-run/many`，Vben 与手册仍写 `POST /api/chat/admin/ai-run/delete-many`。最新路由元数据确认参数是 `List<Guid> idList`，实际 HTTP 绑定由验收用例验证。
- **验收**　断言自动生成路由及参数来源。使用 Vben 批删两条允许删除的测试记录，核对请求参数和最终状态。覆盖空集合、超出批量上限、无删除权限及包含不可删除记录的场景。
- [后端 AI 管理服务](../../src/IczpNet.Chat.Admin.Application/AiRuns/AiRunAppService.cs) · [Vben AI API](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/api/chat/ai-run.ts)

### 聊天对象创建与类型一致性

- [ ] 明确通用创建支持的类型和 `ChatObjectTypeId` 与 `ObjectType` 的一致性。群聊使用 `RoomManager` 创建会话与系统成员单元。店员等有父级约束的类型调用其既有领域入口，或从通用创建中移除该类型。
- **现状**　Vben 可以选择群聊，Domain 通用创建只插入 `ChatObject`，并优先以 `ChatObjectTypeId` 解析对象类型。
- **验收**　成功创建的群聊有完整 Session 和系统会话单元。提交不一致的类型或缺少所需父级时应返回明确错误，数据库中不留下半成品。
- [聊天对象领域服务](../../src/IczpNet.Chat.Domain/ChatObjects/ChatObjectManager.cs) · [群领域服务](../../src/IczpNet.Chat.Domain/RoomSections/Rooms/RoomManager.cs) · [Vben 创建表单](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/views/chat/chat-object/modules/form-modal.vue)

## P2 功能与回归整改

| 待办 | 实施内容 | 验收 |
| --- | --- | --- |
| 聊天对象编辑字段 | 对齐 `categoryIds`、`isEnabled` 和 `displayName` 的表单与管理 DTO。分类用现有领域关系命令，启停沿用独立权限和静态对象保护，显示名按实体派生规则展示。清理无调用方的改名封装，或用专门命令保证改名保留原 Code 和 Description。 | 编辑后重查详情及列表。分类、启停和显示名符合页面承诺，改名保留现有资料。 [管理更新 DTO](../../src/IczpNet.Chat.Admin.Application.Contracts/ChatObjects/Dtos/ChatObjectAdminUpdateInput.cs) · [Vben 表单](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/views/chat/chat-object/modules/form-modal.vue) |
| 在线列表与批量处置 | 在 Domain 实现未选主机时的跨节点查询，使用有界分页与批量 Redis 读取处理指定节点或筛选结果。将批量处置与结果汇总下沉，显式限定节点范围，保留操作原因和结果。 | 用两个节点验证默认列表、节点筛选、空节点、分页和多设备重连。验证单节点处置无跨节点副作用。 [在线管理服务](../../src/IczpNet.Chat.Admin.Application/Online/OnlineAppService.cs) · [在线领域服务](../../src/IczpNet.Chat.Domain/ConnectionPools/OnlineManager.cs) |
| 文章与模板按钮权限 | 按后端验收的用途区分文章读取、发送、调度和撤回，统一模板试发按钮与后端端点的 policy。检查发送人及会话选择器依赖的读取权限和登录后 `grantedPolicies`。 | 按只读、发送、调度、撤回和模板试发分别授予最小权限，逐项检查菜单、按钮、选择器和真实 HTTP 行为。 [文章页](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/views/chat/article-message/index.vue) · [模板页](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/views/chat/message-template/index.vue) |
| Vben 契约回归 | 更新文章和模板的旧路径断言。find-id 按当前 ABP 契约使用 `POST` 和既有查询参数拼写，覆盖模板修订 `/revision?version=...`、发送请求包装、SQL bigint 精度及 AI 批删。 | Chat 页面定向 Vitest 全绿，且断言 HTTP 方法、路径、参数位置和请求体。 [文章测试](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/views/chat/article-message/article-draft.test.ts) · [模板测试](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/views/chat/message-template/template-message.test.ts) |

## 后续治理用例

发布整改通过后，按 [原开发计划](CHAT-ADMIN-DEVELOPMENT.md) 继续建设以下范围。已交付的列表和详情可以直接作为后续操作的入口。

- **群组与群主治理**　补独立群组与成员入口，按现有领域命令交付建群、邀请、管理员任免、群主转让或认领以及解散。覆盖群系统成员隔离、授权变更通知和重复请求。
- **入群及好友申请处置**　追加授权审批、拒绝及过期诊断。复用既有申请领域流程，验证跨群拒绝、并发审批一次生效和通知幂等。
- **群角色和组织授权**　补成员及组织角色绑定、授予和收回、有效权限及来源查询。验证内置角色保护和不同来源的授权合并。
- **群公告与回执**　补按会话范围授权的审核、发布、撤销、置顶、提醒与回执核对。验证版本和回执对应，以及定时发布和重试。
- **其余批次一至四的治理用例**　按原文档登记 Domain 入口、权限、证据、动作与复核结果。批次五继续使用交付手册的扩展顺序。

## 执行顺序与发布验收

1. **先开发后端。** 在 Domain 修复 P1 资源范围、状态保护和查询，按需要在 EF Core 实现范围内投影与分页。随后更新必需的管理 Contracts、权限本地化和薄 AppService。使用真实路由定义登记 HTTP 方法、请求参数、响应和权限。现有实体结构可先复用，涉及持久化变更时按仓库规则另行处理。
2. **后端验收通过后迁移 Vben。** 对照发布的 Swagger 和 `/api/abp/api-definition` 修改 API、按钮、选择器和回归测试，同步交付手册的契约与验收记录。管理 URL 保持在 `/api/chat/admin/...`。
3. **用真实角色走 HTTP 验收。** 使用启用真实授权的测试环境，通过 Vben 使用的网关路由验证未登录、无权限、只读、写操作权限、同租户范围外和启用租户后的跨租户场景。同步核对 ABP 权限授予和配置下发，以及响应、拒绝结果和数据库状态。附件权限、消息正文、任务处置和撤回应分别有负例。
4. **核对治理动作的可追溯性和重试。** 持久化审计记录操作者、租户、资源、动作、原因、关联标识和结果。敏感正文、凭据及私有存储键只在受控路径使用。调度、批量操作与撤回复核并发、重试和客户端可见结果。
5. **按后端、Vben 的顺序发布。** P1 全部关闭，与发布功能关联的 P2 回归通过。保留对应构建和测试输出、实际路由和角色矩阵，再按版本发布。旧客户端入口执行回归检查。

## 验证记录

交付报告记录了最新 Admin 测试 23 项通过、Chat Host 构建 0 警告 0 错误，以及 Vben 类型检查通过。[交付手册验证记录](CHAT-ADMIN-DELIVERY-HANDBOOK.md#5-自动化测试与构建验证记录)

| 验证项 | 最新基线独立复核 |
| --- | --- |
| 后端 Admin 与旧 API 路由 | 25 项通过，0 项失败，覆盖 23 项 Admin 测试和 2 项旧 API 路由回归。补齐前的同口径复核为 23 项通过。 |
| Chat Host 构建 | 通过，0 警告、0 错误。 |
| ABP 测试宿主路由 | 22 个管理服务生成 112 个 action，HTTP 方法与路径组合无重复。补齐前为 100 个 action。 |
| Vben 类型检查 | 通过，退出码 0。 |
| Vben Chat 单测 | 38 项中 33 项通过、5 项失败。文章测试失败 2 项，模板测试失败 3 项，旧路径与方法断言仍待更新。 |
| Vben 菜单 | 核对 19 个子路由和 6 组新增页面及 API 文件。 |

现有后端业务测试基类启用了 `AddAlwaysAllowAuthorization()`，真实角色、租户和目标资源的 HTTP 授权仍按上节验收。[后端测试基类](../../test/IczpNet.Chat.TestBase/ChatTestBaseModule.cs)

```powershell
Set-Location 'F:\Dev\abpvnext\IczpNet.Chat'
dotnet test 'test/IczpNet.Chat.Application.Tests/IczpNet.Chat.Application.Tests.csproj' --no-restore --filter 'FullyQualifiedName~IczpNet.Chat.Admin|FullyQualifiedName~MessageTemplateMvc_Tests|FullyQualifiedName~MessageChangesMvc_Tests'
dotnet build 'host/IczpNet.Chat.HttpApi.Host/IczpNet.Chat.HttpApi.Host.csproj' --no-restore

Set-Location 'F:\Dev\abpvnext\abp-admin-vben\vue-vben-admin\apps\web-antdv-next'
pnpm run typecheck
pnpm exec vitest run --config vitest.cms.config.ts src/views/chat
```
