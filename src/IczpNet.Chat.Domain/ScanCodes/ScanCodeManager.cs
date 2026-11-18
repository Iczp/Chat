using IczpNet.AbpCommons;
using IczpNet.Chat;
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
    IDeviceContext deviceContext,
    IRepository<ScanCode, Guid> repository,
    IEnumerable<IScanCodeHandler> scanCodeHandlers) : DomainService, IScanCodeManager
{
    public const string CurrentChatObjectIdProperty = "CurrentChatObjectId";

    public ICurrentClient CurrentClient { get; } = currentClient;
    public ICurrentUser CurrentUser { get; } = currentUser;
    public IDeviceContext DeviceContext { get; } = deviceContext;
    public IRepository<ScanCode, Guid> Repository { get; } = repository;
    public IEnumerable<IScanCodeHandler> ScanCodeHandlers { get; } = scanCodeHandlers;


    protected virtual async Task<ScanCode> BuildScanCodeAsync(string type, string content, long? currentChatObjectId = null)
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
            DeviceId = await DeviceContext.GetDeviceIdAsync()
        };

        if (currentChatObjectId.HasValue)
        {
            scanCode.ExtraProperties[CurrentChatObjectIdProperty] = currentChatObjectId.Value;
        }

        return scanCode;

    }
    public async Task<ScanCode> ScanCodeAsync(string type, string code, long? currentChatObjectId = null)
    {
        var scanCode = await BuildScanCodeAsync(type, code, currentChatObjectId);

        var handlers = ScanCodeHandlers.ToList();
        scanCode.HandlerCount = handlers.Count;

        foreach (var handler in handlers)
        {
            var sw = Stopwatch.StartNew();

            var hander = new ScanHandler()
            {
                Handler = handler.GetType().Name,
                HandlerFullName = handler.GetType().FullName,
            };
            Volo.Abp.Domain.Entities.EntityHelper.TrySetId(hander, () => GuidGenerator.Create());

            try
            {
                var result = await handler.HandleAsync(scanCode);
                hander.Action = result.Action;
                hander.Success = result.Success;
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
                Logger.LogError(ex, $"处理器 {hander.HandlerFullName} 异常");
            }
            scanCode.ScanHandlerList.Add(hander);
        }

        scanCode.Execution = scanCode.ScanHandlerList.Sum(x => x.Execution);

        return await Repository.InsertAsync(scanCode, autoSave: true);
    }
}
