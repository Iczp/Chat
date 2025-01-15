using IczpNet.Chat.ChatObjects;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Developers;


/// <inheritdoc />
public class DeveloperManager(IRepository<Developer> developerRepository) : DomainService, IDeveloperManager
{
    protected IRepository<Developer> DeveloperRepository { get; } = developerRepository;

    /// <inheritdoc />
    public async Task<bool> IsEnabledAsync(ChatObject owner)
    {
        return await IsEnabledAsync(owner.Id);
    }

    /// <inheritdoc />
    public async Task<bool> IsEnabledAsync(long? ownerId)
    {
        var entity = await DeveloperRepository.FindAsync(x => x.OwnerId == ownerId && x.IsEnabled);
        return entity != null;
    }
}
