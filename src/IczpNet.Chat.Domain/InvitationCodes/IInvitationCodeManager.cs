using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.InvitationCodes;

public class InvitationCodeAdminListResult
{
    public long TotalCount { get; set; }

    public List<InvitationCode> Items { get; set; } = new();
}

public interface IInvitationCodeManager : IDomainService
{
    Task<InvitationCodeAdminListResult> GetAdminListAsync(
        string keyword = null,
        long? ownerId = null,
        bool? isEnabled = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default);

    Task<InvitationCode> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<InvitationCode> CreateAdminAsync(
        string title,
        long? ownerId,
        DateTime? expirationTime,
        CancellationToken cancellationToken = default);

    Task<InvitationCode> UpdateAdminAsync(
        Guid id,
        string title,
        DateTime? expirationTime,
        bool? isEnabled,
        CancellationToken cancellationToken = default);

    Task<InvitationCode> SetIsEnabledAdminAsync(
        Guid id,
        bool isEnabled,
        CancellationToken cancellationToken = default);

    Task DeleteAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
