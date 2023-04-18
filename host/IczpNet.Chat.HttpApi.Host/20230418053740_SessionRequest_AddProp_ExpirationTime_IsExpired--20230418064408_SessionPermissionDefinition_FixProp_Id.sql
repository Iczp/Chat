BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionUnitGrant]') AND [c].[name] = N'DefinitionId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionUnitGrant] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Chat_SessionPermissionUnitGrant] ALTER COLUMN [DefinitionId] nvarchar(450) NOT NULL;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionRoleGrant]') AND [c].[name] = N'DefinitionId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionRoleGrant] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Chat_SessionPermissionRoleGrant] ALTER COLUMN [DefinitionId] nvarchar(450) NOT NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionDefinition]') AND [c].[name] = N'Id');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionDefinition] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Chat_SessionPermissionDefinition] ALTER COLUMN [Id] nvarchar(450) NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230418064408_SessionPermissionDefinition_FixProp_Id', N'7.0.4');
GO

COMMIT;
GO

