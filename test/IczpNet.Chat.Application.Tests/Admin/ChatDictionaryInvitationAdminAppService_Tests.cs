using System;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.EntryNames;
using IczpNet.Chat.Admin.EntryNames.Dtos;
using IczpNet.Chat.Admin.EntryValues;
using IczpNet.Chat.Admin.EntryValues.Dtos;
using IczpNet.Chat.Admin.InvitationCodes;
using IczpNet.Chat.Admin.InvitationCodes.Dtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace IczpNet.Chat.Admin;

public class ChatDictionaryInvitationAdminAppService_Tests : ChatTestBase<ChatAdminApplicationTestModule>
{
    private readonly IEntryNameAdminAppService _entryNameAppService;
    private readonly IEntryValueAdminAppService _entryValueAppService;
    private readonly IInvitationCodeAdminAppService _invitationCodeAppService;

    public ChatDictionaryInvitationAdminAppService_Tests()
    {
        _entryNameAppService = GetRequiredService<IEntryNameAdminAppService>();
        _entryValueAppService = GetRequiredService<IEntryValueAdminAppService>();
        _invitationCodeAppService = GetRequiredService<IInvitationCodeAdminAppService>();
    }

    [Fact]
    public async Task EntryName_and_EntryValue_admin_crud_and_guards_work()
    {
        var rootName = await _entryNameAppService.CreateAsync(new EntryNameAdminCreateInput
        {
            Name = "联系方式",
            Code = "contact_info",
            InputType = "Text",
            Sorting = 10
        });
        Assert.Equal("联系方式", rootName.Name);

        var childName = await _entryNameAppService.CreateAsync(new EntryNameAdminCreateInput
        {
            Name = "微信",
            Code = "wechat_id",
            ParentId = rootName.Id,
            InputType = "Text",
            IsRequired = true,
            Sorting = 5
        });
        Assert.Equal(rootName.Id, childName.ParentId);

        var entryValue = await _entryValueAppService.CreateAsync(new EntryValueAdminCreateInput
        {
            EntryNameId = childName.Id,
            Value = "wx_sample_01",
            IsOption = true,
            IsPublic = true
        });
        Assert.Equal("微信", entryValue.Name);

        // Parent EntryName with child or EntryName with values cannot be deleted
        await Assert.ThrowsAsync<BusinessException>(() => _entryNameAppService.DeleteAsync(rootName.Id));
        await Assert.ThrowsAsync<BusinessException>(() => _entryNameAppService.DeleteAsync(childName.Id));

        var updatedValue = await _entryValueAppService.UpdateAsync(entryValue.Id, new EntryValueAdminUpdateInput
        {
            Value = "wx_sample_02",
            IsOption = false,
            IsPublic = true
        });
        Assert.Equal("wx_sample_02", updatedValue.Value);

        await _entryValueAppService.DeleteAsync(entryValue.Id);
        await _entryNameAppService.DeleteAsync(childName.Id);
        await _entryNameAppService.DeleteAsync(rootName.Id);
    }

    [Fact]
    public async Task InvitationCode_admin_crud_masks_list_code_and_toggles_status()
    {
        var created = await _invitationCodeAppService.CreateAsync(new InvitationCodeAdminCreateInput
        {
            Title = "VIP测试邀请码",
            ExpirationTime = DateTime.UtcNow.AddDays(7)
        });
        Assert.NotNull(created.Code);
        Assert.True(created.Code.Length >= 8);
        Assert.Contains("*", created.MaskedCode);
        Assert.NotEqual(created.Code, created.MaskedCode);
        Assert.True(created.IsEnabled);

        var list = await _invitationCodeAppService.GetListAsync(new InvitationCodeAdminGetListInput
        {
            Keyword = "VIP测试"
        });
        Assert.Equal(1, list.TotalCount);
        Assert.Contains("*", list.Items[0].MaskedCode);

        var disabled = await _invitationCodeAppService.SetIsEnabledAsync(created.Id, new InvitationCodeAdminSetIsEnabledInput
        {
            IsEnabled = false
        });
        Assert.False(disabled.IsEnabled);

        var updated = await _invitationCodeAppService.UpdateAsync(created.Id, new InvitationCodeAdminUpdateInput
        {
            Title = "VIP测试邀请码-已更新",
            IsEnabled = true
        });
        Assert.Equal("VIP测试邀请码-已更新", updated.Title);
        Assert.True(updated.IsEnabled);

        await _invitationCodeAppService.DeleteAsync(created.Id);
        var afterDelete = await _invitationCodeAppService.GetListAsync(new InvitationCodeAdminGetListInput
        {
            Keyword = "VIP测试"
        });
        Assert.Equal(0, afterDelete.TotalCount);
    }

    [Fact]
    public async Task Conventional_controllers_register_dictionary_and_invitation_routes()
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

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(EntryNameAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/entry-name" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("GET") == true);

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(EntryValueAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/entry-value" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("POST") == true);

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(InvitationCodeAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/invitation-code" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("GET") == true);

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(InvitationCodeAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/invitation-code/{id}/set-is-enabled" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("POST") == true);
    }
}
