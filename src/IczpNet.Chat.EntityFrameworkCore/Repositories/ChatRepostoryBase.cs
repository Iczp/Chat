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

namespace IczpNet.Chat.Repositories;

public abstract class ChatRepositoryBase<TEntity, TPrimaryKey> : EfCoreRepository<ChatDbContext, TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
{

    protected ChatRepositoryBase(IDbContextProvider<ChatDbContext> dbContextProvider) : base(dbContextProvider)
    {

    }

    public virtual Task<List<TEntity>> GetManyAsync(IEnumerable<TPrimaryKey> idList, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        return base.GetListAsync(x => idList.Contains(x.Id), includeDetails, cancellationToken);
    }

}

public abstract class ChatRepositoryBase<TEntity> : EfCoreRepository<ChatDbContext, TEntity>
        where TEntity : class, IEntity
{

    protected ChatRepositoryBase(IDbContextProvider<ChatDbContext> dbContextProvider) : base(dbContextProvider)
    {

    }
}



