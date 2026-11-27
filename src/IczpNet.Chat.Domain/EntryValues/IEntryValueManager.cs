using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.EntryValues;

public class EntryValueAdminListResult
{
    public long TotalCount { get; set; }

    public List<EntryValue> Items { get; set; } = new();
}

public interface IEntryValueManager : IDomainService
{
    Task<EntryValueAdminListResult> GetAdminListAsync(
        Guid? entryNameId = null,
        string keyword = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default);

    Task<EntryValue> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<EntryValue> CreateAdminAsync(
        Guid entryNameId,
        string value,
        bool isOption,
        bool isPublic,
        CancellationToken cancellationToken = default);

    Task<EntryValue> UpdateAdminAsync(
        Guid id,
        string value,
        bool isOption,
        bool isPublic,
        CancellationToken cancellationToken = default);

    Task DeleteAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
