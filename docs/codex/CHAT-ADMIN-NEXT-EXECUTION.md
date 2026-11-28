# ChatAdmin 第二轮代码复审与下一步执行计划

复审日期为 2026-11-27。**当前建议暂缓发布。** 文章与模板的前端回归已恢复，模板和定时任务已接入资源范围。下一步先修复消息资源授权、AI 批删参数绑定、申请审批的领域行为，以及新增测试的启动失败，再完成 Vben 与真实角色的联调。

## 复审基线和已确认进展

| 范围 | 复审基线 | 本轮确认 |
| --- | --- | --- |
| Chat 已提交代码 | `f78e7003`，本地 `master` 与 `github/master` 指向该提交 | 管理附件新增公开、解除静态保护及直接删除的拦截。模板和调度 AppService 将范围传入 Domain。消息详情新增 `Messages.GetContent`。AI 批删增加 100 个去重 ID 的上限。通用创建拒绝群聊、店员和不一致的类型，改名保留未传入的 Code、Description。 |
| Vben 已提交代码 | `faf7efee`，本地 `dev` 与 `origin/dev` 指向该提交 | AI 批删已切到 `DELETE /api/chat/admin/ai-run/many`。文章和模板的旧路径断言已更新，Chat 定向 Vitest 38 项通过。 |
| 两仓工作区改动 | 上述提交之上的未提交文件 | 后端新增全员禁言、申请审批、公告置顶及四项测试，Vben 已有这三项动作的页面接入。下文的当前工作区测试结果包括后端新增测试。 |

已有管理服务和页面映射以交付手册为基础。上一轮整改的验收要求仍适用，以下任务按本轮代码状态更新。 [上一轮整改计划](CHAT-ADMIN-REMEDIATION.md) · [交付手册](CHAT-ADMIN-DELIVERY-HANDBOOK.md)

## P1 发布阻塞与实施任务

### 消息正文与会话资源授权

**发现**　消息详情检查 `Messages.GetContent`，会话历史仍由 `Messages.GetList` 进入并直接填充 `Content`。档案列表和详情、历史、发送及撤回继续按请求提供的消息或会话单元查找。领域入口没有用操作者授权范围约束这些目标。

- [ ] 在 Domain 统一校验操作者、租户和会话或聊天对象范围。列表在计数、分页前过滤，详情、历史和撤回先校验目标。发送分别校验文章发送身份、模板使用权限和目标会话。
- [ ] 在所有消息正文出口使用同一授权口径。元数据和正文分开返回，已授权的会话历史继续遵循 `SessionUnit` 和 `DeletedRecorder` 规则。
- [ ] 用真实 ABP 策略验证只有列表权限时不会从历史取得正文。有正文权限且有会话范围时可以阅读；两个同租户但范围不同的操作者不能互查、冒用身份发送或撤回。启用多租户的场景加入跨租户拒绝用例，拒绝时核对数据库和推送状态。

[消息管理服务](../../src/IczpNet.Chat.Admin.Application/Messages/MessageAppService.cs) · [消息领域服务](../../src/IczpNet.Chat.Domain/MessageSections/Messages/MessageAdminManager.cs)

### AI 批删数组参数

**发现**　路由已对齐。当前 Axios 请求默认把数组写为 `idList[]=...`。本地 ASP.NET Core MVC HTTP 实测，同签名的 `List<Guid> idList` 在这种查询格式下绑定为 0 项，重复键 `idList=...&idList=...` 绑定为 2 项。后端遇到空列表直接返回，因此页面可能显示成功但记录仍在。

- [ ] 保留 ABP 自动删除端点，在 Vben 此请求采用现有 `paramsSerializer: 'repeat'`，明确后端从查询字符串绑定。对空集合给出显式业务错误，避免无效请求显示成功。
- [ ] 测试通过实际请求客户端发出两个 ID 并断言真实序列化结果，随后在 Chat ApiHost 通过 HTTP 断言删除后两条记录消失。覆盖空数组、非法 GUID、101 个有效 ID、重复 ID、运行中任务以及无删除权限。混有禁止删除项时验证整批结果符合现有规则。

[Vben AI API](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/api/chat/ai-run.ts) · [Vben 请求序列化](../../../abp-admin-vben/vue-vben-admin/packages/effects/request/src/request-client/request-client.ts) · [AI 任务领域服务](../../src/IczpNet.Chat.Domain/AiRuns/AiRunManager.cs)

### 入群和好友审批必须完成业务副作用

**发现**　工作区的 `HandleAdminAsync` 直接调用实体 `AgreeRequest` 或 `DisagreeRequest`，传入空处理会话单元，再保存记录。现有 `SessionRequestManager.HandleRequestAsync` 还负责验证容量及处理资格、建立好友或入群关系，并经现有流程通知。现在页面可显示通过，申请人仍未加入群或建立好友关系。

- [ ] 从可信操作者推导办理目标及其可用身份，校验目标群或联系人范围，并通过现有领域命令执行。提炼旧入口和管理入口共用的领域逻辑，保持事务、通知幂等和并发处理。
- [ ] 用完整实体关系验收好友和入群审批，核实申请状态、处理人、关系及会话单元、通知和审计。覆盖拒绝、过期、重复、并发审批，以及持有操作权限但无目标群范围的请求。

[管理审批领域入口](../../src/IczpNet.Chat.Domain/SessionRequests/SessionRequestAdminManager.cs) · [现有审批流程](../../src/IczpNet.Chat.Domain/SessionRequests/SessionRequestManager.cs)

### 新增测试必须进入断言并覆盖真实授权

**发现**　定向后端测试 25 项通过、4 项失败。四项失败来自同一测试类的构造器，测试宿主解析 `IArticleScheduleAdminManager` 时缺少 `IArticleScheduleAuthorization`，各用例尚未运行到业务断言。Chat ApiHost 和 Worker 已组合包含现有实现的 Chat Application 模块。当前测试基类启用了 `AddAlwaysAllowAuthorization()`。

- [ ] 在测试宿主接入现有授权实现或注册可控的测试实现，将调度依赖从其他场景的共用构造器移到对应测试。保持 Admin Application 生产项目的依赖方向。
- [ ] 先逐项复跑新增测试，再增加不使用 AlwaysAllow 的 HTTP 测试。使用真实策略验证未登录、已登录无策略、具备策略但范围外和具备策略且范围内。验证在拒绝时写入和业务副作用均未发生。
- [ ] 业务夹具建立有效的实体关系，审批测试断言实际入群或好友关系，调度测试增加跨范围改期、取消和立即执行。附件测试覆盖公开、解除静态保护和直接删除的拒绝分支，核对普通入口保护一致性。

[新增负例与动作测试](../../test/IczpNet.Chat.Application.Tests/Admin/ChatAdminAuthorizationNegative_Tests.cs) · [测试基础模块](../../test/IczpNet.Chat.TestBase/ChatTestBaseModule.cs) · [现有调度授权实现](../../src/IczpNet.Chat.Application/MessageServices/ArticleScheduleAuthorization.cs)

### 会话治理新动作按目标和动作授权

**发现**　会话全员禁言沿用会话启停权限，且按 ID 取会话后直接写入，未限定房间类型和操作者的目标范围。公告置顶只校验通用 Update 权限，缺少旧入口已有的会话权限检查与会话锁。新增入口需要明确适用范围和并发语义后才可发布。

- [ ] 为全员禁言和置顶分别定义动作权限，核对父级权限、租户和目标会话。全员禁言只适用于房间会话，公告置顶提炼共享的范围校验、会话锁和写入流程。
- [ ] 验证只有启停权限的角色无法切换禁言，单聊不可禁言，范围外公告不可置顶。验证同一会话内并发置顶与取消置顶、排序、重试以及持久审计。Vben 按服务端的实际动作策略展示按钮。

[会话管理服务](../../src/IczpNet.Chat.Admin.Application/Sessions/SessionAppService.cs) · [会话领域服务](../../src/IczpNet.Chat.Domain/Sessions/SessionAdminManager.cs) · [公告管理领域服务](../../src/IczpNet.Chat.Domain/SessionSections/Announcements/SessionAnnouncementAdminManager.cs) · [公告原有权限与锁](../../src/IczpNet.Chat.Application/SessionSections/Announcements/SessionAnnouncementAppService.cs)

## P2 沿用的整改与验收

| 事项 | 进展和下一步 |
| --- | --- |
| 附件保护共用 | 管理入口已增加保护，普通入口仍维护另一处判定。收敛至 Domain 并验证临时、已绑定、派生件、静态附件。补并发绑定和删除回归。 [管理附件](../../src/IczpNet.Chat.Domain/Blobs/BlobManager.cs) · [普通附件](../../src/IczpNet.Chat.Application/Blobs/BlobAppService.cs) |
| 模板与调度范围 | 保留已完成的租户及所有者过滤。管理 Domain 入口当前允许传空范围，核查可绕过范围的直接调用，管理调用使用显式范围。明确文章 `Manage` 的全量范围语义，并按授权账号复核可用模板列表、修订、试发、任务创建与处置。 [模板范围](../../src/IczpNet.Chat.Domain/MessageTemplates/MessageTemplateAdminManager.cs) · [调度范围](../../src/IczpNet.Chat.Domain/MessageSections/ArticleScheduleAdminManager.cs) |
| 聊天对象表单 | 后端已阻止无完整会话的群聊和无父级的店员，当前工作区已从对象类型下拉隐藏两项。让 TypeID 与对象类型选项联动，并对齐分类、启停和显示名的实际可写契约。验证普通创建、空值清理、单独改名、静态对象限制及改后重查。 [Vben 表单](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/views/chat/chat-object/modules/form-modal.vue) · [管理更新 DTO](../../src/IczpNet.Chat.Admin.Application.Contracts/ChatObjects/Dtos/ChatObjectAdminUpdateInput.cs) |
| 在线管理 | 无主机筛选时仍查空主机键，加载顺序仍是先取连接明细再分页。将跨节点查询和有界分页、批量处置下沉到 Domain，以双节点数据验证默认列表、节点筛选、分页和多设备。 [在线管理](../../src/IczpNet.Chat.Admin.Application/Online/OnlineAppService.cs) |
| 文章与模板按钮 | 文章页仍以 `ArticleMessages.Manage` 判断多个写操作，模板按钮仍按 `MessageTemplates.Use`。随消息授权整改统一发送、调度、撤回和试发 policy，按最小权限联调选择器、菜单与按钮。 [文章页](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/views/chat/article-message/index.vue) · [模板页](../../../abp-admin-vben/vue-vben-admin/apps/web-antdv-next/src/views/chat/message-template/index.vue) |

## 执行顺序和发布证据

1. **先修后端并跑通新增测试。** 资源范围、状态校验和副作用在 `IczpNet.Chat.Domain` 实现。仓储负责查询和投影，需要的新 DTO、权限和本地化写入 `IczpNet.Chat.Admin.Application.Contracts`。管理 AppService 由 `IczpNet.Chat.Admin.Application` 实现并委托 Domain。
2. **在测试环境验证自动 API。** 保持 Chat Host 的 `RootPath = "chat/admin"` 和 `RemoteServiceName = "ChatAdmin"`，保存运行中的 Swagger、ABP API 定义、HTTP 实测请求和角色结果。AI 批删验证绑定后的 ID 和数据库结果。审批、禁言、置顶和撤回验证持久审计及客户端状态。
3. **再完成 Vben 整改。** 按实际契约接入 `/api/chat/admin/...`，完成网络参数、返回状态和按钮权限测试。沿用 Vben 真实网关，验证无旧 Chat 管理路由回退。
4. **记录通过证据再发布。** P1 均有正反例通过，后端定向测试和前端回归全绿。先发后端，再发 Vben，并保留可回退的配套版本。群组与群主、组织角色及公告剩余治理用例继承原计划，在本轮发布验收后推进。 [ABP 开发规则](../../AGENTS.md) · [完整管理功能计划](CHAT-ADMIN-DEVELOPMENT.md)

## 本轮验证

| 项目 | 结果 |
| --- | --- |
| 后端定向测试 | 29 项中 25 项通过、4 项失败。筛选包含 Admin 和两项旧 API 路由回归。四项新增测试均在依赖解析阶段失败。 |
| Chat ApiHost 构建 | 通过。首轮构建 0 错误，警告含既有 XML 注释、Mapperly 映射和文件复制重试。测试结束后串行增量复跑为 0 警告、0 错误。 |
| ABP 实际路由元数据 | 当前工作区生成 22 个管理服务、115 个 action。确认 AI 批删及三个新增治理动作均注册在 `/api/chat/admin/...`。 |
| Vben 类型检查 | 通过。 |
| Vben Chat 定向 Vitest | 4 个测试文件、38 项通过。 |
| AI 数组绑定 | 本地独立 ASP.NET Core MVC HTTP 探针复现默认方括号键绑定为空，重复键绑定到两个 GUID。生产 Chat API 端到端验收待完成。 |

```powershell
Set-Location 'F:\Dev\abpvnext\IczpNet.Chat'
dotnet test 'test/IczpNet.Chat.Application.Tests/IczpNet.Chat.Application.Tests.csproj' --no-restore --filter 'FullyQualifiedName~ChatAdminAuthorizationNegative_Tests' --logger 'console;verbosity=normal'
dotnet test 'test/IczpNet.Chat.Application.Tests/IczpNet.Chat.Application.Tests.csproj' --no-restore --filter 'FullyQualifiedName~IczpNet.Chat.Admin|FullyQualifiedName~MessageTemplateMvc_Tests|FullyQualifiedName~MessageChangesMvc_Tests'
dotnet build 'host/IczpNet.Chat.HttpApi.Host/IczpNet.Chat.HttpApi.Host.csproj' --no-restore

Set-Location 'F:\Dev\abpvnext\abp-admin-vben\vue-vben-admin\apps\web-antdv-next'
pnpm run typecheck
pnpm exec vitest run --config vitest.cms.config.ts src/views/chat
```
