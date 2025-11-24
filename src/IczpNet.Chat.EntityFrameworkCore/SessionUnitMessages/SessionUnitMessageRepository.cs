using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.MessageReminders;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Repositories;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
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
        var settingTableName = $"{ChatDbProperties.DbTablePrefix}_{nameof(SessionUnitSetting)}";
        var messageReminderTableName = $"{ChatDbProperties.DbTablePrefix}_{nameof(MessageReminder)}";
        var sql = $@"
INSERT INTO [{sessionUnitMessageTableName}] (
    [OwnerId],
    [SessionId],
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
    @OwnerId AS OwnerId,
    @SessionId AS SessionId,
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

    -- IsRemindMe
    CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM [{messageReminderTableName}] r
            WHERE r.[SessionUnitId] = su.Id
              AND r.[MessageId] = @MessageId
        ) THEN 1 ELSE 0 
    END AS IsRemindMe,

    0 AS IsDeleted,
    @ExtraProperties AS ExtraProperties,
    REPLACE(NEWID(), '-', '') AS ConcurrencyStamp,

    -- SYSUTCDATETIME() AS CreationTime,
    @CreationTime AS CreationTime,

    @CreatorId AS CreatorId
FROM [{sessionUnitTableName}] su
LEFT JOIN [{settingTableName}] s ON s.SessionUnitId = su.Id
WHERE su.SessionId = @SessionId
    AND su.IsDeleted = 0

    -- Setting rules
    AND s.IsKilled = 0
    AND s.IsEnabled = 1
    AND (s.HistoryFristTime IS NULL OR @CreationTime > s.HistoryFristTime)
    AND (s.HistoryLastTime IS NULL OR @CreationTime < s.HistoryLastTime)
    AND (s.ClearTime IS NULL OR @CreationTime > s.ClearTime)
";

        return await dbContext.Database.ExecuteSqlRawAsync(sql,
            new SqlParameter("@OwnerId", message.SenderId.Value),
            new SqlParameter("@SessionId", message.SessionId),
            new SqlParameter("@MessageId", message.Id),
            new SqlParameter("@SenderSessionUnitId", message.SenderSessionUnitId),
            new SqlParameter("@ExtraProperties", "{}"),
            new SqlParameter("@CreatorId", message.CreatorId ?? (object)DBNull.Value),
            new SqlParameter("@CreationTime", message.CreationTime));
    }
}
