using System;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.AiRuns;
using IczpNet.Chat.Admin.AiRuns.Dtos;
using IczpNet.Chat.Admin.Blobs;
using IczpNet.Chat.Admin.Blobs.Dtos;
using IczpNet.Chat.Admin.Security;
using IczpNet.Chat.Admin.Security.Dtos;
using IczpNet.Chat.AiRuns;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace IczpNet.Chat.Admin;

public class ChatOpsGovernanceAdminAppService_Tests : ChatTestBase<ChatAdminApplicationTestModule>
{
    private readonly IUrlSecurityRuleAdminAppService _ruleAppService;
    private readonly ISecurityReportAdminAppService _reportAppService;
    private readonly IAiRunAdminAppService _aiRunAppService;
    private readonly IBlobAdminAppService _blobAppService;

    public ChatOpsGovernanceAdminAppService_Tests()
    {
        _ruleAppService = GetRequiredService<IUrlSecurityRuleAdminAppService>();
        _reportAppService = GetRequiredService<ISecurityReportAdminAppService>();
        _aiRunAppService = GetRequiredService<IAiRunAdminAppService>();
        _blobAppService = GetRequiredService<IBlobAdminAppService>();
    }

    [Fact]
    public async Task UrlSecurityRule_and_SecurityReport_admin_workflow_works()
    {
        var rule = await _ruleAppService.CreateAsync(new UrlSecurityRuleAdminCreateInput
        {
            Host = "Phishing.Example.com",
            Level = UrlSecurityLevel.Blocked,
            Category = UrlRiskCategory.Phishing,
            Reason = "测试钓鱼域名",
            IsEnabled = true
        });
        Assert.Equal("phishing.example.com", rule.Host);

        var rules = await _ruleAppService.GetListAsync(new UrlSecurityRuleAdminGetListInput
        {
            Keyword = "phishing.example.com"
        });
        Assert.Equal(1, rules.TotalCount);

        var disabledRule = await _ruleAppService.SetEnabledAsync(rule.Id, new UrlSecurityRuleAdminSetEnabledInput
        {
            IsEnabled = false
        });
        Assert.False(disabledRule.IsEnabled);

        var reportRepo = GetRequiredService<IRepository<SecurityReport, Guid>>();
        var report = await WithUnitOfWorkAsync(() =>
            reportRepo.InsertAsync(
                new SecurityReport(Guid.NewGuid(), "url", "https://scam.example.org/login", "疑似诈骗"),
                autoSave: true));

        var handled = await _reportAppService.HandleAsync(report.Id, new SecurityReportAdminHandleInput
        {
            Status = 1,
            AuditRemark = "确认违规并拉黑",
            AddBlacklist = true
        });
        Assert.Equal(1, handled.Status);

        // Duplicate handling is blocked
        await Assert.ThrowsAsync<BusinessException>(() =>
            _reportAppService.HandleAsync(report.Id, new SecurityReportAdminHandleInput
            {
                Status = 2,
                AuditRemark = "重复处理"
            }));

        // Blacklist rule auto-created
        var blacklistRules = await _ruleAppService.GetListAsync(new UrlSecurityRuleAdminGetListInput
        {
            Keyword = "scam.example.org"
        });
        Assert.Equal(1, blacklistRules.TotalCount);

        await _ruleAppService.DeleteAsync(rule.Id);
    }

    [Fact]
    public async Task AiRun_admin_list_cancel_retry_and_delete_guard_work()
    {
        var aiRunManager = GetRequiredService<IAiRunManager>();
        var run = await aiRunManager.CreateAsync(
            sourceMessageId: 88001L,
            sessionId: Guid.NewGuid(),
            requesterSessionUnitId: Guid.NewGuid(),
            provider: "openai-test");

        var list = await _aiRunAppService.GetListAsync(new AiRunAdminGetListInput
        {
            SourceMessageId = 88001L
        });
        Assert.Equal(1, list.TotalCount);

        var cancelled = await _aiRunAppService.CancelAsync(run.Id, new AiRunAdminCancelInput { Reason = "管理端取消" });
        Assert.Equal(AiRunStatus.Cancelled, cancelled.Status);

        var retried = await _aiRunAppService.RetryAsync(run.Id);
        Assert.Equal(AiRunStatus.Queued, retried.Status);

        await _aiRunAppService.DeleteAsync(run.Id);
        var afterDelete = await _aiRunAppService.GetListAsync(new AiRunAdminGetListInput
        {
            SourceMessageId = 88001L
        });
        Assert.Equal(0, afterDelete.TotalCount);
    }

    [Fact]
    public async Task Blob_admin_update_and_delete_guards_work()
    {
        var blobRepo = GetRequiredService<IRepository<Blob, Guid>>();
        var staticBlob = await WithUnitOfWorkAsync(() =>
            blobRepo.InsertAsync(new Blob(Guid.NewGuid(), "chat-files")
            {
                Name = "static-logo.png",
                FileName = "static-logo.png",
                IsStatic = true,
                IsPublic = true
            }, autoSave: true));

        var referencedBlob = await WithUnitOfWorkAsync(() =>
            blobRepo.InsertAsync(new Blob(Guid.NewGuid(), "chat-files")
            {
                Name = "msg-photo.jpg",
                FileName = "msg-photo.jpg",
                RefCount = 1
            }, autoSave: true));

        var orphanBlob = await WithUnitOfWorkAsync(() =>
            blobRepo.InsertAsync(new Blob(Guid.NewGuid(), "public-assets")
            {
                Name = "orphan.txt",
                FileName = "orphan.txt",
                RefCount = 0
            }, autoSave: true));

        var list = await _blobAppService.GetListAsync(new BlobAdminGetListInput
        {
            Keyword = "orphan.txt"
        });
        Assert.Equal(1, list.TotalCount);

        var updated = await _blobAppService.UpdateAsync(orphanBlob.Id, new BlobAdminUpdateInput
        {
            FileName = "orphan-renamed.txt",
            IsPublic = true
        });
        Assert.Equal("orphan-renamed.txt", updated.FileName);
        Assert.True(updated.IsPublic);

        // Protected chat-files blob cannot be made public; static blob cannot clear IsStatic
        await Assert.ThrowsAsync<BusinessException>(() =>
            _blobAppService.UpdateAsync(referencedBlob.Id, new BlobAdminUpdateInput { IsPublic = true }));
        await Assert.ThrowsAsync<BusinessException>(() =>
            _blobAppService.UpdateAsync(staticBlob.Id, new BlobAdminUpdateInput { IsStatic = false }));

        await Assert.ThrowsAsync<BusinessException>(() => _blobAppService.DeleteAsync(staticBlob.Id));
        await Assert.ThrowsAsync<BusinessException>(() => _blobAppService.DeleteAsync(referencedBlob.Id));

        await _blobAppService.DeleteAsync(orphanBlob.Id);
    }

    [Fact]
    public async Task Conventional_controllers_register_security_airun_and_blob_routes()
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
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(UrlSecurityRuleAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/url-security-rule");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(SecurityReportAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/security-report");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.AiRuns.AiRunAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/ai-run");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.Blobs.BlobAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/blob");
    }
}
