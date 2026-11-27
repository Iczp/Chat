using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.ScanCodes;

public class ScanCodeAdminListResult
{
    public long TotalCount { get; set; }

    public List<ScanCode> Items { get; set; } = new();
}

public interface IScanCodeManager
{
    Task<ScanCode> ScanCodeAsync(string type, string code, long? currentChatObjectId = null);

    Task<ScanCodeAdminListResult> GetAdminListAsync(
        string keyword = null,
        string handlerFullName = null,
        bool? success = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default);

    Task<ScanCode> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
