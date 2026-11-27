using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.ChatObjectCategories;
using IczpNet.Chat.Admin.ChatObjectCategories.Dtos;
using IczpNet.Chat.Admin.ChatObjectTypes;
using IczpNet.Chat.Admin.ChatObjectTypes.Dtos;
using IczpNet.Chat.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace IczpNet.Chat.Admin;

public class ChatCategoryTypeAdminAppService_Tests : ChatTestBase<ChatAdminApplicationTestModule>
{
    private readonly IChatObjectTypeAdminAppService _typeAppService;
    private readonly IChatObjectCategoryAdminAppService _categoryAppService;

    public ChatCategoryTypeAdminAppService_Tests()
    {
        _typeAppService = GetRequiredService<IChatObjectTypeAdminAppService>();
        _categoryAppService = GetRequiredService<IChatObjectCategoryAdminAppService>();
    }

    [Fact]
    public async Task ChatObjectType_and_Category_admin_crud_and_delete_guards_work()
    {
        var createdType = await _typeAppService.CreateAsync(new ChatObjectTypeAdminCreateInput
        {
            Id = "CustomPartner",
            Name = "合作伙伴",
            ObjectType = ChatObjectTypeEnums.Customer,
            Description = "外部合作对象",
            MaxDepth = 2,
            IsHasChild = true
        });
        Assert.Equal("CustomPartner", createdType.Id);
        Assert.Equal("合作伙伴", createdType.Name);

        var typeList = await _typeAppService.GetListAsync(new ChatObjectTypeAdminGetListInput
        {
            Keyword = "CustomPartner"
        });
        Assert.Equal(1, typeList.TotalCount);

        var updatedType = await _typeAppService.UpdateAsync("CustomPartner", new ChatObjectTypeAdminUpdateInput
        {
            Name = "合作伙伴-更新",
            ObjectType = ChatObjectTypeEnums.Customer,
            Description = "已更新",
            MaxDepth = 3,
            IsHasChild = true
        });
        Assert.Equal("合作伙伴-更新", updatedType.Name);

        var rootCat = await _categoryAppService.CreateAsync(new ChatObjectCategoryAdminCreateInput
        {
            Name = "华东大区",
            ChatObjectTypeId = "CustomPartner",
            Sorting = 10,
            Description = "顶层分类"
        });
        Assert.Equal("华东大区", rootCat.Name);

        var childCat = await _categoryAppService.CreateAsync(new ChatObjectCategoryAdminCreateInput
        {
            Name = "上海分部",
            ChatObjectTypeId = "CustomPartner",
            ParentId = rootCat.Id,
            Sorting = 5
        });
        Assert.Equal(rootCat.Id, childCat.ParentId);

        // Parent category with child cannot be deleted
        await Assert.ThrowsAsync<BusinessException>(() => _categoryAppService.DeleteAsync(rootCat.Id));

        // Type referenced by category cannot be deleted
        await Assert.ThrowsAsync<BusinessException>(() => _typeAppService.DeleteAsync("CustomPartner"));

        var updatedChild = await _categoryAppService.UpdateAsync(childCat.Id, new ChatObjectCategoryAdminUpdateInput
        {
            Name = "上海浦东分部",
            ChatObjectTypeId = "CustomPartner",
            ParentId = rootCat.Id,
            Sorting = 6
        });
        Assert.Equal("上海浦东分部", updatedChild.Name);

        await _categoryAppService.DeleteAsync(childCat.Id);
        await _categoryAppService.DeleteAsync(rootCat.Id);
        await _typeAppService.DeleteAsync("CustomPartner");

        var afterDelete = await _typeAppService.GetListAsync(new ChatObjectTypeAdminGetListInput
        {
            Keyword = "CustomPartner"
        });
        Assert.Equal(0, afterDelete.TotalCount);
    }

    [Fact]
    public async Task Conventional_controllers_register_category_and_type_admin_routes()
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
            .ToList();

        var categoryEndpoints = allEndpoints
            .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType()
                == typeof(ChatObjectCategoryAppService))
            .ToList();

        Assert.Contains(categoryEndpoints, endpoint =>
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/chat-object-category" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("GET") == true);
        Assert.Contains(categoryEndpoints, endpoint =>
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/chat-object-category" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("POST") == true);
        Assert.Contains(categoryEndpoints, endpoint =>
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/chat-object-category/{id}" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("PUT") == true);
        Assert.Contains(categoryEndpoints, endpoint =>
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/chat-object-category/{id}" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("DELETE") == true);

        var typeEndpoints = allEndpoints
            .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType()
                == typeof(ChatObjectTypeAppService))
            .ToList();

        Assert.Contains(typeEndpoints, endpoint =>
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/chat-object-type" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("GET") == true);
        Assert.Contains(typeEndpoints, endpoint =>
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/chat-object-type/{id}" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("DELETE") == true);
    }
}
