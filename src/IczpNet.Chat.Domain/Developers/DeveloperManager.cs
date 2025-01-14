using IczpNet.Chat.ChatObjects;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Developers;

public class DeveloperManager(IRepository<Developer> developerRepository) : DomainService, IDeveloperManager
{
    protected IRepository<Developer> DeveloperRepository { get; } = developerRepository;

    public async Task<bool> IsEnabledAsync(ChatObject owner)
    {
        var entity = await DeveloperRepository.FindAsync(x => x.OwnerId == owner.Id && x.IsEnabled);
        return entity != null;
    }
}
