using System;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.MessageReports;
using IczpNet.Chat.Admin.MessageReports.Dtos;
using IczpNet.Chat.Admin.Online;
using IczpNet.Chat.Admin.ScanCodes;
using IczpNet.Chat.Admin.ScanCodes.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.MessageReports;
using IczpNet.Chat.ScanCodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace IczpNet.Chat.Admin;

public class ChatRealtimeReportAdminAppService_Tests : ChatTestBase<ChatAdminApplicationTestModule>
{
    private readonly IScanCodeAdminAppService _scanCodeAppService;
    private readonly IMessageReportAdminAppService _messageReportAppService;

    public ChatRealtimeReportAdminAppService_Tests()
    {
        _scanCodeAppService = GetRequiredService<IScanCodeAdminAppService>();
        _messageReportAppService = GetRequiredService<IMessageReportAdminAppService>();
    }

    [Fact]
    public async Task ScanCode_admin_list_and_detail_work()
    {
        var repo = GetRequiredService<IRepository<ScanCode, Guid>>();
        var scanId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var entity = new ScanCode
            {
                Type = "QR_CODE",
                Content = "gotoim://room/12345",
                DeviceId = "dev-01",
                ClientId = "vben-admin",
                Execution = 12.5,
                HandlerCount = 1,
                ScanHandlerList =
                [
                    new ScanHandler
                    {
                        Action = "JoinRoom",
                        Handler = "RoomScanHandler",
                        HandlerFullName = "IczpNet.Chat.ScanCodes.RoomScanHandler",
                        Execution = 12.5,
                        Success = true,
                        Message = "OK",
                        Result = "{\"roomId\":12345}"
                    }
                ]
            };
            Volo.Abp.Domain.Entities.EntityHelper.TrySetId(entity, () => scanId);
            await repo.InsertAsync(entity, autoSave: true);
        });

        var list = await _scanCodeAppService.GetListAsync(new ScanCodeAdminGetListInput
        {
            Keyword = "gotoim://room/12345",
            Success = true
        });
        Assert.Equal(1, list.TotalCount);
        Assert.Single(list.Items[0].ScanHandlerList);

        var detail = await _scanCodeAppService.GetAsync(scanId);
        Assert.Equal("QR_CODE", detail.Type);
        Assert.True(detail.ScanHandlerList[0].IsSuccess);
    }

    [Fact]
    public async Task MessageReport_admin_options_list_and_summary_work()
    {
        var opts = await _messageReportAppService.GetOptionsAsync();
        Assert.NotNull(opts);

        var dayRepo = GetRequiredService<IMessageReportDayRepository>();
        var sessionId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            var report = new MessageReportDay
            {
                SessionId = sessionId,
                MessageType = MessageTypes.Text,
                DateBucket = 20261127,
                Count = 42
            };
            report.SetId(Guid.NewGuid());
            await dayRepo.InsertAsync(report, autoSave: true);
        });

        var list = await _messageReportAppService.GetListAsync(new MessageReportAdminGetListInput
        {
            ReportType = MessageReportTypes.Day,
            SessionId = sessionId
        });
        Assert.Equal(1, list.TotalCount);
        Assert.Equal(42, list.Items[0].Count);

        var summary = await _messageReportAppService.GetSummaryAsync(new MessageReportAdminGetListInput
        {
            ReportType = MessageReportTypes.Day,
            SessionId = sessionId
        });
        Assert.Equal(1, summary.TotalCount);
        Assert.Equal(42, summary.Items[0].TotalCount);
    }

    [Fact]
    public async Task Conventional_controllers_register_scan_report_and_online_routes()
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
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.ScanCodes.ScanCodeAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/scan-code");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.MessageReports.MessageReportAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/message-report");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.MessageReports.MessageReportAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/message-report/summary");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(OnlineAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/online");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(OnlineAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/online/hosts");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(OnlineAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/online/abort");
    }
}
