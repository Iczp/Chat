using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Repositories;
using IczpNet.Chat.SessionUnits;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.SessionUnitMessages;

public class SessionUnitMessageRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<SessionUnitMessage>(dbContextProvider), ISessionUnitMessageRepository
{
    public async Task<int> InsertMessagesForAllAsync(Message message)
    {

        var dbContext = await GetDbContextAsync();
        var sessionUnitMessageTableName = $"{ChatDbProperties.DbTablePrefix}_{nameof(SessionUnitMessage)}";
        var followTableName = $"{ChatDbProperties.DbTablePrefix}_{nameof(Follow)}";
        var sessionUnitTableName = $"{ChatDbProperties.DbTablePrefix}_{nameof(SessionUnit)}";
        var sql = $@"
INSERT INTO [{sessionUnitMessageTableName}] (
    [SessionUnitId],
    [MessageId],
    [IsRead],
    [IsOpened],
    [IsFavorited],
    [IsFollowing],
    [IsRemindMe],
    [IsDeleted],
    [ExtraProperties],
    [ConcurrencyStamp],
    [CreationTime],
    [CreatorId]
)
SELECT 
    su.Id AS SessionUnitId,
    @MessageId AS MessageId,
    0 AS IsRead,
    0 AS IsOpened,
    0 AS IsFavorited,
    -- IsFollowing 动态查询
    CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM [{followTableName}] f
            WHERE f.OwnerSessionUnitId = su.Id
              AND f.DestinationSessionUnitId = @SenderSessionUnitId
        ) THEN 1 ELSE 0 
    END AS IsFollowing,
    0 AS IsRemindMe,
    0 AS IsDeleted,
    @ExtraProperties AS ExtraProperties,
    REPLACE(NEWID(), '-', '') AS ConcurrencyStamp,
    SYSUTCDATETIME() AS CreationTime,
    @CreatorId AS CreatorId
FROM [{sessionUnitTableName}] su
WHERE su.SessionId = @SessionId
";

        return await dbContext.Database.ExecuteSqlRawAsync(sql,
            new SqlParameter("@SessionId", message.SessionId),
            new SqlParameter("@MessageId", message.Id),
            new SqlParameter("@SenderSessionUnitId", message.SenderSessionUnitId),
            new SqlParameter("@ExtraProperties", "{}"),
            new SqlParameter("@CreatorId", message.CreatorId));
    }
}
