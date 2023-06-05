using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.DbTables
{
    public interface IDbTableRepository
    {
        Task<IQueryable<DbTable>> GetQueryableAsync();
    }
}
