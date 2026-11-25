using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionOrganizationRoles;
using IczpNet.Chat.SessionOrganizations;
using IczpNet.Chat.SessionPermissionDefinitions;
using IczpNet.Chat.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.SessionRoles;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionServices;
using IczpNet.Chat.SessionUnitOrganizations;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Linq;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

public class GroupManagementStageABC_Tests : ChatTestBase<RoomManagementPoliciesTestModule>
{
    private static async Task InsertAsync<T>(ChatDbContext context, Dictionary<string, object> values)
    {
        var entity = context.Model.FindEntityType(typeof(T))!;
        var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
        var columns = new List<string>();
        var parameters = new List<object>();
        foreach (var property in entity.GetProperties())
        {
            if (property.GetComputedColumnSql() != null) continue;
            if (!values.TryGetValue(property.Name, out var value))
            {
                if (property.IsNullable) continue;
                var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                value = type == typeof(string) ? "" : type == typeof(Guid) ? Guid.NewGuid() :
                    type == typeof(DateTime) ? DateTime.UtcNow : Activator.CreateInstance(type);
            }
            var converter = property.GetTypeMapping().Converter;
            if (converter != null) value = converter.ConvertToProvider(value);
            columns.Add("\"" + property.GetColumnName(store) + "\"");
            parameters.Add(value ?? DBNull.Value);
        }
        await context.Database.ExecuteSqlRawAsync("INSERT INTO \"" + entity.GetTableName() + "\" (" +
            string.Join(",", columns) + ") VALUES (" + string.Join(",", Enumerable.Range(0, parameters.Count).Select(i => "{" + i + "}")) + ")",
            parameters.ToArray());
    }

    private SessionOrganiztionBySessionUnitAppService CreateOrgService()
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        var checker = GetRequiredService<ISessionPermissionChecker>();
        lazy.LazyGetRequiredService<ISessionUnitManager>().Returns(GetRequiredService<ISessionUnitManager>());
        lazy.LazyGetRequiredService<ISessionUnitRepository>().Returns(GetRequiredService<ISessionUnitRepository>());
        lazy.LazyGetRequiredService<IRepository<SessionUnitOrganization>>().Returns(GetRequiredService<IRepository<SessionUnitOrganization>>());
        lazy.LazyGetRequiredService<IRepository<SessionOrganizationRole>>().Returns(GetRequiredService<IRepository<SessionOrganizationRole>>());
        lazy.LazyGetRequiredService<IRepository<SessionRole, Guid>>().Returns(GetRequiredService<IRepository<SessionRole, Guid>>());
        lazy.LazyGetRequiredService<ISessionPermissionChecker>().Returns(checker);
        lazy.LazyGetRequiredService<IAsyncQueryableExecuter>().Returns(GetRequiredService<IAsyncQueryableExecuter>());
        lazy.LazyGetRequiredService<IUnitOfWorkManager>().Returns(GetRequiredService<IUnitOfWorkManager>());
        lazy.LazyGetRequiredService<ICurrentUser>().Returns(Substitute.For<ICurrentUser>());
        lazy.LazyGetService<ISessionPermissionChecker>().Returns(checker);

        return new SessionOrganiztionBySessionUnitAppService(
            GetRequiredService<IRepository<SessionOrganization, long>>(),
            GetRequiredService<ISessionOrganizationManager>())
        {
            LazyServiceProvider = lazy
        };
    }

    [Fact]
    public async Task Restricted_admin_template_and_org_role_dynamic_permissions_work()
    {
        var ids = await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            var roomId = 7100L;
            var creatorOwnerId = 7101L;
            var adminOwnerId = 7102L;
            var memberOwnerId = 7103L;
            var childMemberOwnerId = 7104L;
            var sessionId = Guid.NewGuid();
            var creatorUnitId = Guid.NewGuid();
            var adminUnitId = Guid.NewGuid();
            var memberUnitId = Guid.NewGuid();
            var childMemberUnitId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var adminRoleId = Guid.NewGuid();

            foreach (var (id, name, type) in new[]
            {
                (roomId, "StageC测试群", ChatObjectTypeEnums.Room),
                (creatorOwnerId, "群主", ChatObjectTypeEnums.Personal),
                (adminOwnerId, "管理员", ChatObjectTypeEnums.Personal),
                (memberOwnerId, "父组织成员", ChatObjectTypeEnums.Personal),
                (childMemberOwnerId, "子组织成员", ChatObjectTypeEnums.Personal),
            })
            {
                await InsertAsync<ChatObject>(context, new()
                {
                    ["Id"] = id, ["Name"] = name, ["IsEnabled"] = true, ["IsActive"] = true, ["ObjectType"] = type
                });
            }

            await InsertAsync<Session>(context, new()
            {
                ["Id"] = sessionId, ["OwnerId"] = roomId, ["Channel"] = Channels.RoomChannel, ["IsEnabled"] = true
            });

            foreach (var (uid, oid, isCreator, isAdmin) in new[]
            {
                (creatorUnitId, creatorOwnerId, true, false),
                (adminUnitId, adminOwnerId, false, true),
                (memberUnitId, memberOwnerId, false, false),
                (childMemberUnitId, childMemberOwnerId, false, false),
            })
            {
                await InsertAsync<SessionUnit>(context, new()
                {
                    ["Id"] = uid, ["SessionId"] = sessionId, ["OwnerId"] = oid,
                    ["DestinationId"] = roomId, ["DestinationObjectType"] = ChatObjectTypeEnums.Room,
                    ["IsAdmin"] = isAdmin
                });
                await InsertAsync<SessionUnitSetting>(context, new()
                {
                    ["SessionUnitId"] = uid, ["SessionId"] = sessionId,
                    ["IsCreator"] = isCreator, ["IsAdmin"] = isAdmin, ["IsEnabled"] = true, ["IsKilled"] = false
                });
            }

            await InsertAsync<SessionPermissionDefinition>(context, new()
            {
                ["Id"] = SessionPermissionDefinitionConsts.AnnouncementPermission.Publish, ["Name"] = "发布公告", ["IsEnabled"] = true
            });
            await InsertAsync<SessionPermissionDefinition>(context, new()
            {
                ["Id"] = SessionPermissionDefinitionConsts.SessionRolePermission.Create, ["Name"] = "创建角色", ["IsEnabled"] = true
            });
            await InsertAsync<SessionPermissionDefinition>(context, new()
            {
                ["Id"] = SessionPermissionDefinitionConsts.SessionOrganizationPermission.Update, ["Name"] = "更新组织", ["IsEnabled"] = true
            });

            var parentOrg = new SessionOrganization("研发部", sessionId, null);
            await context.SessionOrganization.AddAsync(parentOrg);
            await context.SaveChangesAsync();

            var childOrg = new SessionOrganization("前端组", sessionId, parentOrg.Id);
            await context.SessionOrganization.AddAsync(childOrg);
            await context.SaveChangesAsync();

            await InsertAsync<SessionRole>(context, new()
            {
                ["Id"] = adminRoleId, ["SessionId"] = sessionId, ["Name"] = "管理员", ["PermissionCount"] = 0
            });
            await InsertAsync<SessionUnitRoles.SessionUnitRole>(context, new()
            {
                ["SessionUnitId"] = adminUnitId, ["SessionRoleId"] = adminRoleId
            });

            await InsertAsync<SessionRole>(context, new()
            {
                ["Id"] = roleId, ["SessionId"] = sessionId, ["Name"] = "公告专员", ["PermissionCount"] = 1
            });
            await InsertAsync<SessionPermissionRoleGrant>(context, new()
            {
                ["DefinitionId"] = SessionPermissionDefinitionConsts.AnnouncementPermission.Publish,
                ["RoleId"] = roleId, ["Value"] = 1, ["IsEnabled"] = true
            });

            return (sessionId, creatorUnitId, adminUnitId, memberUnitId, childMemberUnitId, parentOrgId: parentOrg.Id, childOrgId: childOrg.Id, roleId);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var checker = GetRequiredService<ISessionPermissionChecker>();
            var unitRepo = GetRequiredService<ISessionUnitRepository>();
            var unitQuery = (await unitRepo.GetQueryableAsync())
                .Include(x => x.Setting)
                .Include(x => x.SessionUnitRoleList).ThenInclude(r => r.SessionRole);
            var creator = await unitQuery.FirstAsync(x => x.Id == ids.creatorUnitId);
            var admin = await unitQuery.FirstAsync(x => x.Id == ids.adminUnitId);
            var member = await unitQuery.FirstAsync(x => x.Id == ids.memberUnitId);
            var childMember = await unitQuery.FirstAsync(x => x.Id == ids.childMemberUnitId);

            // 1. 管理员受限模板：有发布公告权限，无创建角色权限；群主拥有全部权限
            Assert.True(await checker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Publish, creator));
            Assert.True(await checker.IsGrantedAsync(SessionPermissionDefinitionConsts.SessionRolePermission.Create, creator));
            Assert.True(await checker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Publish, admin));
            Assert.False(await checker.IsGrantedAsync(SessionPermissionDefinitionConsts.SessionRolePermission.Create, admin));

            // 2. 群主绑定父组织 -> 公告专员角色；并把成员加入父组织、子成员加入子组织
            var orgService = CreateOrgService();
            await orgService.SetOrganizationRolesAsync(new SetOrganizationRolesInput
            {
                SessionUnitId = ids.creatorUnitId,
                OrganizationId = ids.parentOrgId,
                RoleIdList = new List<Guid> { ids.roleId }
            });

            // 3. 防提权：管理员不能调整已绑定角色的父组织成员归属
            await Assert.ThrowsAsync<UserFriendlyException>(() => orgService.BatchSetMembersAsync(new BatchSetMemberOrganizationsInput
            {
                SessionUnitId = ids.adminUnitId,
                MemberSessionUnitIdList = new List<Guid> { ids.memberUnitId },
                OrganizationIdList = new List<long> { ids.parentOrgId },
                Mode = MemberOrganizationBatchMode.Add
            }));

            // 4. 群主将 member 加入父组织，childMember 加入子组织
            await orgService.BatchSetMembersAsync(new BatchSetMemberOrganizationsInput
            {
                SessionUnitId = ids.creatorUnitId,
                MemberSessionUnitIdList = new List<Guid> { ids.memberUnitId },
                OrganizationIdList = new List<long> { ids.parentOrgId },
                Mode = MemberOrganizationBatchMode.Add
            });
            await orgService.BatchSetMembersAsync(new BatchSetMemberOrganizationsInput
            {
                SessionUnitId = ids.creatorUnitId,
                MemberSessionUnitIdList = new List<Guid> { ids.childMemberUnitId },
                OrganizationIdList = new List<long> { ids.childOrgId },
                Mode = MemberOrganizationBatchMode.Add
            });

            // Replace 空列表校验 & Clear 非空校验
            await Assert.ThrowsAsync<UserFriendlyException>(() => orgService.BatchSetMembersAsync(new BatchSetMemberOrganizationsInput
            {
                SessionUnitId = ids.creatorUnitId,
                MemberSessionUnitIdList = new List<Guid> { ids.memberUnitId },
                OrganizationIdList = new List<long>(),
                Mode = MemberOrganizationBatchMode.Replace
            }));
            await Assert.ThrowsAsync<UserFriendlyException>(() => orgService.BatchSetMembersAsync(new BatchSetMemberOrganizationsInput
            {
                SessionUnitId = ids.creatorUnitId,
                MemberSessionUnitIdList = new List<Guid> { ids.memberUnitId },
                OrganizationIdList = new List<long> { ids.parentOrgId },
                Mode = MemberOrganizationBatchMode.Clear
            }));

            // 重新加载检查动态权限：父组织成员获得公告发布权限，子组织成员不向父组织继承
            Assert.True(await checker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Publish, member));
            Assert.False(await checker.IsGrantedAsync(SessionPermissionDefinitionConsts.AnnouncementPermission.Publish, childMember));

            // 权限来源明晰
            var sources = await orgService.GetMemberPermissionSourcesAsync(ids.creatorUnitId, ids.memberUnitId);
            var publishSource = sources.Permissions.FirstOrDefault(p => p.DefinitionId == SessionPermissionDefinitionConsts.AnnouncementPermission.Publish);
            Assert.NotNull(publishSource);
            Assert.Contains(publishSource.OrganizationRoleSources, s => s.Contains("研发部") && s.Contains("公告专员"));

            // 有成员或已绑定角色的组织不可删除
            var orgManager = GetRequiredService<ISessionOrganizationManager>();
            await Assert.ThrowsAsync<UserFriendlyException>(() => orgManager.DeleteAsync(ids.parentOrgId));
        });
    }
}
