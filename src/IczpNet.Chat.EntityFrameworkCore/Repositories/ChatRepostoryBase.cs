using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IczpNet.Chat.Repositories;

using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using IczpNet.Chat.EntityFrameworkCore;

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



