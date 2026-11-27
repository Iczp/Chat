using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.ChatObjects;
using IczpNet.Chat.Admin.ChatObjects.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.ChatObjectCategories;
using IczpNet.Chat.ChatObjectCategoryUnits;
using IczpNet.Chat.ChatObjectEntryValues;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.EntryNames;
using IczpNet.Chat.EntryValues;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Entities;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Xunit;

namespace IczpNet.Chat.Admin;

[DependsOn(
    typeof(ChatAdminApplicationModule),
    typeof(RoomManagementPoliciesTestModule)
)]
public class ChatAdminApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.GetConfiguration()["Chat:TemplateMessages:Enabled"] = "true";

        // ExposedBlobAppService in BlobAppServiceProtection_Tests is private without [DisableConventionalRegistration],
        // which breaks Castle DynamicProxy when AbpDddApplicationModule registers interceptors on test assembly services.
        for (var i = context.Services.Count - 1; i >= 0; i--)
        {
            var implType = context.Services[i].ImplementationType;
            if (implType != null && implType.IsNestedPrivate && implType.Assembly == typeof(ChatAdminApplicationTestModule).Assembly)
            {
                context.Services.RemoveAt(i);
            }
        }

        context.Services.AddTransient<IArticleScheduleAuthorization>(_ => new TestArticleScheduleAuthorization());
    }

    private sealed class TestArticleScheduleAuthorization : IArticleScheduleAuthorization
    {
        public Task<ClaimsPrincipal> AuthorizeAsync(Guid userId, long ownerId) =>
            Task.FromResult(new ClaimsPrincipal());
    }
}

[DependsOn(typeof(AbpAspNetCoreMvcModule))]
public class ChatAdminMvcTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(ChatAdminApplicationModule).Assembly, config =>
            {
                config.RootPath = ChatAdminRemoteServiceConsts.RootPath;
                config.RemoteServiceName = ChatAdminRemoteServiceConsts.RemoteServiceName;
            });
        });
    }
}

public class ChatObjectAdminAppService_Tests : ChatTestBase<ChatAdminApplicationTestModule>
{
    private readonly IChatObjectAdminAppService _chatObjectAdminAppService;
    private readonly IStaticPermissionDefinitionStore _staticPermissionDefinitionStore;

    public ChatObjectAdminAppService_Tests()
    {
        _chatObjectAdminAppService = GetRequiredService<IChatObjectAdminAppService>();
        _staticPermissionDefinitionStore = GetRequiredService<IStaticPermissionDefinitionStore>();
    }

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

        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO \"" + entity.GetTableName() + "\" (" +
            string.Join(",", columns) + ") VALUES (" +
            string.Join(",", Enumerable.Range(0, parameters.Count).Select(i => "{" + i + "}")) + ")",
            parameters.ToArray());
    }

    [Fact]
    public async Task Permissions_are_registered_in_definition_manager()
    {
        var groups = await _staticPermissionDefinitionStore.GetGroupsAsync();
        var group = groups.FirstOrDefault(x => x.Name == ChatAdminPermissions.GroupName);
        Assert.NotNull(group);

        Assert.NotNull(await _staticPermissionDefinitionStore.GetOrNullAsync(ChatAdminPermissions.ChatObjects.Default));
        Assert.NotNull(await _staticPermissionDefinitionStore.GetOrNullAsync(ChatAdminPermissions.ChatObjects.GetList));
        Assert.NotNull(await _staticPermissionDefinitionStore.GetOrNullAsync(ChatAdminPermissions.ChatObjects.GetDetail));
        Assert.NotNull(await _staticPermissionDefinitionStore.GetOrNullAsync(ChatAdminPermissions.ChatObjects.Update));
        Assert.NotNull(await _staticPermissionDefinitionStore.GetOrNullAsync(ChatAdminPermissions.ChatObjects.SetVerificationMethod));
        Assert.NotNull(await _staticPermissionDefinitionStore.GetOrNullAsync(ChatAdminPermissions.ChatObjects.SetIsEnabled));
        Assert.NotNull(await _staticPermissionDefinitionStore.GetOrNullAsync(ChatAdminPermissions.ChatObjectCategories.Default));
        Assert.NotNull(await _staticPermissionDefinitionStore.GetOrNullAsync(ChatAdminPermissions.ChatObjectTypes.Default));
    }

    [Fact]
    public async Task GetListAsync_returns_empty_when_no_data()
    {
        var result = await _chatObjectAdminAppService.GetListAsync(new ChatObjectAdminGetListInput
        {
            Keyword = "nonexistent-keyword-xyz"
        });

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetListAsync_and_GetAsync_filter_page_and_load_detail_correctly()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var rootCategoryId = Guid.NewGuid();
        var childCategoryId = Guid.NewGuid();
        var entryNameId = Guid.NewGuid();
        var entryValueId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await InsertAsync<ChatObjectType>(context, new Dictionary<string, object>
            {
                [nameof(ChatObjectType.Id)] = "Personal",
                [nameof(ChatObjectType.Name)] = "个人",
                [nameof(ChatObjectType.ObjectType)] = ChatObjectTypeEnums.Personal
            });

            await InsertAsync<ChatObjectCategory>(context, new Dictionary<string, object>
            {
                [nameof(ChatObjectCategory.Id)] = rootCategoryId,
                [nameof(ChatObjectCategory.Name)] = "VIP根分类",
                [nameof(ChatObjectCategory.ChatObjectTypeId)] = "Personal",
                [nameof(ChatObjectCategory.Depth)] = 0,
                [nameof(ChatObjectCategory.FullPath)] = rootCategoryId.ToString(),
                [nameof(ChatObjectCategory.FullPathName)] = "VIP根分类"
            });

            await InsertAsync<ChatObjectCategory>(context, new Dictionary<string, object>
            {
                [nameof(ChatObjectCategory.Id)] = childCategoryId,
                [nameof(ChatObjectCategory.ParentId)] = rootCategoryId,
                [nameof(ChatObjectCategory.Name)] = "SVIP子分类",
                [nameof(ChatObjectCategory.ChatObjectTypeId)] = "Personal",
                [nameof(ChatObjectCategory.Depth)] = 1,
                [nameof(ChatObjectCategory.FullPath)] = $"{rootCategoryId}/{childCategoryId}",
                [nameof(ChatObjectCategory.FullPathName)] = "VIP根分类/SVIP子分类"
            });

            await InsertAsync<EntryName>(context, new Dictionary<string, object>
            {
                [nameof(EntryName.Id)] = entryNameId,
                [nameof(EntryName.Name)] = "手机号",
                [nameof(EntryName.Code)] = "phone",
                [nameof(EntryName.InputType)] = "Text"
            });

            await InsertAsync<EntryValue>(context, new Dictionary<string, object>
            {
                [nameof(EntryValue.Id)] = entryValueId,
                [nameof(EntryValue.EntryNameId)] = entryNameId,
                [nameof(EntryValue.Value)] = "13800138000"
            });

            await InsertAsync<ChatObject>(context, new Dictionary<string, object>
            {
                [nameof(ChatObject.Id)] = 9001L,
                [nameof(ChatObject.Name)] = "管理员测试账号A",
                [nameof(ChatObject.NameSpelling)] = "guanliyuanceshizhanghaoA",
                [nameof(ChatObject.NameSpellingAbbreviation)] = "glycszhA",
                [nameof(ChatObject.Code)] = "admin_test_a",
                [nameof(ChatObject.AppUserId)] = userId1,
                [nameof(ChatObject.ChatObjectTypeId)] = "Personal",
                [nameof(ChatObject.ObjectType)] = ChatObjectTypeEnums.Personal,
                [nameof(ChatObject.IsEnabled)] = true,
                [nameof(ChatObject.IsActive)] = true,
                [nameof(ChatObject.IsPublic)] = true,
                [nameof(ChatObject.CreationTime)] = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            });

            await InsertAsync<ChatObject>(context, new Dictionary<string, object>
            {
                [nameof(ChatObject.Id)] = 9002L,
                [nameof(ChatObject.Name)] = "管理员测试账号B",
                [nameof(ChatObject.NameSpelling)] = "guanliyuanceshizhanghaoB",
                [nameof(ChatObject.NameSpellingAbbreviation)] = "glycszhB",
                [nameof(ChatObject.Code)] = "admin_test_b",
                [nameof(ChatObject.AppUserId)] = userId1,
                [nameof(ChatObject.ChatObjectTypeId)] = "Personal",
                [nameof(ChatObject.ObjectType)] = ChatObjectTypeEnums.Personal,
                [nameof(ChatObject.IsEnabled)] = false,
                [nameof(ChatObject.IsActive)] = false,
                [nameof(ChatObject.IsPublic)] = false,
                [nameof(ChatObject.CreationTime)] = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc)
            });

            await InsertAsync<ChatObject>(context, new Dictionary<string, object>
            {
                [nameof(ChatObject.Id)] = 9003L,
                [nameof(ChatObject.Name)] = "普通群聊C",
                [nameof(ChatObject.NameSpelling)] = "putongqunliaoC",
                [nameof(ChatObject.NameSpellingAbbreviation)] = "ptqlC",
                [nameof(ChatObject.Code)] = "room_test_c",
                [nameof(ChatObject.AppUserId)] = userId2,
                [nameof(ChatObject.ChatObjectTypeId)] = "Room",
                [nameof(ChatObject.ObjectType)] = ChatObjectTypeEnums.Room,
                [nameof(ChatObject.IsEnabled)] = true,
                [nameof(ChatObject.IsActive)] = true,
                [nameof(ChatObject.CreationTime)] = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc)
            });

            await InsertAsync<ChatObjectCategoryUnit>(context, new Dictionary<string, object>
            {
                [nameof(ChatObjectCategoryUnit.ChatObjectId)] = 9001L,
                [nameof(ChatObjectCategoryUnit.CategoryId)] = childCategoryId
            });

            await InsertAsync<ChatObjectEntryValue>(context, new Dictionary<string, object>
            {
                [nameof(ChatObjectEntryValue.OwnerId)] = 9001L,
                [nameof(ChatObjectEntryValue.EntryValueId)] = entryValueId
            });
        });

        // Keyword + IsEnabled filter
        var enabledList = await _chatObjectAdminAppService.GetListAsync(new ChatObjectAdminGetListInput
        {
            Keyword = "管理员测试",
            IsEnabled = true,
            SkipCount = 0,
            MaxResultCount = 10
        });
        Assert.Equal(1, enabledList.TotalCount);
        Assert.Single(enabledList.Items);
        Assert.Equal(9001L, enabledList.Items[0].Id);
        Assert.Equal("admin_test_a", enabledList.Items[0].Code);

        // AppUserId + Sorting + Paging
        var userObjects = await _chatObjectAdminAppService.GetListAsync(new ChatObjectAdminGetListInput
        {
            AppUserId = userId1,
            Sorting = "id desc",
            SkipCount = 0,
            MaxResultCount = 1
        });
        Assert.Equal(2, userObjects.TotalCount);
        Assert.Single(userObjects.Items);
        Assert.Equal(9002L, userObjects.Items[0].Id);

        // IncludeChildCategories
        var byRootCategory = await _chatObjectAdminAppService.GetListAsync(new ChatObjectAdminGetListInput
        {
            CategoryIdList = [rootCategoryId],
            IncludeChildCategories = true
        });
        Assert.Equal(1, byRootCategory.TotalCount);
        Assert.Equal(9001L, byRootCategory.Items[0].Id);

        // Detail
        var detail = await _chatObjectAdminAppService.GetAsync(9001L);
        Assert.Equal(9001L, detail.Id);
        Assert.Equal("管理员测试账号A", detail.Name);
        Assert.Equal(2, detail.SameUserChatObjectCount);
        Assert.Contains(childCategoryId, detail.CategoryIds);
        Assert.Contains("SVIP子分类", detail.CategoryNames);
        Assert.Single(detail.EntryValues);
        Assert.Equal("手机号", detail.EntryValues[0].EntryName);
        Assert.Equal("13800138000", detail.EntryValues[0].Value);

        // Not found
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _chatObjectAdminAppService.GetAsync(9999999L));

        // Update + SetVerificationMethod + SetIsEnabled
        var updated = await _chatObjectAdminAppService.UpdateAsync(9001L, new ChatObjectAdminUpdateInput
        {
            Name = "管理员测试账号A-已更新",
            Code = "admin_test_a_updated",
            Gender = Genders.Male,
            Description = "更新后的说明",
            IsPublic = false
        });
        Assert.Equal("管理员测试账号A-已更新", updated.Name);
        Assert.Equal("admin_test_a_updated", updated.Code);
        Assert.False(updated.IsPublic);

        var verified = await _chatObjectAdminAppService.SetVerificationMethodAsync(9001L, new ChatObjectAdminSetVerificationMethodInput
        {
            VerificationMethod = VerificationMethods.Required
        });
        Assert.Equal(VerificationMethods.Required, verified.VerificationMethod);

        var disabled = await _chatObjectAdminAppService.SetIsEnabledAsync(9001L, new ChatObjectAdminSetIsEnabledInput
        {
            IsEnabled = false
        });
        Assert.False(disabled.IsEnabled);
    }

    [Fact]
    public async Task SetIsEnabledAsync_rejects_disabling_static_chat_object()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await InsertAsync<ChatObject>(context, new Dictionary<string, object>
            {
                [nameof(ChatObject.Id)] = 9050L,
                [nameof(ChatObject.Name)] = "系统固有对象",
                [nameof(ChatObject.Code)] = "static_sys_obj",
                [nameof(ChatObject.ChatObjectTypeId)] = "Robot",
                [nameof(ChatObject.ObjectType)] = ChatObjectTypeEnums.Robot,
                [nameof(ChatObject.IsStatic)] = true,
                [nameof(ChatObject.IsEnabled)] = true,
                [nameof(ChatObject.CreationTime)] = DateTime.UtcNow
            });
        });

        await Assert.ThrowsAsync<BusinessException>(() =>
            _chatObjectAdminAppService.SetIsEnabledAsync(9050L, new ChatObjectAdminSetIsEnabledInput
            {
                IsEnabled = false
            }));
    }

    [Fact]
    public async Task Conventional_controllers_register_chat_admin_routes()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        await builder.AddApplicationAsync<ChatAdminMvcTestModule>();
        await using var app = builder.Build();
        await app.InitializeApplicationAsync();
        app.MapControllers();

        var allEndpoints = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType()
                == typeof(IczpNet.Chat.Admin.ChatObjects.ChatObjectAppService))
            .ToList();

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/chat-object" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("GET") == true);

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/chat-object/{id}" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("GET") == true);

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/chat-object/{id}" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("PUT") == true);

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/chat-object/{id}/set-verification-method" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("POST") == true);

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/chat-object/{id}/set-is-enabled" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("POST") == true);
    }
}
