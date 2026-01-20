using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionBoxes;

public class BoxManager(
    IRepository<Box, Guid> repository) : DomainService, IBoxManager
{
    public IRepository<Box, Guid> Repository { get; } = repository;

    public async Task<IEnumerable<Box>> GetListByOwnerAsync(long ownerId)
    {
        return (await Repository.GetQueryableAsync()).Where(x => x.OwnerId == ownerId);
    }
}
