USE [Chat_Module_v3]
GO

--修改 Ticks 为 creationTime 的 js时间戳

UPDATE Chat_SessionUnit
SET Sorting = FLOOR(Sorting )
WHERE Sorting > 0;


UPDATE Chat_SessionUnit
SET Ticks = FLOOR((Ticks - 621355968000000000) / 10000)
WHERE Ticks > 621355968000000000;


--直接一次性更新全表（CreationTime  JS 时间戳 Ticks）
UPDATE [Chat_SessionUnit]
SET Ticks = DATEDIFF_BIG(MILLISECOND, '1970-01-01', CreationTime) where Ticks=0;


--分片更新
DECLARE @batchSize INT = 50000;
DECLARE @page BIGINT = 0;
DECLARE @updated INT = 1;

WHILE (@updated > 0)
BEGIN
    ;WITH cte AS (
        SELECT 
            Id,
            rn = ROW_NUMBER() OVER (ORDER BY Id)   -- Guid 随机，但顺序分页是 OK 的
        FROM Chat_SessionUnit WITH (NOLOCK)
    ),
    target AS (
        SELECT TOP (@batchSize) Id
        FROM cte
        WHERE rn > (@page * @batchSize)
        ORDER BY rn
    )
    UPDATE su
    SET su.Ticks = DATEDIFF_BIG(MILLISECOND, '1970-01-01', su.CreationTime)
    FROM Chat_SessionUnit su
    INNER JOIN target t ON su.Id = t.Id;

    SET @updated = @@ROWCOUNT;

    PRINT CONCAT('Updated batch ', @page, ' updated rows = ', @updated);

    SET @page = @page + 1;
END