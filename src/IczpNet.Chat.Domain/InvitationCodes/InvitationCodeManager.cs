using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.InvitationCodes;

public class InvitationCodeManager(
    IRepository<InvitationCode, Guid> repository) : DomainService, IInvitationCodeManager
{
    protected IRepository<InvitationCode, Guid> Repository { get; } = repository;

    public virtual async Task<InvitationCodeAdminListResult> GetAdminListAsync(
        string keyword = null,
        long? ownerId = null,
        bool? isEnabled = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = (await Repository.GetQueryableAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x =>
                (x.Title != null && x.Title.Contains(keyword.Trim())) ||
                (x.Code != null && x.Code.Contains(keyword.Trim())))
            .WhereIf(ownerId.HasValue, x => x.OwnerId == ownerId.Value)
            .WhereIf(isEnabled.HasValue, x => x.IsEnabled == isEnabled.Value);

        var totalCount = await AsyncExecuter.LongCountAsync(queryable, cancellationToken);
        if (totalCount == 0)
        {
            return new InvitationCodeAdminListResult();
        }

        queryable = ApplySorting(queryable, sorting);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount, 1, 200)),
            cancellationToken);

        return new InvitationCodeAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<InvitationCode> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.FindAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(InvitationCode), id);
        }

        return entity;
    }

    public virtual async Task<InvitationCode> CreateAdminAsync(
        string title,
        long? ownerId,
        DateTime? expirationTime,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new BusinessException("ChatAdmin:InvitationCodeTitleRequired", "邀请码标题不能为空。");
        }

        var code = await GenerateUniqueCodeAsync(cancellationToken);
        var entity = new InvitationCode
        {
            IsEnabled = true
        };
        entity.SetTitle(title.Trim());
        entity.SetCode(code);
        entity.SetExpirationTime(expirationTime ?? Clock.Now.AddDays(30));
        if (ownerId.HasValue)
        {
            entity.SetOwnerId(ownerId.Value);
        }

        await Repository.InsertAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return entity;
    }

    public virtual async Task<InvitationCode> UpdateAdminAsync(
        Guid id,
        string title,
        DateTime? expirationTime,
        bool? isEnabled,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken: cancellationToken);

        if (!string.IsNullOrWhiteSpace(title))
        {
            entity.SetTitle(title.Trim());
        }
        if (expirationTime.HasValue)
        {
            entity.SetExpirationTime(expirationTime.Value);
        }
        if (isEnabled.HasValue)
        {
            entity.IsEnabled = isEnabled.Value;
        }

        await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return entity;
    }

    public virtual async Task<InvitationCode> SetIsEnabledAdminAsync(
        Guid id,
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken: cancellationToken);
        entity.IsEnabled = isEnabled;
        await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return entity;
    }

    public virtual async Task DeleteAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.FindAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(InvitationCode), id);
        }

        await Repository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
    }

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var chars = new char[10];
            for (var i = 0; i < chars.Length; i++)
            {
                chars[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
            }
            var candidate = new string(chars);
            if (!await Repository.AnyAsync(x => x.Code == candidate, cancellationToken))
            {
                return candidate;
            }
        }

        return Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
    }

    private static IQueryable<InvitationCode> ApplySorting(IQueryable<InvitationCode> queryable, string sorting)
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
            "title" => desc ? queryable.OrderByDescending(x => x.Title) : queryable.OrderBy(x => x.Title),
            "expirationtime" => desc ? queryable.OrderByDescending(x => x.ExpirationTime) : queryable.OrderBy(x => x.ExpirationTime),
            "isenabled" => desc ? queryable.OrderByDescending(x => x.IsEnabled) : queryable.OrderBy(x => x.IsEnabled),
            "creationtime" => desc ? queryable.OrderByDescending(x => x.CreationTime) : queryable.OrderBy(x => x.CreationTime),
            _ => queryable.OrderByDescending(x => x.CreationTime)
        };
    }
}
