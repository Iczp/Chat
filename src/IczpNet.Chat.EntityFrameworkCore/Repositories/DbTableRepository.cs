﻿using IczpNet.Chat.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IczpNet.Chat.DbTables;
using System.Linq;

namespace IczpNet.Chat.Repositories;

public class DbTableRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<DbTable>(dbContextProvider), IDbTableRepository
{
    public override async Task<IQueryable<DbTable>> GetQueryableAsync()
    {
        var context = await GetDbContextAsync();

        var sql = @$"
SELECT
        QUOTENAME(SCHEMA_NAME(sOBJ.schema_id)) + '.' + QUOTENAME(sOBJ.name) AS [TableName]
        , SUM(sPTN.Rows) AS [RowCount]
FROM 
        sys.objects AS sOBJ
        INNER JOIN sys.partitions AS sPTN
            ON sOBJ.object_id = sPTN.object_id
WHERE
        sOBJ.type = 'U'
        AND sOBJ.is_ms_shipped = 0x0
        AND index_id < 2 -- 0:Heap, 1:Clustered
GROUP BY 
        sOBJ.schema_id
        , sOBJ.name
";
        await Task.Yield();

        return context.DbTable.FromSqlRaw(sql);
    }
}
