using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using IczpNet.Chat.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using Volo.Abp.Timing;

namespace IczpNet.Chat.Repositories;

public abstract class ChatRepositoryBase<TEntity, TPrimaryKey>(IDbContextProvider<ChatDbContext> dbContextProvider) : EfCoreRepository<ChatDbContext, TEntity, TPrimaryKey>(dbContextProvider)
        where TEntity : class, IEntity<TPrimaryKey>
{
    protected IClock Clock => LazyServiceProvider.LazyGetRequiredService<IClock>();
    public virtual Task<List<TEntity>> GetManyAsync(IEnumerable<TPrimaryKey> idList, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        return base.GetListAsync(x => idList.Contains(x.Id), includeDetails, cancellationToken);
    }

}

public abstract class ChatRepositoryBase<TEntity>(IDbContextProvider<ChatDbContext> dbContextProvider) : EfCoreRepository<ChatDbContext, TEntity>(dbContextProvider)
        where TEntity : class, IEntity
{
}



