BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Word]') AND [c].[name] = N'ExtraProperties');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Word] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [Chat_Word] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Word] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Word] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Word]') AND [c].[name] = N'ConcurrencyStamp');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Word] DROP CONSTRAINT [' + @var1 + '];');
UPDATE [Chat_Word] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Word] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Word] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletRequest]') AND [c].[name] = N'ExtraProperties');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletRequest] DROP CONSTRAINT [' + @var2 + '];');
UPDATE [Chat_WalletRequest] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_WalletRequest] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_WalletRequest] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletRequest]') AND [c].[name] = N'ConcurrencyStamp');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletRequest] DROP CONSTRAINT [' + @var3 + '];');
UPDATE [Chat_WalletRequest] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_WalletRequest] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_WalletRequest] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletRecorder]') AND [c].[name] = N'ExtraProperties');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletRecorder] DROP CONSTRAINT [' + @var4 + '];');
UPDATE [Chat_WalletRecorder] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_WalletRecorder] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_WalletRecorder] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletRecorder]') AND [c].[name] = N'ConcurrencyStamp');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletRecorder] DROP CONSTRAINT [' + @var5 + '];');
UPDATE [Chat_WalletRecorder] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_WalletRecorder] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_WalletRecorder] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletOrder]') AND [c].[name] = N'ExtraProperties');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletOrder] DROP CONSTRAINT [' + @var6 + '];');
UPDATE [Chat_WalletOrder] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_WalletOrder] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_WalletOrder] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletOrder]') AND [c].[name] = N'ConcurrencyStamp');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletOrder] DROP CONSTRAINT [' + @var7 + '];');
UPDATE [Chat_WalletOrder] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_WalletOrder] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_WalletOrder] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletBusiness]') AND [c].[name] = N'ExtraProperties');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletBusiness] DROP CONSTRAINT [' + @var8 + '];');
UPDATE [Chat_WalletBusiness] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_WalletBusiness] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_WalletBusiness] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletBusiness]') AND [c].[name] = N'ConcurrencyStamp');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletBusiness] DROP CONSTRAINT [' + @var9 + '];');
UPDATE [Chat_WalletBusiness] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_WalletBusiness] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_WalletBusiness] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Wallet]') AND [c].[name] = N'ExtraProperties');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Wallet] DROP CONSTRAINT [' + @var10 + '];');
UPDATE [Chat_Wallet] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Wallet] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Wallet] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Wallet]') AND [c].[name] = N'ConcurrencyStamp');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Wallet] DROP CONSTRAINT [' + @var11 + '];');
UPDATE [Chat_Wallet] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Wallet] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Wallet] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_TextContentWord]') AND [c].[name] = N'ExtraProperties');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Chat_TextContentWord] DROP CONSTRAINT [' + @var12 + '];');
UPDATE [Chat_TextContentWord] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_TextContentWord] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_TextContentWord] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_TextContentWord]') AND [c].[name] = N'ConcurrencyStamp');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Chat_TextContentWord] DROP CONSTRAINT [' + @var13 + '];');
UPDATE [Chat_TextContentWord] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_TextContentWord] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_TextContentWord] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitTag]') AND [c].[name] = N'ExtraProperties');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitTag] DROP CONSTRAINT [' + @var14 + '];');
UPDATE [Chat_SessionUnitTag] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitTag] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitTag] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitTag]') AND [c].[name] = N'ConcurrencyStamp');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitTag] DROP CONSTRAINT [' + @var15 + '];');
UPDATE [Chat_SessionUnitTag] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitTag] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitTag] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitSetting]') AND [c].[name] = N'ExtraProperties');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitSetting] DROP CONSTRAINT [' + @var16 + '];');
UPDATE [Chat_SessionUnitSetting] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitSetting] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitSetting] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitSetting]') AND [c].[name] = N'ConcurrencyStamp');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitSetting] DROP CONSTRAINT [' + @var17 + '];');
UPDATE [Chat_SessionUnitSetting] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitSetting] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitSetting] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitRole]') AND [c].[name] = N'ExtraProperties');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitRole] DROP CONSTRAINT [' + @var18 + '];');
UPDATE [Chat_SessionUnitRole] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitRole] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitRole] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitRole]') AND [c].[name] = N'ConcurrencyStamp');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitRole] DROP CONSTRAINT [' + @var19 + '];');
UPDATE [Chat_SessionUnitRole] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitRole] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitRole] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var20 sysname;
SELECT @var20 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitOrganization]') AND [c].[name] = N'ExtraProperties');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitOrganization] DROP CONSTRAINT [' + @var20 + '];');
UPDATE [Chat_SessionUnitOrganization] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitOrganization] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitOrganization] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var21 sysname;
SELECT @var21 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitOrganization]') AND [c].[name] = N'ConcurrencyStamp');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitOrganization] DROP CONSTRAINT [' + @var21 + '];');
UPDATE [Chat_SessionUnitOrganization] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitOrganization] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitOrganization] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var22 sysname;
SELECT @var22 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitEntryValue]') AND [c].[name] = N'ExtraProperties');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitEntryValue] DROP CONSTRAINT [' + @var22 + '];');
UPDATE [Chat_SessionUnitEntryValue] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitEntryValue] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitEntryValue] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var23 sysname;
SELECT @var23 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitEntryValue]') AND [c].[name] = N'ConcurrencyStamp');
IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitEntryValue] DROP CONSTRAINT [' + @var23 + '];');
UPDATE [Chat_SessionUnitEntryValue] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitEntryValue] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitEntryValue] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var24 sysname;
SELECT @var24 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitCounter]') AND [c].[name] = N'ExtraProperties');
IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitCounter] DROP CONSTRAINT [' + @var24 + '];');
UPDATE [Chat_SessionUnitCounter] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitCounter] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitCounter] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var25 sysname;
SELECT @var25 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitCounter]') AND [c].[name] = N'ConcurrencyStamp');
IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitCounter] DROP CONSTRAINT [' + @var25 + '];');
UPDATE [Chat_SessionUnitCounter] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitCounter] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitCounter] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var26 sysname;
SELECT @var26 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitContactTag]') AND [c].[name] = N'ExtraProperties');
IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitContactTag] DROP CONSTRAINT [' + @var26 + '];');
UPDATE [Chat_SessionUnitContactTag] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitContactTag] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitContactTag] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var27 sysname;
SELECT @var27 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitContactTag]') AND [c].[name] = N'ConcurrencyStamp');
IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitContactTag] DROP CONSTRAINT [' + @var27 + '];');
UPDATE [Chat_SessionUnitContactTag] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitContactTag] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitContactTag] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var28 sysname;
SELECT @var28 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'ExtraProperties');
IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var28 + '];');
UPDATE [Chat_SessionUnit] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnit] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnit] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var29 sysname;
SELECT @var29 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'ConcurrencyStamp');
IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var29 + '];');
UPDATE [Chat_SessionUnit] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnit] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnit] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var30 sysname;
SELECT @var30 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionTag]') AND [c].[name] = N'ExtraProperties');
IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionTag] DROP CONSTRAINT [' + @var30 + '];');
UPDATE [Chat_SessionTag] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionTag] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionTag] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var31 sysname;
SELECT @var31 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionTag]') AND [c].[name] = N'ConcurrencyStamp');
IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionTag] DROP CONSTRAINT [' + @var31 + '];');
UPDATE [Chat_SessionTag] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionTag] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionTag] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var32 sysname;
SELECT @var32 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionRole]') AND [c].[name] = N'ExtraProperties');
IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionRole] DROP CONSTRAINT [' + @var32 + '];');
UPDATE [Chat_SessionRole] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionRole] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionRole] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var33 sysname;
SELECT @var33 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionRole]') AND [c].[name] = N'ConcurrencyStamp');
IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionRole] DROP CONSTRAINT [' + @var33 + '];');
UPDATE [Chat_SessionRole] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionRole] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionRole] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var34 sysname;
SELECT @var34 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionRequest]') AND [c].[name] = N'ExtraProperties');
IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionRequest] DROP CONSTRAINT [' + @var34 + '];');
UPDATE [Chat_SessionRequest] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionRequest] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionRequest] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var35 sysname;
SELECT @var35 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionRequest]') AND [c].[name] = N'ConcurrencyStamp');
IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionRequest] DROP CONSTRAINT [' + @var35 + '];');
UPDATE [Chat_SessionRequest] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionRequest] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionRequest] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var36 sysname;
SELECT @var36 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionUnitGrant]') AND [c].[name] = N'ExtraProperties');
IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionUnitGrant] DROP CONSTRAINT [' + @var36 + '];');
UPDATE [Chat_SessionPermissionUnitGrant] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionPermissionUnitGrant] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionPermissionUnitGrant] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var37 sysname;
SELECT @var37 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionUnitGrant]') AND [c].[name] = N'ConcurrencyStamp');
IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionUnitGrant] DROP CONSTRAINT [' + @var37 + '];');
UPDATE [Chat_SessionPermissionUnitGrant] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionPermissionUnitGrant] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionPermissionUnitGrant] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var38 sysname;
SELECT @var38 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionRoleGrant]') AND [c].[name] = N'ExtraProperties');
IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionRoleGrant] DROP CONSTRAINT [' + @var38 + '];');
UPDATE [Chat_SessionPermissionRoleGrant] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionPermissionRoleGrant] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionPermissionRoleGrant] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var39 sysname;
SELECT @var39 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionRoleGrant]') AND [c].[name] = N'ConcurrencyStamp');
IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionRoleGrant] DROP CONSTRAINT [' + @var39 + '];');
UPDATE [Chat_SessionPermissionRoleGrant] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionPermissionRoleGrant] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionPermissionRoleGrant] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var40 sysname;
SELECT @var40 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionGroup]') AND [c].[name] = N'ExtraProperties');
IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionGroup] DROP CONSTRAINT [' + @var40 + '];');
UPDATE [Chat_SessionPermissionGroup] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionPermissionGroup] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionPermissionGroup] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var41 sysname;
SELECT @var41 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionGroup]') AND [c].[name] = N'ConcurrencyStamp');
IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionGroup] DROP CONSTRAINT [' + @var41 + '];');
UPDATE [Chat_SessionPermissionGroup] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionPermissionGroup] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionPermissionGroup] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var42 sysname;
SELECT @var42 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionDefinition]') AND [c].[name] = N'ExtraProperties');
IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionDefinition] DROP CONSTRAINT [' + @var42 + '];');
UPDATE [Chat_SessionPermissionDefinition] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionPermissionDefinition] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionPermissionDefinition] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var43 sysname;
SELECT @var43 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionDefinition]') AND [c].[name] = N'ConcurrencyStamp');
IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionDefinition] DROP CONSTRAINT [' + @var43 + '];');
UPDATE [Chat_SessionPermissionDefinition] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionPermissionDefinition] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionPermissionDefinition] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var44 sysname;
SELECT @var44 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionOrganization]') AND [c].[name] = N'ExtraProperties');
IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionOrganization] DROP CONSTRAINT [' + @var44 + '];');
UPDATE [Chat_SessionOrganization] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionOrganization] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionOrganization] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var45 sysname;
SELECT @var45 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionOrganization]') AND [c].[name] = N'ConcurrencyStamp');
IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionOrganization] DROP CONSTRAINT [' + @var45 + '];');
UPDATE [Chat_SessionOrganization] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionOrganization] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionOrganization] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var46 sysname;
SELECT @var46 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Session]') AND [c].[name] = N'ExtraProperties');
IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Session] DROP CONSTRAINT [' + @var46 + '];');
UPDATE [Chat_Session] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Session] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Session] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var47 sysname;
SELECT @var47 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Session]') AND [c].[name] = N'ConcurrencyStamp');
IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Session] DROP CONSTRAINT [' + @var47 + '];');
UPDATE [Chat_Session] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Session] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Session] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var48 sysname;
SELECT @var48 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ServerHost]') AND [c].[name] = N'ExtraProperties');
IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ServerHost] DROP CONSTRAINT [' + @var48 + '];');
UPDATE [Chat_ServerHost] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ServerHost] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ServerHost] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var49 sysname;
SELECT @var49 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ServerHost]') AND [c].[name] = N'ConcurrencyStamp');
IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ServerHost] DROP CONSTRAINT [' + @var49 + '];');
UPDATE [Chat_ServerHost] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ServerHost] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ServerHost] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var50 sysname;
SELECT @var50 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Scoped]') AND [c].[name] = N'ExtraProperties');
IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Scoped] DROP CONSTRAINT [' + @var50 + '];');
UPDATE [Chat_Scoped] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Scoped] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Scoped] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var51 sysname;
SELECT @var51 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Scoped]') AND [c].[name] = N'ConcurrencyStamp');
IF @var51 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Scoped] DROP CONSTRAINT [' + @var51 + '];');
UPDATE [Chat_Scoped] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Scoped] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Scoped] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var52 sysname;
SELECT @var52 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_RedEnvelopeUnit]') AND [c].[name] = N'ExtraProperties');
IF @var52 IS NOT NULL EXEC(N'ALTER TABLE [Chat_RedEnvelopeUnit] DROP CONSTRAINT [' + @var52 + '];');
UPDATE [Chat_RedEnvelopeUnit] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_RedEnvelopeUnit] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_RedEnvelopeUnit] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var53 sysname;
SELECT @var53 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_RedEnvelopeUnit]') AND [c].[name] = N'ConcurrencyStamp');
IF @var53 IS NOT NULL EXEC(N'ALTER TABLE [Chat_RedEnvelopeUnit] DROP CONSTRAINT [' + @var53 + '];');
UPDATE [Chat_RedEnvelopeUnit] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_RedEnvelopeUnit] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_RedEnvelopeUnit] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var54 sysname;
SELECT @var54 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'ExtraProperties');
IF @var54 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var54 + '];');
UPDATE [Chat_ReadedRecorder] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ReadedRecorder] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ReadedRecorder] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var55 sysname;
SELECT @var55 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'ConcurrencyStamp');
IF @var55 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var55 + '];');
UPDATE [Chat_ReadedRecorder] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ReadedRecorder] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ReadedRecorder] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var56 sysname;
SELECT @var56 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_PaymentPlatform]') AND [c].[name] = N'ExtraProperties');
IF @var56 IS NOT NULL EXEC(N'ALTER TABLE [Chat_PaymentPlatform] DROP CONSTRAINT [' + @var56 + '];');
UPDATE [Chat_PaymentPlatform] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_PaymentPlatform] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_PaymentPlatform] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var57 sysname;
SELECT @var57 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_PaymentPlatform]') AND [c].[name] = N'ConcurrencyStamp');
IF @var57 IS NOT NULL EXEC(N'ALTER TABLE [Chat_PaymentPlatform] DROP CONSTRAINT [' + @var57 + '];');
UPDATE [Chat_PaymentPlatform] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_PaymentPlatform] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_PaymentPlatform] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var58 sysname;
SELECT @var58 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_OpenedRecorder]') AND [c].[name] = N'ExtraProperties');
IF @var58 IS NOT NULL EXEC(N'ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [' + @var58 + '];');
UPDATE [Chat_OpenedRecorder] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_OpenedRecorder] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_OpenedRecorder] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var59 sysname;
SELECT @var59 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_OpenedRecorder]') AND [c].[name] = N'ConcurrencyStamp');
IF @var59 IS NOT NULL EXEC(N'ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [' + @var59 + '];');
UPDATE [Chat_OpenedRecorder] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_OpenedRecorder] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_OpenedRecorder] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var60 sysname;
SELECT @var60 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Motto]') AND [c].[name] = N'ExtraProperties');
IF @var60 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Motto] DROP CONSTRAINT [' + @var60 + '];');
UPDATE [Chat_Motto] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Motto] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Motto] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var61 sysname;
SELECT @var61 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Motto]') AND [c].[name] = N'ConcurrencyStamp');
IF @var61 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Motto] DROP CONSTRAINT [' + @var61 + '];');
UPDATE [Chat_Motto] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Motto] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Motto] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var62 sysname;
SELECT @var62 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageWord]') AND [c].[name] = N'ExtraProperties');
IF @var62 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageWord] DROP CONSTRAINT [' + @var62 + '];');
UPDATE [Chat_MessageWord] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_MessageWord] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_MessageWord] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var63 sysname;
SELECT @var63 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageWord]') AND [c].[name] = N'ConcurrencyStamp');
IF @var63 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageWord] DROP CONSTRAINT [' + @var63 + '];');
UPDATE [Chat_MessageWord] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_MessageWord] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_MessageWord] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var64 sysname;
SELECT @var64 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageReminder]') AND [c].[name] = N'ExtraProperties');
IF @var64 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageReminder] DROP CONSTRAINT [' + @var64 + '];');
UPDATE [Chat_MessageReminder] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_MessageReminder] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_MessageReminder] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var65 sysname;
SELECT @var65 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageReminder]') AND [c].[name] = N'ConcurrencyStamp');
IF @var65 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageReminder] DROP CONSTRAINT [' + @var65 + '];');
UPDATE [Chat_MessageReminder] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_MessageReminder] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_MessageReminder] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var66 sysname;
SELECT @var66 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageContent]') AND [c].[name] = N'ExtraProperties');
IF @var66 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageContent] DROP CONSTRAINT [' + @var66 + '];');
UPDATE [Chat_MessageContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_MessageContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_MessageContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var67 sysname;
SELECT @var67 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var67 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageContent] DROP CONSTRAINT [' + @var67 + '];');
UPDATE [Chat_MessageContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_MessageContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_MessageContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var68 sysname;
SELECT @var68 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_VideoContent]') AND [c].[name] = N'ExtraProperties');
IF @var68 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_VideoContent] DROP CONSTRAINT [' + @var68 + '];');
UPDATE [Chat_Message_Template_VideoContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_VideoContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_VideoContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var69 sysname;
SELECT @var69 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_VideoContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var69 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_VideoContent] DROP CONSTRAINT [' + @var69 + '];');
UPDATE [Chat_Message_Template_VideoContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_VideoContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_VideoContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var70 sysname;
SELECT @var70 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_TextContent]') AND [c].[name] = N'ExtraProperties');
IF @var70 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_TextContent] DROP CONSTRAINT [' + @var70 + '];');
UPDATE [Chat_Message_Template_TextContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_TextContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_TextContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var71 sysname;
SELECT @var71 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_TextContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var71 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_TextContent] DROP CONSTRAINT [' + @var71 + '];');
UPDATE [Chat_Message_Template_TextContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_TextContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_TextContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var72 sysname;
SELECT @var72 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_SoundContent]') AND [c].[name] = N'ExtraProperties');
IF @var72 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_SoundContent] DROP CONSTRAINT [' + @var72 + '];');
UPDATE [Chat_Message_Template_SoundContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_SoundContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_SoundContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var73 sysname;
SELECT @var73 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_SoundContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var73 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_SoundContent] DROP CONSTRAINT [' + @var73 + '];');
UPDATE [Chat_Message_Template_SoundContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_SoundContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_SoundContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var74 sysname;
SELECT @var74 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_RedEnvelopeContent]') AND [c].[name] = N'ExtraProperties');
IF @var74 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] DROP CONSTRAINT [' + @var74 + '];');
UPDATE [Chat_Message_Template_RedEnvelopeContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var75 sysname;
SELECT @var75 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_RedEnvelopeContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var75 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] DROP CONSTRAINT [' + @var75 + '];');
UPDATE [Chat_Message_Template_RedEnvelopeContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var76 sysname;
SELECT @var76 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_LocationContent]') AND [c].[name] = N'ExtraProperties');
IF @var76 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_LocationContent] DROP CONSTRAINT [' + @var76 + '];');
UPDATE [Chat_Message_Template_LocationContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_LocationContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_LocationContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var77 sysname;
SELECT @var77 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_LocationContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var77 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_LocationContent] DROP CONSTRAINT [' + @var77 + '];');
UPDATE [Chat_Message_Template_LocationContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_LocationContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_LocationContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var78 sysname;
SELECT @var78 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_LinkContent]') AND [c].[name] = N'ExtraProperties');
IF @var78 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_LinkContent] DROP CONSTRAINT [' + @var78 + '];');
UPDATE [Chat_Message_Template_LinkContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_LinkContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_LinkContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var79 sysname;
SELECT @var79 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_LinkContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var79 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_LinkContent] DROP CONSTRAINT [' + @var79 + '];');
UPDATE [Chat_Message_Template_LinkContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_LinkContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_LinkContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var80 sysname;
SELECT @var80 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ImageContent]') AND [c].[name] = N'ExtraProperties');
IF @var80 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ImageContent] DROP CONSTRAINT [' + @var80 + '];');
UPDATE [Chat_Message_Template_ImageContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_ImageContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_ImageContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var81 sysname;
SELECT @var81 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ImageContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var81 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ImageContent] DROP CONSTRAINT [' + @var81 + '];');
UPDATE [Chat_Message_Template_ImageContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_ImageContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_ImageContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var82 sysname;
SELECT @var82 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_HtmlContent]') AND [c].[name] = N'ExtraProperties');
IF @var82 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_HtmlContent] DROP CONSTRAINT [' + @var82 + '];');
UPDATE [Chat_Message_Template_HtmlContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_HtmlContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_HtmlContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var83 sysname;
SELECT @var83 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_HtmlContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var83 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_HtmlContent] DROP CONSTRAINT [' + @var83 + '];');
UPDATE [Chat_Message_Template_HtmlContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_HtmlContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_HtmlContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var84 sysname;
SELECT @var84 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_HistoryContent]') AND [c].[name] = N'ExtraProperties');
IF @var84 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_HistoryContent] DROP CONSTRAINT [' + @var84 + '];');
UPDATE [Chat_Message_Template_HistoryContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_HistoryContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_HistoryContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var85 sysname;
SELECT @var85 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_HistoryContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var85 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_HistoryContent] DROP CONSTRAINT [' + @var85 + '];');
UPDATE [Chat_Message_Template_HistoryContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_HistoryContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_HistoryContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var86 sysname;
SELECT @var86 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_FileContent]') AND [c].[name] = N'ExtraProperties');
IF @var86 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_FileContent] DROP CONSTRAINT [' + @var86 + '];');
UPDATE [Chat_Message_Template_FileContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_FileContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_FileContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var87 sysname;
SELECT @var87 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_FileContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var87 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_FileContent] DROP CONSTRAINT [' + @var87 + '];');
UPDATE [Chat_Message_Template_FileContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_FileContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_FileContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var88 sysname;
SELECT @var88 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ContactsContent]') AND [c].[name] = N'ExtraProperties');
IF @var88 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ContactsContent] DROP CONSTRAINT [' + @var88 + '];');
UPDATE [Chat_Message_Template_ContactsContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_ContactsContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_ContactsContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var89 sysname;
SELECT @var89 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ContactsContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var89 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ContactsContent] DROP CONSTRAINT [' + @var89 + '];');
UPDATE [Chat_Message_Template_ContactsContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_ContactsContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_ContactsContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var90 sysname;
SELECT @var90 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_CmdContent]') AND [c].[name] = N'ExtraProperties');
IF @var90 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_CmdContent] DROP CONSTRAINT [' + @var90 + '];');
UPDATE [Chat_Message_Template_CmdContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_CmdContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_CmdContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var91 sysname;
SELECT @var91 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_CmdContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var91 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_CmdContent] DROP CONSTRAINT [' + @var91 + '];');
UPDATE [Chat_Message_Template_CmdContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_CmdContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_CmdContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var92 sysname;
SELECT @var92 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ArticleContent]') AND [c].[name] = N'ExtraProperties');
IF @var92 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ArticleContent] DROP CONSTRAINT [' + @var92 + '];');
UPDATE [Chat_Message_Template_ArticleContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_ArticleContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_ArticleContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var93 sysname;
SELECT @var93 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ArticleContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var93 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ArticleContent] DROP CONSTRAINT [' + @var93 + '];');
UPDATE [Chat_Message_Template_ArticleContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_ArticleContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_ArticleContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var94 sysname;
SELECT @var94 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message]') AND [c].[name] = N'ExtraProperties');
IF @var94 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message] DROP CONSTRAINT [' + @var94 + '];');
UPDATE [Chat_Message] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var95 sysname;
SELECT @var95 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message]') AND [c].[name] = N'ConcurrencyStamp');
IF @var95 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message] DROP CONSTRAINT [' + @var95 + '];');
UPDATE [Chat_Message] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var96 sysname;
SELECT @var96 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Menu]') AND [c].[name] = N'ExtraProperties');
IF @var96 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Menu] DROP CONSTRAINT [' + @var96 + '];');
UPDATE [Chat_Menu] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Menu] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Menu] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var97 sysname;
SELECT @var97 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Menu]') AND [c].[name] = N'ConcurrencyStamp');
IF @var97 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Menu] DROP CONSTRAINT [' + @var97 + '];');
UPDATE [Chat_Menu] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Menu] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Menu] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var98 sysname;
SELECT @var98 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_InvitationCode]') AND [c].[name] = N'ExtraProperties');
IF @var98 IS NOT NULL EXEC(N'ALTER TABLE [Chat_InvitationCode] DROP CONSTRAINT [' + @var98 + '];');
UPDATE [Chat_InvitationCode] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_InvitationCode] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_InvitationCode] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var99 sysname;
SELECT @var99 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_InvitationCode]') AND [c].[name] = N'ConcurrencyStamp');
IF @var99 IS NOT NULL EXEC(N'ALTER TABLE [Chat_InvitationCode] DROP CONSTRAINT [' + @var99 + '];');
UPDATE [Chat_InvitationCode] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_InvitationCode] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_InvitationCode] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var100 sysname;
SELECT @var100 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HttpResponse]') AND [c].[name] = N'ExtraProperties');
IF @var100 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HttpResponse] DROP CONSTRAINT [' + @var100 + '];');
UPDATE [Chat_HttpResponse] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_HttpResponse] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_HttpResponse] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var101 sysname;
SELECT @var101 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HttpResponse]') AND [c].[name] = N'ConcurrencyStamp');
IF @var101 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HttpResponse] DROP CONSTRAINT [' + @var101 + '];');
UPDATE [Chat_HttpResponse] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_HttpResponse] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_HttpResponse] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var102 sysname;
SELECT @var102 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HttpRequest]') AND [c].[name] = N'ExtraProperties');
IF @var102 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HttpRequest] DROP CONSTRAINT [' + @var102 + '];');
UPDATE [Chat_HttpRequest] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_HttpRequest] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_HttpRequest] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var103 sysname;
SELECT @var103 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HttpRequest]') AND [c].[name] = N'ConcurrencyStamp');
IF @var103 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HttpRequest] DROP CONSTRAINT [' + @var103 + '];');
UPDATE [Chat_HttpRequest] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_HttpRequest] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_HttpRequest] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var104 sysname;
SELECT @var104 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HistoryMessage]') AND [c].[name] = N'ExtraProperties');
IF @var104 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HistoryMessage] DROP CONSTRAINT [' + @var104 + '];');
UPDATE [Chat_HistoryMessage] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_HistoryMessage] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_HistoryMessage] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var105 sysname;
SELECT @var105 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HistoryMessage]') AND [c].[name] = N'ConcurrencyStamp');
IF @var105 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HistoryMessage] DROP CONSTRAINT [' + @var105 + '];');
UPDATE [Chat_HistoryMessage] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_HistoryMessage] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_HistoryMessage] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var106 sysname;
SELECT @var106 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Follow]') AND [c].[name] = N'ExtraProperties');
IF @var106 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Follow] DROP CONSTRAINT [' + @var106 + '];');
UPDATE [Chat_Follow] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Follow] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Follow] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var107 sysname;
SELECT @var107 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Follow]') AND [c].[name] = N'ConcurrencyStamp');
IF @var107 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Follow] DROP CONSTRAINT [' + @var107 + '];');
UPDATE [Chat_Follow] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Follow] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Follow] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var108 sysname;
SELECT @var108 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_FavoritedRecorder]') AND [c].[name] = N'ExtraProperties');
IF @var108 IS NOT NULL EXEC(N'ALTER TABLE [Chat_FavoritedRecorder] DROP CONSTRAINT [' + @var108 + '];');
UPDATE [Chat_FavoritedRecorder] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_FavoritedRecorder] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_FavoritedRecorder] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var109 sysname;
SELECT @var109 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_FavoritedRecorder]') AND [c].[name] = N'ConcurrencyStamp');
IF @var109 IS NOT NULL EXEC(N'ALTER TABLE [Chat_FavoritedRecorder] DROP CONSTRAINT [' + @var109 + '];');
UPDATE [Chat_FavoritedRecorder] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_FavoritedRecorder] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_FavoritedRecorder] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var110 sysname;
SELECT @var110 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryValue]') AND [c].[name] = N'ExtraProperties');
IF @var110 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryValue] DROP CONSTRAINT [' + @var110 + '];');
UPDATE [Chat_EntryValue] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_EntryValue] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_EntryValue] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var111 sysname;
SELECT @var111 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryValue]') AND [c].[name] = N'ConcurrencyStamp');
IF @var111 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryValue] DROP CONSTRAINT [' + @var111 + '];');
UPDATE [Chat_EntryValue] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_EntryValue] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_EntryValue] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var112 sysname;
SELECT @var112 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryName]') AND [c].[name] = N'ExtraProperties');
IF @var112 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryName] DROP CONSTRAINT [' + @var112 + '];');
UPDATE [Chat_EntryName] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_EntryName] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_EntryName] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var113 sysname;
SELECT @var113 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryName]') AND [c].[name] = N'ConcurrencyStamp');
IF @var113 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryName] DROP CONSTRAINT [' + @var113 + '];');
UPDATE [Chat_EntryName] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_EntryName] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_EntryName] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var114 sysname;
SELECT @var114 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Developer]') AND [c].[name] = N'ExtraProperties');
IF @var114 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Developer] DROP CONSTRAINT [' + @var114 + '];');
UPDATE [Chat_Developer] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Developer] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Developer] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var115 sysname;
SELECT @var115 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Developer]') AND [c].[name] = N'ConcurrencyStamp');
IF @var115 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Developer] DROP CONSTRAINT [' + @var115 + '];');
UPDATE [Chat_Developer] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Developer] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Developer] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var116 sysname;
SELECT @var116 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ContactTag]') AND [c].[name] = N'ExtraProperties');
IF @var116 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ContactTag] DROP CONSTRAINT [' + @var116 + '];');
UPDATE [Chat_ContactTag] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ContactTag] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ContactTag] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var117 sysname;
SELECT @var117 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ContactTag]') AND [c].[name] = N'ConcurrencyStamp');
IF @var117 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ContactTag] DROP CONSTRAINT [' + @var117 + '];');
UPDATE [Chat_ContactTag] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ContactTag] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ContactTag] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var118 sysname;
SELECT @var118 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ConnectionChatObject]') AND [c].[name] = N'ExtraProperties');
IF @var118 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ConnectionChatObject] DROP CONSTRAINT [' + @var118 + '];');
UPDATE [Chat_ConnectionChatObject] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ConnectionChatObject] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ConnectionChatObject] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var119 sysname;
SELECT @var119 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ConnectionChatObject]') AND [c].[name] = N'ConcurrencyStamp');
IF @var119 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ConnectionChatObject] DROP CONSTRAINT [' + @var119 + '];');
UPDATE [Chat_ConnectionChatObject] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ConnectionChatObject] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ConnectionChatObject] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var120 sysname;
SELECT @var120 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Connection]') AND [c].[name] = N'ExtraProperties');
IF @var120 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Connection] DROP CONSTRAINT [' + @var120 + '];');
UPDATE [Chat_Connection] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Connection] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Connection] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var121 sysname;
SELECT @var121 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Connection]') AND [c].[name] = N'ConcurrencyStamp');
IF @var121 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Connection] DROP CONSTRAINT [' + @var121 + '];');
UPDATE [Chat_Connection] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Connection] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Connection] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var122 sysname;
SELECT @var122 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ClientConfig]') AND [c].[name] = N'ExtraProperties');
IF @var122 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ClientConfig] DROP CONSTRAINT [' + @var122 + '];');
UPDATE [Chat_ClientConfig] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ClientConfig] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ClientConfig] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var123 sysname;
SELECT @var123 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ClientConfig]') AND [c].[name] = N'ConcurrencyStamp');
IF @var123 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ClientConfig] DROP CONSTRAINT [' + @var123 + '];');
UPDATE [Chat_ClientConfig] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ClientConfig] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ClientConfig] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var124 sysname;
SELECT @var124 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectType]') AND [c].[name] = N'ExtraProperties');
IF @var124 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectType] DROP CONSTRAINT [' + @var124 + '];');
UPDATE [Chat_ChatObjectType] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ChatObjectType] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ChatObjectType] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var125 sysname;
SELECT @var125 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectType]') AND [c].[name] = N'ConcurrencyStamp');
IF @var125 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectType] DROP CONSTRAINT [' + @var125 + '];');
UPDATE [Chat_ChatObjectType] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ChatObjectType] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ChatObjectType] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var126 sysname;
SELECT @var126 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectEntryValue]') AND [c].[name] = N'ExtraProperties');
IF @var126 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectEntryValue] DROP CONSTRAINT [' + @var126 + '];');
UPDATE [Chat_ChatObjectEntryValue] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ChatObjectEntryValue] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ChatObjectEntryValue] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var127 sysname;
SELECT @var127 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectEntryValue]') AND [c].[name] = N'ConcurrencyStamp');
IF @var127 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectEntryValue] DROP CONSTRAINT [' + @var127 + '];');
UPDATE [Chat_ChatObjectEntryValue] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ChatObjectEntryValue] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ChatObjectEntryValue] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var128 sysname;
SELECT @var128 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectCategoryUnit]') AND [c].[name] = N'ExtraProperties');
IF @var128 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectCategoryUnit] DROP CONSTRAINT [' + @var128 + '];');
UPDATE [Chat_ChatObjectCategoryUnit] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ChatObjectCategoryUnit] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ChatObjectCategoryUnit] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var129 sysname;
SELECT @var129 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectCategoryUnit]') AND [c].[name] = N'ConcurrencyStamp');
IF @var129 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectCategoryUnit] DROP CONSTRAINT [' + @var129 + '];');
UPDATE [Chat_ChatObjectCategoryUnit] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ChatObjectCategoryUnit] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ChatObjectCategoryUnit] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var130 sysname;
SELECT @var130 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectCategory]') AND [c].[name] = N'ExtraProperties');
IF @var130 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectCategory] DROP CONSTRAINT [' + @var130 + '];');
UPDATE [Chat_ChatObjectCategory] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ChatObjectCategory] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ChatObjectCategory] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var131 sysname;
SELECT @var131 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectCategory]') AND [c].[name] = N'ConcurrencyStamp');
IF @var131 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectCategory] DROP CONSTRAINT [' + @var131 + '];');
UPDATE [Chat_ChatObjectCategory] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ChatObjectCategory] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ChatObjectCategory] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var132 sysname;
SELECT @var132 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObject]') AND [c].[name] = N'ExtraProperties');
IF @var132 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObject] DROP CONSTRAINT [' + @var132 + '];');
UPDATE [Chat_ChatObject] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ChatObject] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ChatObject] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var133 sysname;
SELECT @var133 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObject]') AND [c].[name] = N'ConcurrencyStamp');
IF @var133 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObject] DROP CONSTRAINT [' + @var133 + '];');
UPDATE [Chat_ChatObject] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ChatObject] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ChatObject] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var134 sysname;
SELECT @var134 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_BlobContent]') AND [c].[name] = N'ExtraProperties');
IF @var134 IS NOT NULL EXEC(N'ALTER TABLE [Chat_BlobContent] DROP CONSTRAINT [' + @var134 + '];');
UPDATE [Chat_BlobContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_BlobContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_BlobContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var135 sysname;
SELECT @var135 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_BlobContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var135 IS NOT NULL EXEC(N'ALTER TABLE [Chat_BlobContent] DROP CONSTRAINT [' + @var135 + '];');
UPDATE [Chat_BlobContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_BlobContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_BlobContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var136 sysname;
SELECT @var136 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Blob]') AND [c].[name] = N'ExtraProperties');
IF @var136 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Blob] DROP CONSTRAINT [' + @var136 + '];');
UPDATE [Chat_Blob] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Blob] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Blob] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var137 sysname;
SELECT @var137 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Blob]') AND [c].[name] = N'ConcurrencyStamp');
IF @var137 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Blob] DROP CONSTRAINT [' + @var137 + '];');
UPDATE [Chat_Blob] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Blob] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Blob] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var138 sysname;
SELECT @var138 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ArticleMessage]') AND [c].[name] = N'ExtraProperties');
IF @var138 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ArticleMessage] DROP CONSTRAINT [' + @var138 + '];');
UPDATE [Chat_ArticleMessage] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ArticleMessage] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ArticleMessage] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var139 sysname;
SELECT @var139 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ArticleMessage]') AND [c].[name] = N'ConcurrencyStamp');
IF @var139 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ArticleMessage] DROP CONSTRAINT [' + @var139 + '];');
UPDATE [Chat_ArticleMessage] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ArticleMessage] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ArticleMessage] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var140 sysname;
SELECT @var140 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Article]') AND [c].[name] = N'ExtraProperties');
IF @var140 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Article] DROP CONSTRAINT [' + @var140 + '];');
UPDATE [Chat_Article] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Article] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Article] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var141 sysname;
SELECT @var141 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Article]') AND [c].[name] = N'ConcurrencyStamp');
IF @var141 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Article] DROP CONSTRAINT [' + @var141 + '];');
UPDATE [Chat_Article] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Article] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Article] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240717034714_Abp8.2', N'8.0.7');
GO

COMMIT;
GO

