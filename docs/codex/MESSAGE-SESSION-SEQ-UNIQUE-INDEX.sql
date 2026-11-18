BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message]') AND [c].[name] = N'SessionSeq');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message] DROP CONSTRAINT ' + @var + ';');

CREATE UNIQUE INDEX [IX_Chat_Message_SessionId_SessionSeq] ON [Chat_Message] ([SessionId], [SessionSeq]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261109112254_AddMessageSessionSeqUniqueIndex', N'10.0.12');

COMMIT;
GO

