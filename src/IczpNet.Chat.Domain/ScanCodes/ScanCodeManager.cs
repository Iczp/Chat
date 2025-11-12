using IczpNet.AbpCommons;
using IczpNet.Chat.Devices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Clients;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace IczpNet.Chat.ScanCodes;

public class ScanCodeManager(
    ICurrentClient currentClient,
    ICurrentUser currentUser,
    IDeviceResolver deviceResolver,
    IRepository<ScanCode, Guid> repository,
    IEnumerable<IScanCodeHandler> scanCodeHandlers) : DomainService, IScanCodeManager
{
    public ICurrentClient CurrentClient { get; } = currentClient;
    public ICurrentUser CurrentUser { get; } = currentUser;
    public IDeviceResolver DeviceResolver { get; } = deviceResolver;
    public IRepository<ScanCode, Guid> Repository { get; } = repository;
    public IEnumerable<IScanCodeHandler> ScanCodeHandlers { get; } = scanCodeHandlers;


    protected virtual async Task<ScanCode> BuildScanCodeAsync(string type, string content)
    {
        await Task.Yield();

        Assert.If(string.IsNullOrWhiteSpace(type), $"{nameof(type)}不能为空");

        Assert.If(string.IsNullOrWhiteSpace(content), $"{nameof(content)}不能为空");

        var scanCode = new ScanCode()
        {
            Type = type,
            Content = content,
            ClientId = CurrentClient.Id,
            UserId = CurrentUser.Id,
            DeviceId = await DeviceResolver.GetDeviceIdAsync()
        };

        return scanCode;

    }
    public async Task<ScanCode> ScanCodeAsync(string type, string code)
    {
        var scanCode = await BuildScanCodeAsync(type, code);

        scanCode.HandlerCount = ScanCodeHandlers.Count();

        var handlerTasks = ScanCodeHandlers.Select(async handler =>
        {
            var sw = Stopwatch.StartNew();

            var hander = new ScanHandler()
            {
                Handler = handler.GetType().Name,
                HandlerFullName = handler.GetType().FullName,
            };

            try
            {
                var result = await handler.HandleAsync(scanCode);
                hander.Action = result.Action;
                hander.Success = true;
                hander.Execution = sw.Elapsed.TotalMilliseconds;
                hander.Result = result.Data;
                hander.Message = result.Message;
                sw.Stop();
            }
            catch (Exception ex)
            {
                sw.Stop();
                hander.Execution = sw.Elapsed.TotalMilliseconds;
                hander.Success = false;
                hander.Message = ex.Message;
                hander.Result = ex.StackTrace;
                Logger.LogError($"处理器 {hander.HandlerFullName} 异常", ex);
            }
            scanCode.ScanHandlerList.Add(hander);
        });
        await Task.WhenAll(handlerTasks);

        scanCode.Execution = scanCode.ScanHandlerList.Sum(x => x.Execution);

        return await Repository.InsertAsync(scanCode, autoSave: true);
    }
}
