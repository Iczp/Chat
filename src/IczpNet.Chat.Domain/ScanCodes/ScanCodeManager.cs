using IczpNet.AbpCommons;
using IczpNet.Chat;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Clients;
using Volo.Abp.Domain.Entities;
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

    public virtual async Task<ScanCodeAdminListResult> GetAdminListAsync(
        string keyword = null,
        string handlerFullName = null,
        bool? success = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = (await Repository.WithDetailsAsync(x => x.ScanHandlerList))
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x =>
                (x.Type != null && x.Type.Contains(keyword.Trim())) ||
                (x.Content != null && x.Content.Contains(keyword.Trim())) ||
                (x.ClientId != null && x.ClientId.Contains(keyword.Trim())) ||
                (x.DeviceId != null && x.DeviceId.Contains(keyword.Trim())))
            .WhereIf(!string.IsNullOrWhiteSpace(handlerFullName), x =>
                x.ScanHandlerList.Any(h => h.HandlerFullName != null && h.HandlerFullName.Contains(handlerFullName.Trim())))
            .WhereIf(success.HasValue, x =>
                x.ScanHandlerList.Any(h => h.Success == success.Value));

        var totalCount = await AsyncExecuter.LongCountAsync(queryable, cancellationToken);
        if (totalCount == 0)
        {
            return new ScanCodeAdminListResult();
        }

        queryable = ApplySorting(queryable, sorting);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount, 1, 200)),
            cancellationToken);

        return new ScanCodeAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<ScanCode> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var queryable = await Repository.WithDetailsAsync(x => x.ScanHandlerList);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id), cancellationToken);
        return entity ?? throw new EntityNotFoundException(typeof(ScanCode), id);
    }

    private static IQueryable<ScanCode> ApplySorting(IQueryable<ScanCode> queryable, string sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return queryable.OrderByDescending(x => x.CreationTime);
        }

        var parts = sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0].ToLowerInvariant();
        var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return field switch
        {
            "execution" => desc ? queryable.OrderByDescending(x => x.Execution) : queryable.OrderBy(x => x.Execution),
            "handlercount" => desc ? queryable.OrderByDescending(x => x.HandlerCount) : queryable.OrderBy(x => x.HandlerCount),
            "creationtime" => desc ? queryable.OrderByDescending(x => x.CreationTime) : queryable.OrderBy(x => x.CreationTime),
            _ => queryable.OrderByDescending(x => x.CreationTime)
        };
    }
}
