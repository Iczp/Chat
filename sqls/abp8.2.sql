IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_Article] (
    [Id] uniqueidentifier NOT NULL,
    [ArticleType] int NOT NULL,
    [EditorType] int NOT NULL,
    [Title] nvarchar(256) NOT NULL,
    [Description] nvarchar(256) NULL,
    [CoverImageUrl] nvarchar(500) NULL,
    [Content] nvarchar(max) NULL,
    [Author] nvarchar(50) NULL,
    [VisitsCount] bigint NOT NULL,
    [OriginalUrl] nvarchar(500) NULL,
    [OwnerUserId] uniqueidentifier NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_Article] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Chat_ChatObjectType] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(500) NULL,
    [MaxDepth] int NOT NULL,
    [IsHasChild] bit NOT NULL,
    [IsStatic] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_ChatObjectType] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Chat_Connection] (
    [Id] uniqueidentifier NOT NULL,
    [AppUserId] uniqueidentifier NULL,
    [ChatObjectId] uniqueidentifier NULL,
    [Server] nvarchar(200) NULL,
    [DeviceId] nvarchar(50) NULL,
    [Ip] nvarchar(36) NULL,
    [Agent] nvarchar(300) NULL,
    [ActiveTime] datetime2 NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_Connection] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Chat_MessageContent] (
    [Id] uniqueidentifier NOT NULL,
    [ContentType] nvarchar(50) NULL,
    [Body] nvarchar(max) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_MessageContent] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Chat_PaymentPlatform] (
    [Id] nvarchar(64) NOT NULL,
    [Name] nvarchar(50) NULL,
    [Description] nvarchar(100) NULL,
    [IsEnabled] bit NOT NULL,
    [IsStatic] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_PaymentPlatform] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Chat_WalletBusiness] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(50) NULL,
    [BusinessType] int NOT NULL,
    [Description] nvarchar(100) NULL,
    [IsEnabled] bit NOT NULL,
    [IsStatic] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_WalletBusiness] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Chat_ChatObject] (
    [Id] bigint NOT NULL IDENTITY,
    [TypeName] nvarchar(50) NULL,
    [MaxMessageAutoId] bigint NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Code] nvarchar(50) NULL,
    [Portrait] nvarchar(300) NULL,
    [AppUserId] uniqueidentifier NULL,
    [ObjectType] int NULL,
    [Description] nvarchar(500) NULL,
    [IsStatic] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [ChatObjectTypeId] nvarchar(450) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [ParentId] bigint NULL,
    [FullPath] nvarchar(1000) NOT NULL,
    [FullPathName] nvarchar(1000) NOT NULL,
    [Depth] int NOT NULL,
    [Sorting] float NOT NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_ChatObject] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_ChatObject_Chat_ChatObjectType_ChatObjectTypeId] FOREIGN KEY ([ChatObjectTypeId]) REFERENCES [Chat_ChatObjectType] ([Id]),
    CONSTRAINT [FK_Chat_ChatObject_Chat_ChatObject_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_ChatObjectCategory] (
    [Id] uniqueidentifier NOT NULL,
    [ChatObjectTypeId] nvarchar(450) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [Name] nvarchar(64) NOT NULL,
    [ParentId] uniqueidentifier NULL,
    [FullPath] nvarchar(1000) NOT NULL,
    [FullPathName] nvarchar(1000) NOT NULL,
    [Depth] int NOT NULL,
    [Sorting] float NOT NULL,
    [Description] nvarchar(500) NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_ChatObjectCategory] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_ChatObjectCategory_Chat_ChatObjectCategory_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Chat_ChatObjectCategory] ([Id]),
    CONSTRAINT [FK_Chat_ChatObjectCategory_Chat_ChatObjectType_ChatObjectTypeId] FOREIGN KEY ([ChatObjectTypeId]) REFERENCES [Chat_ChatObjectType] ([Id])
);
GO

CREATE TABLE [Chat_Favorite] (
    [Id] uniqueidentifier NOT NULL,
    [OwnerId] bigint NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_Favorite] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Favorite_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_FriendshipRequest] (
    [Id] uniqueidentifier NOT NULL,
    [DeviceId] nvarchar(36) NULL,
    [Message] nvarchar(200) NULL,
    [IsHandled] bit NOT NULL,
    [IsAgreed] bit NULL,
    [HandlMessage] nvarchar(200) NULL,
    [HandlTime] datetime2 NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [OwnerId] bigint NOT NULL,
    [DestinationId] bigint NULL,
    CONSTRAINT [PK_Chat_FriendshipRequest] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_FriendshipRequest_Chat_ChatObject_DestinationId] FOREIGN KEY ([DestinationId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_FriendshipRequest_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_FriendshipTag] (
    [Id] uniqueidentifier NOT NULL,
    [OwnerId] bigint NULL,
    [Name] nvarchar(20) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_FriendshipTag] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_FriendshipTag_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_Message_Template_ArticleContent] (
    [Id] uniqueidentifier NOT NULL,
    [ArticleType] int NOT NULL,
    [EditorType] int NOT NULL,
    [Title] nvarchar(256) NOT NULL,
    [Description] nvarchar(256) NULL,
    [CoverImageUrl] nvarchar(500) NULL,
    [Content] nvarchar(max) NULL,
    [Author] nvarchar(50) NULL,
    [VisitsCount] bigint NOT NULL,
    [OriginalUrl] nvarchar(500) NULL,
    [CreatorUserId] nvarchar(36) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [OwnerId] bigint NULL,
    CONSTRAINT [PK_Chat_Message_Template_ArticleContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Template_ArticleContent_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_Message_Template_CmdContent] (
    [Id] uniqueidentifier NOT NULL,
    [Cmd] nvarchar(20) NULL,
    [Text] nvarchar(500) NULL,
    [Url] nvarchar(500) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [OwnerId] bigint NULL,
    CONSTRAINT [PK_Chat_Message_Template_CmdContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Template_CmdContent_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_Message_Template_ContactsContent] (
    [Id] uniqueidentifier NOT NULL,
    [DestinationId] bigint NOT NULL,
    [Name] nvarchar(50) NULL,
    [Code] nvarchar(50) NULL,
    [Portrait] nvarchar(300) NULL,
    [ObjectType] int NULL,
    [Description] nvarchar(200) NULL,
    [Remark] nvarchar(200) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [OwnerId] bigint NULL,
    CONSTRAINT [PK_Chat_Message_Template_ContactsContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Template_ContactsContent_Chat_ChatObject_DestinationId] FOREIGN KEY ([DestinationId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_Template_ContactsContent_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_Message_Template_FileContent] (
    [Id] uniqueidentifier NOT NULL,
    [Url] nvarchar(500) NULL,
    [FileName] nvarchar(256) NULL,
    [ActionUrl] nvarchar(500) NULL,
    [ContentType] nvarchar(100) NULL,
    [Suffix] nvarchar(10) NULL,
    [ContentLength] bigint NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [OwnerId] bigint NULL,
    CONSTRAINT [PK_Chat_Message_Template_FileContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Template_FileContent_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_Message_Template_HistoryContent] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(256) NOT NULL,
    [Description] nvarchar(500) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [OwnerId] bigint NULL,
    CONSTRAINT [PK_Chat_Message_Template_HistoryContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Template_HistoryContent_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_Message_Template_HtmlContent] (
    [Id] uniqueidentifier NOT NULL,
    [EditorType] int NOT NULL,
    [Title] nvarchar(256) NULL,
    [Content] nvarchar(max) NULL,
    [OriginalUrl] nvarchar(500) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [OwnerId] bigint NULL,
    CONSTRAINT [PK_Chat_Message_Template_HtmlContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Template_HtmlContent_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_Message_Template_ImageContent] (
    [Id] uniqueidentifier NOT NULL,
    [Url] nvarchar(500) NULL,
    [ActionUrl] nvarchar(500) NULL,
    [ThumbnailUrl] nvarchar(500) NULL,
    [ThumbnailActionUrl] nvarchar(500) NULL,
    [Orientation] nvarchar(36) NULL,
    [Width] int NULL,
    [Height] int NULL,
    [Size] int NULL,
    [Qrcode] nvarchar(500) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [OwnerId] bigint NULL,
    CONSTRAINT [PK_Chat_Message_Template_ImageContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Template_ImageContent_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_Message_Template_LinkContent] (
    [Id] uniqueidentifier NOT NULL,
    [Url] nvarchar(500) NOT NULL,
    [Title] nvarchar(256) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Image] nvarchar(500) NULL,
    [IssuerName] nvarchar(256) NULL,
    [IssuerIcon] nvarchar(500) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [OwnerId] bigint NULL,
    CONSTRAINT [PK_Chat_Message_Template_LinkContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Template_LinkContent_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_Message_Template_LocationContent] (
    [Id] uniqueidentifier NOT NULL,
    [Provider] nvarchar(100) NULL,
    [Name] nvarchar(100) NOT NULL,
    [Address] nvarchar(200) NULL,
    [Image] nvarchar(500) NULL,
    [Latitude] real NOT NULL,
    [Longitude] real NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [OwnerId] bigint NULL,
    CONSTRAINT [PK_Chat_Message_Template_LocationContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Template_LocationContent_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_Message_Template_RedEnvelopeContent] (
    [Id] uniqueidentifier NOT NULL,
    [OwnerId] bigint NULL,
    [GrantMode] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Count] int NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Text] nvarchar(256) NULL,
    [ExpireTime] datetime2 NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Chat_Message_Template_RedEnvelopeContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Template_RedEnvelopeContent_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_Message_Template_SoundContent] (
    [Id] uniqueidentifier NOT NULL,
    [Url] nvarchar(500) NOT NULL,
    [Path] nvarchar(500) NULL,
    [Text] nvarchar(500) NULL,
    [Time] int NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [OwnerId] bigint NULL,
    CONSTRAINT [PK_Chat_Message_Template_SoundContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Template_SoundContent_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_Message_Template_TextContent] (
    [Id] uniqueidentifier NOT NULL,
    [Text] nvarchar(500) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [OwnerId] bigint NULL,
    CONSTRAINT [PK_Chat_Message_Template_TextContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Template_TextContent_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_Message_Template_VideoContent] (
    [Id] uniqueidentifier NOT NULL,
    [Url] nvarchar(256) NOT NULL,
    [Width] int NULL,
    [Height] int NULL,
    [Size] int NULL,
    [ImageUrl] nvarchar(256) NULL,
    [ImageWidth] int NULL,
    [ImageHeight] int NULL,
    [ImageSize] int NULL,
    [Duration] int NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [OwnerId] bigint NULL,
    CONSTRAINT [PK_Chat_Message_Template_VideoContent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Template_VideoContent_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_SessionSetting] (
    [Id] uniqueidentifier NOT NULL,
    [Rename] nvarchar(50) NULL,
    [Remarks] nvarchar(500) NULL,
    [IsCantacts] bit NOT NULL,
    [SortingNumber] bigint NULL,
    [IsImmersed] bit NOT NULL,
    [IsShowMemberName] bit NOT NULL,
    [IsShowRead] bit NOT NULL,
    [BackgroundImage] nvarchar(500) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [OwnerId] bigint NOT NULL,
    [DestinationId] bigint NULL,
    CONSTRAINT [PK_Chat_SessionSetting] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_SessionSetting_Chat_ChatObject_DestinationId] FOREIGN KEY ([DestinationId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_SessionSetting_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Wallet] (
    [Id] uniqueidentifier NOT NULL,
    [AppUserId] uniqueidentifier NULL,
    [OwnerId] bigint NOT NULL,
    [AvailableAmount] decimal(18,2) NOT NULL,
    [LockAmount] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Password] nvarchar(100) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_Wallet] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Wallet_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_ChatObjectCategoryUnit] (
    [ChatObjectId] bigint NOT NULL,
    [CategoryId] uniqueidentifier NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_ChatObjectCategoryUnit] PRIMARY KEY ([ChatObjectId], [CategoryId]),
    CONSTRAINT [FK_Chat_ChatObjectCategoryUnit_Chat_ChatObjectCategory_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Chat_ChatObjectCategory] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_ChatObjectCategoryUnit_Chat_ChatObject_ChatObjectId] FOREIGN KEY ([ChatObjectId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Friendship] (
    [Id] uniqueidentifier NOT NULL,
    [RequestId] uniqueidentifier NULL,
    [Rename] nvarchar(50) NULL,
    [Remarks] nvarchar(500) NULL,
    [IsCantacts] bit NOT NULL,
    [SortingNumber] bigint NULL,
    [IsImmersed] bit NOT NULL,
    [IsShowMemberName] bit NOT NULL,
    [IsShowRead] bit NOT NULL,
    [BackgroundImage] nvarchar(500) NULL,
    [IsPassive] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [OwnerId] bigint NOT NULL,
    [DestinationId] bigint NULL,
    CONSTRAINT [PK_Chat_Friendship] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Friendship_Chat_ChatObject_DestinationId] FOREIGN KEY ([DestinationId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_Friendship_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Friendship_Chat_FriendshipRequest_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [Chat_FriendshipRequest] ([Id])
);
GO

CREATE TABLE [Chat_RedEnvelopeUnit] (
    [Id] uniqueidentifier NOT NULL,
    [RedEnvelopeContentId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [IsTop] bit NOT NULL,
    [IsOwned] bit NOT NULL,
    [OwnerId] bigint NULL,
    [OwnedTime] datetime2 NULL,
    [RollbackTime] datetime2 NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_RedEnvelopeUnit] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_RedEnvelopeUnit_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_RedEnvelopeUnit_Chat_Message_Template_RedEnvelopeContent_RedEnvelopeContentId] FOREIGN KEY ([RedEnvelopeContentId]) REFERENCES [Chat_Message_Template_RedEnvelopeContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_WalletRecorder] (
    [Id] uniqueidentifier NOT NULL,
    [AutoId] bigint NOT NULL IDENTITY,
    [OwnerId] bigint NULL,
    [WalletId] uniqueidentifier NULL,
    [WalletBusinessId] nvarchar(450) NULL,
    [WalletBusinessType] int NOT NULL,
    [AvailableAmountBeforeChange] decimal(18,2) NOT NULL,
    [TotalAmountBeforeChange] decimal(18,2) NOT NULL,
    [LockAmountBeforeChange] decimal(18,2) NOT NULL,
    [AvailableAmount] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [LockAmount] decimal(18,2) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Description] nvarchar(100) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_WalletRecorder] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_WalletRecorder_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_WalletRecorder_Chat_WalletBusiness_WalletBusinessId] FOREIGN KEY ([WalletBusinessId]) REFERENCES [Chat_WalletBusiness] ([Id]),
    CONSTRAINT [FK_Chat_WalletRecorder_Chat_Wallet_WalletId] FOREIGN KEY ([WalletId]) REFERENCES [Chat_Wallet] ([Id])
);
GO

CREATE TABLE [Chat_FriendshipTagUnit] (
    [FriendshipId] uniqueidentifier NOT NULL,
    [FriendshipTagId] uniqueidentifier NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_FriendshipTagUnit] PRIMARY KEY ([FriendshipId], [FriendshipTagId]),
    CONSTRAINT [FK_Chat_FriendshipTagUnit_Chat_FriendshipTag_FriendshipTagId] FOREIGN KEY ([FriendshipTagId]) REFERENCES [Chat_FriendshipTag] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_FriendshipTagUnit_Chat_Friendship_FriendshipId] FOREIGN KEY ([FriendshipId]) REFERENCES [Chat_Friendship] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_WalletRequest] (
    [Id] uniqueidentifier NOT NULL,
    [OwnerId] bigint NOT NULL,
    [WalletRecorderId] uniqueidentifier NOT NULL,
    [WalletBusinessId] nvarchar(450) NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaymentPlatformId] nvarchar(64) NULL,
    [Descripton] nvarchar(100) NULL,
    [IsPosting] bit NOT NULL,
    [PostDate] datetime2 NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_WalletRequest] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_WalletRequest_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_WalletRequest_Chat_PaymentPlatform_PaymentPlatformId] FOREIGN KEY ([PaymentPlatformId]) REFERENCES [Chat_PaymentPlatform] ([Id]),
    CONSTRAINT [FK_Chat_WalletRequest_Chat_WalletBusiness_WalletBusinessId] FOREIGN KEY ([WalletBusinessId]) REFERENCES [Chat_WalletBusiness] ([Id]),
    CONSTRAINT [FK_Chat_WalletRequest_Chat_WalletRecorder_WalletRecorderId] FOREIGN KEY ([WalletRecorderId]) REFERENCES [Chat_WalletRecorder] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_ArticleMessage] (
    [ArticleId] uniqueidentifier NOT NULL,
    [MessageId] bigint NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_ArticleMessage] PRIMARY KEY ([MessageId], [ArticleId]),
    CONSTRAINT [FK_Chat_ArticleMessage_Chat_Article_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Chat_Article] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_FavoriteMessage] (
    [FavoriteId] uniqueidentifier NOT NULL,
    [MessageId] bigint NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_FavoriteMessage] PRIMARY KEY ([MessageId], [FavoriteId]),
    CONSTRAINT [FK_Chat_FavoriteMessage_Chat_Favorite_FavoriteId] FOREIGN KEY ([FavoriteId]) REFERENCES [Chat_Favorite] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_HistoryMessage] (
    [HistoryContentId] uniqueidentifier NOT NULL,
    [MessageId] bigint NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_HistoryMessage] PRIMARY KEY ([MessageId], [HistoryContentId]),
    CONSTRAINT [FK_Chat_HistoryMessage_Chat_Message_Template_HistoryContent_HistoryContentId] FOREIGN KEY ([HistoryContentId]) REFERENCES [Chat_Message_Template_HistoryContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Message] (
    [Id] bigint NOT NULL IDENTITY,
    [AutoId] bigint NOT NULL,
    [SessionKey] nvarchar(100) NULL,
    [SessionId] uniqueidentifier NULL,
    [SessionUnitCount] int NOT NULL,
    [MessageContentId] uniqueidentifier NULL,
    [Provider] nvarchar(100) NULL,
    [SenderId] bigint NULL,
    [SenderType] int NULL,
    [ReceiverId] bigint NULL,
    [ReceiverType] int NULL,
    [Channel] int NOT NULL,
    [MessageType] int NOT NULL,
    [ContentJson] nvarchar(max) NULL,
    [IsRemindAll] bit NOT NULL,
    [KeyName] nvarchar(100) NULL,
    [KeyValue] nvarchar(max) NULL,
    [IsRollbacked] bit NOT NULL,
    [RollbackTime] datetime2 NULL,
    [ForwardMessageId] bigint NULL,
    [ForwardDepth] bigint NOT NULL,
    [ForwardPath] nvarchar(1000) NULL,
    [QuoteMessageId] bigint NULL,
    [QuoteDepth] bigint NOT NULL,
    [QuotePath] nvarchar(1000) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_Message] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Message_Chat_ChatObject_ReceiverId] FOREIGN KEY ([ReceiverId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_Message_Chat_ChatObject_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_Message_Chat_MessageContent_MessageContentId] FOREIGN KEY ([MessageContentId]) REFERENCES [Chat_MessageContent] ([Id]),
    CONSTRAINT [FK_Chat_Message_Chat_Message_ForwardMessageId] FOREIGN KEY ([ForwardMessageId]) REFERENCES [Chat_Message] ([Id]),
    CONSTRAINT [FK_Chat_Message_Chat_Message_QuoteMessageId] FOREIGN KEY ([QuoteMessageId]) REFERENCES [Chat_Message] ([Id])
);
GO

CREATE TABLE [Chat_Message_MapTo_ArticleContent] (
    [ArticleContentListId] uniqueidentifier NOT NULL,
    [MessageListId] bigint NOT NULL,
    CONSTRAINT [PK_Chat_Message_MapTo_ArticleContent] PRIMARY KEY ([ArticleContentListId], [MessageListId]),
    CONSTRAINT [FK_Chat_Message_MapTo_ArticleContent_Chat_Message_MessageListId] FOREIGN KEY ([MessageListId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_MapTo_ArticleContent_Chat_Message_Template_ArticleContent_ArticleContentListId] FOREIGN KEY ([ArticleContentListId]) REFERENCES [Chat_Message_Template_ArticleContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Message_MapTo_CmdContent] (
    [CmdContentListId] uniqueidentifier NOT NULL,
    [MessageListId] bigint NOT NULL,
    CONSTRAINT [PK_Chat_Message_MapTo_CmdContent] PRIMARY KEY ([CmdContentListId], [MessageListId]),
    CONSTRAINT [FK_Chat_Message_MapTo_CmdContent_Chat_Message_MessageListId] FOREIGN KEY ([MessageListId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_MapTo_CmdContent_Chat_Message_Template_CmdContent_CmdContentListId] FOREIGN KEY ([CmdContentListId]) REFERENCES [Chat_Message_Template_CmdContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Message_MapTo_ContactsContent] (
    [ContactsContentListId] uniqueidentifier NOT NULL,
    [MessageListId] bigint NOT NULL,
    CONSTRAINT [PK_Chat_Message_MapTo_ContactsContent] PRIMARY KEY ([ContactsContentListId], [MessageListId]),
    CONSTRAINT [FK_Chat_Message_MapTo_ContactsContent_Chat_Message_MessageListId] FOREIGN KEY ([MessageListId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_MapTo_ContactsContent_Chat_Message_Template_ContactsContent_ContactsContentListId] FOREIGN KEY ([ContactsContentListId]) REFERENCES [Chat_Message_Template_ContactsContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Message_MapTo_FileContent] (
    [FileContentListId] uniqueidentifier NOT NULL,
    [MessageListId] bigint NOT NULL,
    CONSTRAINT [PK_Chat_Message_MapTo_FileContent] PRIMARY KEY ([FileContentListId], [MessageListId]),
    CONSTRAINT [FK_Chat_Message_MapTo_FileContent_Chat_Message_MessageListId] FOREIGN KEY ([MessageListId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_MapTo_FileContent_Chat_Message_Template_FileContent_FileContentListId] FOREIGN KEY ([FileContentListId]) REFERENCES [Chat_Message_Template_FileContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Message_MapTo_HistoryContent] (
    [HistoryContentListId] uniqueidentifier NOT NULL,
    [MessageListId] bigint NOT NULL,
    CONSTRAINT [PK_Chat_Message_MapTo_HistoryContent] PRIMARY KEY ([HistoryContentListId], [MessageListId]),
    CONSTRAINT [FK_Chat_Message_MapTo_HistoryContent_Chat_Message_MessageListId] FOREIGN KEY ([MessageListId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_MapTo_HistoryContent_Chat_Message_Template_HistoryContent_HistoryContentListId] FOREIGN KEY ([HistoryContentListId]) REFERENCES [Chat_Message_Template_HistoryContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Message_MapTo_HtmlContent] (
    [HtmlContentListId] uniqueidentifier NOT NULL,
    [MessageListId] bigint NOT NULL,
    CONSTRAINT [PK_Chat_Message_MapTo_HtmlContent] PRIMARY KEY ([HtmlContentListId], [MessageListId]),
    CONSTRAINT [FK_Chat_Message_MapTo_HtmlContent_Chat_Message_MessageListId] FOREIGN KEY ([MessageListId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_MapTo_HtmlContent_Chat_Message_Template_HtmlContent_HtmlContentListId] FOREIGN KEY ([HtmlContentListId]) REFERENCES [Chat_Message_Template_HtmlContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Message_MapTo_ImageContent] (
    [ImageContentListId] uniqueidentifier NOT NULL,
    [MessageListId] bigint NOT NULL,
    CONSTRAINT [PK_Chat_Message_MapTo_ImageContent] PRIMARY KEY ([ImageContentListId], [MessageListId]),
    CONSTRAINT [FK_Chat_Message_MapTo_ImageContent_Chat_Message_MessageListId] FOREIGN KEY ([MessageListId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_MapTo_ImageContent_Chat_Message_Template_ImageContent_ImageContentListId] FOREIGN KEY ([ImageContentListId]) REFERENCES [Chat_Message_Template_ImageContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Message_MapTo_LinkContent] (
    [LinkContentListId] uniqueidentifier NOT NULL,
    [MessageListId] bigint NOT NULL,
    CONSTRAINT [PK_Chat_Message_MapTo_LinkContent] PRIMARY KEY ([LinkContentListId], [MessageListId]),
    CONSTRAINT [FK_Chat_Message_MapTo_LinkContent_Chat_Message_MessageListId] FOREIGN KEY ([MessageListId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_MapTo_LinkContent_Chat_Message_Template_LinkContent_LinkContentListId] FOREIGN KEY ([LinkContentListId]) REFERENCES [Chat_Message_Template_LinkContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Message_MapTo_LocationContent] (
    [LocationContentListId] uniqueidentifier NOT NULL,
    [MessageListId] bigint NOT NULL,
    CONSTRAINT [PK_Chat_Message_MapTo_LocationContent] PRIMARY KEY ([LocationContentListId], [MessageListId]),
    CONSTRAINT [FK_Chat_Message_MapTo_LocationContent_Chat_Message_MessageListId] FOREIGN KEY ([MessageListId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_MapTo_LocationContent_Chat_Message_Template_LocationContent_LocationContentListId] FOREIGN KEY ([LocationContentListId]) REFERENCES [Chat_Message_Template_LocationContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Message_MapTo_RedEnvelopeContent] (
    [MessageListId] bigint NOT NULL,
    [RedEnvelopeContentListId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Chat_Message_MapTo_RedEnvelopeContent] PRIMARY KEY ([MessageListId], [RedEnvelopeContentListId]),
    CONSTRAINT [FK_Chat_Message_MapTo_RedEnvelopeContent_Chat_Message_MessageListId] FOREIGN KEY ([MessageListId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_MapTo_RedEnvelopeContent_Chat_Message_Template_RedEnvelopeContent_RedEnvelopeContentListId] FOREIGN KEY ([RedEnvelopeContentListId]) REFERENCES [Chat_Message_Template_RedEnvelopeContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Message_MapTo_SoundContent] (
    [MessageListId] bigint NOT NULL,
    [SoundContentListId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Chat_Message_MapTo_SoundContent] PRIMARY KEY ([MessageListId], [SoundContentListId]),
    CONSTRAINT [FK_Chat_Message_MapTo_SoundContent_Chat_Message_MessageListId] FOREIGN KEY ([MessageListId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_MapTo_SoundContent_Chat_Message_Template_SoundContent_SoundContentListId] FOREIGN KEY ([SoundContentListId]) REFERENCES [Chat_Message_Template_SoundContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Message_MapTo_TextContent] (
    [MessageListId] bigint NOT NULL,
    [TextContentListId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Chat_Message_MapTo_TextContent] PRIMARY KEY ([MessageListId], [TextContentListId]),
    CONSTRAINT [FK_Chat_Message_MapTo_TextContent_Chat_Message_MessageListId] FOREIGN KEY ([MessageListId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_MapTo_TextContent_Chat_Message_Template_TextContent_TextContentListId] FOREIGN KEY ([TextContentListId]) REFERENCES [Chat_Message_Template_TextContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_Message_MapTo_VideoContent] (
    [MessageListId] bigint NOT NULL,
    [VideoContentListId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Chat_Message_MapTo_VideoContent] PRIMARY KEY ([MessageListId], [VideoContentListId]),
    CONSTRAINT [FK_Chat_Message_MapTo_VideoContent_Chat_Message_MessageListId] FOREIGN KEY ([MessageListId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Message_MapTo_VideoContent_Chat_Message_Template_VideoContent_VideoContentListId] FOREIGN KEY ([VideoContentListId]) REFERENCES [Chat_Message_Template_VideoContent] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_OpenedRecorder] (
    [Id] uniqueidentifier NOT NULL,
    [DeviceId] nvarchar(36) NULL,
    [MessageAutoId] bigint NULL,
    [MessageId] bigint NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [OwnerId] bigint NOT NULL,
    [DestinationId] bigint NULL,
    CONSTRAINT [PK_Chat_OpenedRecorder] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_OpenedRecorder_Chat_ChatObject_DestinationId] FOREIGN KEY ([DestinationId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_OpenedRecorder_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_OpenedRecorder_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id])
);
GO

CREATE TABLE [Chat_ReadedRecorder] (
    [Id] uniqueidentifier NOT NULL,
    [DeviceId] nvarchar(36) NULL,
    [MessageAutoId] bigint NULL,
    [MessageId] bigint NULL,
    [SessionId] nvarchar(100) NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [OwnerId] bigint NOT NULL,
    [DestinationId] bigint NULL,
    CONSTRAINT [PK_Chat_ReadedRecorder] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_ReadedRecorder_Chat_ChatObject_DestinationId] FOREIGN KEY ([DestinationId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_ReadedRecorder_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_ReadedRecorder_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id])
);
GO

CREATE TABLE [Chat_Session] (
    [Id] uniqueidentifier NOT NULL,
    [SessionKey] nvarchar(80) NULL,
    [Channel] int NOT NULL,
    [Title] nvarchar(50) NULL,
    [Description] nvarchar(100) NULL,
    [OwnerId] bigint NULL,
    [LastMessageId] bigint NULL,
    [LastMessageAutoId] bigint NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_Session] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Session_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_Session_Chat_Message_LastMessageId] FOREIGN KEY ([LastMessageId]) REFERENCES [Chat_Message] ([Id])
);
GO

CREATE TABLE [Chat_SessionRole] (
    [Id] uniqueidentifier NOT NULL,
    [SessionId] uniqueidentifier NULL,
    [Name] nvarchar(20) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_SessionRole] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_SessionRole_Chat_Session_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Chat_Session] ([Id])
);
GO

CREATE TABLE [Chat_SessionTag] (
    [Id] uniqueidentifier NOT NULL,
    [SessionId] uniqueidentifier NULL,
    [Name] nvarchar(20) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_SessionTag] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_SessionTag_Chat_Session_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Chat_Session] ([Id])
);
GO

CREATE TABLE [Chat_SessionUnit] (
    [Id] uniqueidentifier NOT NULL,
    [SessionId] uniqueidentifier NOT NULL,
    [DestinationObjectType] int NULL,
    [Rename] nvarchar(50) NULL,
    [ReadedMessageAutoId] bigint NOT NULL,
    [LastMessageAutoId] bigint NOT NULL,
    [HistoryFristTime] datetime2 NULL,
    [HistoryLastTime] datetime2 NULL,
    [IsKilled] bit NOT NULL,
    [KillType] int NULL,
    [KillTime] datetime2 NULL,
    [KillerId] bigint NULL,
    [ClearTime] datetime2 NULL,
    [RemoveTime] datetime2 NULL,
    [IsImmersed] bit NOT NULL,
    [IsImportant] bit NOT NULL,
    [JoinWay] int NULL,
    [InviterId] bigint NULL,
    [Sorting] float NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [OwnerId] bigint NOT NULL,
    [DestinationId] bigint NULL,
    CONSTRAINT [PK_Chat_SessionUnit] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_SessionUnit_Chat_ChatObject_DestinationId] FOREIGN KEY ([DestinationId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_SessionUnit_Chat_ChatObject_InviterId] FOREIGN KEY ([InviterId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_SessionUnit_Chat_ChatObject_KillerId] FOREIGN KEY ([KillerId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_SessionUnit_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_SessionUnit_Chat_Session_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Chat_Session] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_MessageReminder] (
    [SessionUnitId] uniqueidentifier NOT NULL,
    [MessageId] bigint NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_MessageReminder] PRIMARY KEY ([MessageId], [SessionUnitId]),
    CONSTRAINT [FK_Chat_MessageReminder_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_MessageReminder_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_SessionUnitRole] (
    [SessionUnitId] uniqueidentifier NOT NULL,
    [SessionRoleId] uniqueidentifier NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_SessionUnitRole] PRIMARY KEY ([SessionUnitId], [SessionRoleId]),
    CONSTRAINT [FK_Chat_SessionUnitRole_Chat_SessionRole_SessionRoleId] FOREIGN KEY ([SessionRoleId]) REFERENCES [Chat_SessionRole] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_SessionUnitRole_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_SessionUnitTag] (
    [SessionUnitId] uniqueidentifier NOT NULL,
    [SessionTagId] uniqueidentifier NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_SessionUnitTag] PRIMARY KEY ([SessionUnitId], [SessionTagId]),
    CONSTRAINT [FK_Chat_SessionUnitTag_Chat_SessionTag_SessionTagId] FOREIGN KEY ([SessionTagId]) REFERENCES [Chat_SessionTag] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_SessionUnitTag_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Chat_ArticleMessage_ArticleId] ON [Chat_ArticleMessage] ([ArticleId]);
GO

CREATE INDEX [IX_Chat_ChatObject_ChatObjectTypeId] ON [Chat_ChatObject] ([ChatObjectTypeId]);
GO

CREATE INDEX [IX_Chat_ChatObject_ParentId] ON [Chat_ChatObject] ([ParentId]);
GO

CREATE INDEX [IX_Chat_ChatObjectCategory_ChatObjectTypeId] ON [Chat_ChatObjectCategory] ([ChatObjectTypeId]);
GO

CREATE INDEX [IX_Chat_ChatObjectCategory_ParentId] ON [Chat_ChatObjectCategory] ([ParentId]);
GO

CREATE INDEX [IX_Chat_ChatObjectCategoryUnit_CategoryId] ON [Chat_ChatObjectCategoryUnit] ([CategoryId]);
GO

CREATE INDEX [IX_Chat_Favorite_OwnerId] ON [Chat_Favorite] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_FavoriteMessage_FavoriteId] ON [Chat_FavoriteMessage] ([FavoriteId]);
GO

CREATE INDEX [IX_Chat_Friendship_DestinationId] ON [Chat_Friendship] ([DestinationId]);
GO

CREATE INDEX [IX_Chat_Friendship_OwnerId] ON [Chat_Friendship] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Friendship_RequestId] ON [Chat_Friendship] ([RequestId]);
GO

CREATE INDEX [IX_Chat_FriendshipRequest_DestinationId] ON [Chat_FriendshipRequest] ([DestinationId]);
GO

CREATE INDEX [IX_Chat_FriendshipRequest_OwnerId] ON [Chat_FriendshipRequest] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_FriendshipTag_OwnerId] ON [Chat_FriendshipTag] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_FriendshipTagUnit_FriendshipTagId] ON [Chat_FriendshipTagUnit] ([FriendshipTagId]);
GO

CREATE INDEX [IX_Chat_HistoryMessage_HistoryContentId] ON [Chat_HistoryMessage] ([HistoryContentId]);
GO

CREATE INDEX [IX_Chat_Message_AutoId] ON [Chat_Message] ([AutoId]);
GO

CREATE INDEX [IX_Chat_Message_ForwardMessageId] ON [Chat_Message] ([ForwardMessageId]);
GO

CREATE INDEX [IX_Chat_Message_MessageContentId] ON [Chat_Message] ([MessageContentId]);
GO

CREATE INDEX [IX_Chat_Message_QuoteMessageId] ON [Chat_Message] ([QuoteMessageId]);
GO

CREATE INDEX [IX_Chat_Message_ReceiverId] ON [Chat_Message] ([ReceiverId]);
GO

CREATE INDEX [IX_Chat_Message_SenderId] ON [Chat_Message] ([SenderId]);
GO

CREATE INDEX [IX_Chat_Message_SessionId] ON [Chat_Message] ([SessionId]);
GO

CREATE INDEX [IX_Chat_Message_SessionUnitCount] ON [Chat_Message] ([SessionUnitCount]);
GO

CREATE INDEX [IX_Chat_Message_MapTo_ArticleContent_MessageListId] ON [Chat_Message_MapTo_ArticleContent] ([MessageListId]);
GO

CREATE INDEX [IX_Chat_Message_MapTo_CmdContent_MessageListId] ON [Chat_Message_MapTo_CmdContent] ([MessageListId]);
GO

CREATE INDEX [IX_Chat_Message_MapTo_ContactsContent_MessageListId] ON [Chat_Message_MapTo_ContactsContent] ([MessageListId]);
GO

CREATE INDEX [IX_Chat_Message_MapTo_FileContent_MessageListId] ON [Chat_Message_MapTo_FileContent] ([MessageListId]);
GO

CREATE INDEX [IX_Chat_Message_MapTo_HistoryContent_MessageListId] ON [Chat_Message_MapTo_HistoryContent] ([MessageListId]);
GO

CREATE INDEX [IX_Chat_Message_MapTo_HtmlContent_MessageListId] ON [Chat_Message_MapTo_HtmlContent] ([MessageListId]);
GO

CREATE INDEX [IX_Chat_Message_MapTo_ImageContent_MessageListId] ON [Chat_Message_MapTo_ImageContent] ([MessageListId]);
GO

CREATE INDEX [IX_Chat_Message_MapTo_LinkContent_MessageListId] ON [Chat_Message_MapTo_LinkContent] ([MessageListId]);
GO

CREATE INDEX [IX_Chat_Message_MapTo_LocationContent_MessageListId] ON [Chat_Message_MapTo_LocationContent] ([MessageListId]);
GO

CREATE INDEX [IX_Chat_Message_MapTo_RedEnvelopeContent_RedEnvelopeContentListId] ON [Chat_Message_MapTo_RedEnvelopeContent] ([RedEnvelopeContentListId]);
GO

CREATE INDEX [IX_Chat_Message_MapTo_SoundContent_SoundContentListId] ON [Chat_Message_MapTo_SoundContent] ([SoundContentListId]);
GO

CREATE INDEX [IX_Chat_Message_MapTo_TextContent_TextContentListId] ON [Chat_Message_MapTo_TextContent] ([TextContentListId]);
GO

CREATE INDEX [IX_Chat_Message_MapTo_VideoContent_VideoContentListId] ON [Chat_Message_MapTo_VideoContent] ([VideoContentListId]);
GO

CREATE INDEX [IX_Chat_Message_Template_ArticleContent_OwnerId] ON [Chat_Message_Template_ArticleContent] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Message_Template_CmdContent_OwnerId] ON [Chat_Message_Template_CmdContent] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Message_Template_ContactsContent_DestinationId] ON [Chat_Message_Template_ContactsContent] ([DestinationId]);
GO

CREATE INDEX [IX_Chat_Message_Template_ContactsContent_OwnerId] ON [Chat_Message_Template_ContactsContent] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Message_Template_FileContent_OwnerId] ON [Chat_Message_Template_FileContent] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Message_Template_HistoryContent_OwnerId] ON [Chat_Message_Template_HistoryContent] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Message_Template_HtmlContent_OwnerId] ON [Chat_Message_Template_HtmlContent] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Message_Template_ImageContent_OwnerId] ON [Chat_Message_Template_ImageContent] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Message_Template_LinkContent_OwnerId] ON [Chat_Message_Template_LinkContent] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Message_Template_LocationContent_OwnerId] ON [Chat_Message_Template_LocationContent] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Message_Template_RedEnvelopeContent_OwnerId] ON [Chat_Message_Template_RedEnvelopeContent] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Message_Template_SoundContent_OwnerId] ON [Chat_Message_Template_SoundContent] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Message_Template_TextContent_OwnerId] ON [Chat_Message_Template_TextContent] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Message_Template_VideoContent_OwnerId] ON [Chat_Message_Template_VideoContent] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_MessageReminder_SessionUnitId] ON [Chat_MessageReminder] ([SessionUnitId]);
GO

CREATE INDEX [IX_Chat_OpenedRecorder_DestinationId] ON [Chat_OpenedRecorder] ([DestinationId]);
GO

CREATE INDEX [IX_Chat_OpenedRecorder_MessageId] ON [Chat_OpenedRecorder] ([MessageId]);
GO

CREATE INDEX [IX_Chat_OpenedRecorder_OwnerId] ON [Chat_OpenedRecorder] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_ReadedRecorder_DestinationId] ON [Chat_ReadedRecorder] ([DestinationId]);
GO

CREATE INDEX [IX_Chat_ReadedRecorder_MessageId] ON [Chat_ReadedRecorder] ([MessageId]);
GO

CREATE INDEX [IX_Chat_ReadedRecorder_OwnerId] ON [Chat_ReadedRecorder] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_RedEnvelopeUnit_OwnerId] ON [Chat_RedEnvelopeUnit] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_RedEnvelopeUnit_RedEnvelopeContentId] ON [Chat_RedEnvelopeUnit] ([RedEnvelopeContentId]);
GO

CREATE INDEX [IX_Chat_Session_LastMessageAutoId] ON [Chat_Session] ([LastMessageAutoId] DESC);
GO

CREATE INDEX [IX_Chat_Session_LastMessageId] ON [Chat_Session] ([LastMessageId]);
GO

CREATE INDEX [IX_Chat_Session_OwnerId] ON [Chat_Session] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Session_SessionKey] ON [Chat_Session] ([SessionKey]);
GO

CREATE INDEX [IX_Chat_SessionRole_SessionId] ON [Chat_SessionRole] ([SessionId]);
GO

CREATE INDEX [IX_Chat_SessionSetting_DestinationId] ON [Chat_SessionSetting] ([DestinationId]);
GO

CREATE INDEX [IX_Chat_SessionSetting_OwnerId] ON [Chat_SessionSetting] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_SessionTag_SessionId] ON [Chat_SessionTag] ([SessionId]);
GO

CREATE INDEX [IX_Chat_SessionUnit_DestinationId] ON [Chat_SessionUnit] ([DestinationId]);
GO

CREATE INDEX [IX_Chat_SessionUnit_InviterId] ON [Chat_SessionUnit] ([InviterId]);
GO

CREATE INDEX [IX_Chat_SessionUnit_KillerId] ON [Chat_SessionUnit] ([KillerId]);
GO

CREATE INDEX [IX_Chat_SessionUnit_LastMessageAutoId] ON [Chat_SessionUnit] ([LastMessageAutoId] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_OwnerId] ON [Chat_SessionUnit] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_SessionUnit_ReadedMessageAutoId] ON [Chat_SessionUnit] ([ReadedMessageAutoId] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_SessionId] ON [Chat_SessionUnit] ([SessionId]);
GO

CREATE INDEX [IX_Chat_SessionUnit_Sorting] ON [Chat_SessionUnit] ([Sorting] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnitRole_SessionRoleId] ON [Chat_SessionUnitRole] ([SessionRoleId]);
GO

CREATE INDEX [IX_Chat_SessionUnitTag_SessionTagId] ON [Chat_SessionUnitTag] ([SessionTagId]);
GO

CREATE INDEX [IX_Chat_Wallet_OwnerId] ON [Chat_Wallet] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_WalletRecorder_OwnerId] ON [Chat_WalletRecorder] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_WalletRecorder_WalletBusinessId] ON [Chat_WalletRecorder] ([WalletBusinessId]);
GO

CREATE INDEX [IX_Chat_WalletRecorder_WalletId] ON [Chat_WalletRecorder] ([WalletId]);
GO

CREATE INDEX [IX_Chat_WalletRequest_OwnerId] ON [Chat_WalletRequest] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_WalletRequest_PaymentPlatformId] ON [Chat_WalletRequest] ([PaymentPlatformId]);
GO

CREATE INDEX [IX_Chat_WalletRequest_WalletBusinessId] ON [Chat_WalletRequest] ([WalletBusinessId]);
GO

CREATE INDEX [IX_Chat_WalletRequest_WalletRecorderId] ON [Chat_WalletRequest] ([WalletRecorderId]);
GO

ALTER TABLE [Chat_ArticleMessage] ADD CONSTRAINT [FK_Chat_ArticleMessage_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [Chat_FavoriteMessage] ADD CONSTRAINT [FK_Chat_FavoriteMessage_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [Chat_HistoryMessage] ADD CONSTRAINT [FK_Chat_HistoryMessage_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [Chat_Message] ADD CONSTRAINT [FK_Chat_Message_Chat_Session_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Chat_Session] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230302061537_Chat_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_Message_AutoId] ON [Chat_Message];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message]') AND [c].[name] = N'AutoId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Chat_Message] DROP COLUMN [AutoId];
GO

CREATE INDEX [IX_Chat_Message_Id] ON [Chat_Message] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230302063635_Message_RemoveProp_AutoId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_Session_LastMessageAutoId] ON [Chat_Session];
GO

DROP INDEX [IX_Chat_Session_LastMessageId] ON [Chat_Session];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Session]') AND [c].[name] = N'LastMessageAutoId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Session] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Chat_Session] DROP COLUMN [LastMessageAutoId];
GO

CREATE INDEX [IX_Chat_Session_LastMessageId] ON [Chat_Session] ([LastMessageId] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230302063953_Session_RemoveProp_LastMessageAutoId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_SessionUnit_LastMessageAutoId] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_ReadedMessageAutoId] ON [Chat_SessionUnit];
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'LastMessageAutoId');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [LastMessageAutoId];
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'ReadedMessageAutoId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [ReadedMessageAutoId];
GO

ALTER TABLE [Chat_SessionUnit] ADD [LastMessageId] bigint NULL;
GO

ALTER TABLE [Chat_SessionUnit] ADD [ReadedMessageId] bigint NULL;
GO

CREATE INDEX [IX_Chat_SessionUnit_LastMessageId] ON [Chat_SessionUnit] ([LastMessageId] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_ReadedMessageId] ON [Chat_SessionUnit] ([ReadedMessageId] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_Sorting_LastMessageId] ON [Chat_SessionUnit] ([Sorting] DESC, [LastMessageId] DESC);
GO

ALTER TABLE [Chat_SessionUnit] ADD CONSTRAINT [FK_Chat_SessionUnit_Chat_Message_LastMessageId] FOREIGN KEY ([LastMessageId]) REFERENCES [Chat_Message] ([Id]);
GO

ALTER TABLE [Chat_SessionUnit] ADD CONSTRAINT [FK_Chat_SessionUnit_Chat_Message_ReadedMessageId] FOREIGN KEY ([ReadedMessageId]) REFERENCES [Chat_Message] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230302065256_SessionUnit_RenameProp_LastMessageId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_Message_Id] ON [Chat_Message];
GO

CREATE INDEX [IX_Chat_Message_Id] ON [Chat_Message] ([Id] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230303031209_Message_AddIndex_Id_Desc', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ChatObjectType] ADD [ObjectType] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230303071338_ChatObjectType_AddProp_ObjectType', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObject]') AND [c].[name] = N'TypeName');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObject] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Chat_ChatObject] DROP COLUMN [TypeName];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230303085227_ChatObject_RemoveProp_TypeName', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230324092307_Abp7_1_0', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [IsCantacts] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_SessionUnit] ADD [IsShowMemberName] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_SessionUnit] ADD [IsShowReaded] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_SessionUnit] ADD [Remarks] nvarchar(500) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230331034029_SessionUnit_AddSettings_Props', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [MemberName] nvarchar(50) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230331034712_SessionUnit_AddProp_MemberName', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [BackgroundImage] nvarchar(500) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230331035026_SessionUnit_AddProp_BackgroundImage', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ChatObject] ADD [IsPublic] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230331091801_ChatObject_AddProp_IsPublic', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [ServiceStatus] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230403005801_SessionUnit_AddProp_ServiceStatus', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [IsPublic] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_SessionUnit] ADD [IsStatic] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230403024443_SessionUnit_AddProp_IsStatic_IsPublic', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectCategory]') AND [c].[name] = N'FullPathName');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectCategory] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Chat_ChatObjectCategory] ALTER COLUMN [FullPathName] nvarchar(1000) NULL;
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectCategory]') AND [c].[name] = N'FullPath');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectCategory] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Chat_ChatObjectCategory] ALTER COLUMN [FullPath] nvarchar(1000) NULL;
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObject]') AND [c].[name] = N'FullPathName');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObject] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Chat_ChatObject] ALTER COLUMN [FullPathName] nvarchar(1000) NULL;
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObject]') AND [c].[name] = N'FullPath');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObject] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [Chat_ChatObject] ALTER COLUMN [FullPath] nvarchar(1000) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230403035514_Tree_RemoveAttribute_Required', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Message] ADD [SessionUnitId] uniqueidentifier NULL;
GO

CREATE INDEX [IX_Chat_Message_SessionUnitId] ON [Chat_Message] ([SessionUnitId]);
GO

ALTER TABLE [Chat_Message] ADD CONSTRAINT [FK_Chat_Message_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230406004853_Message_addProp_SessionUnitId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [InviterUnitId] uniqueidentifier NULL;
GO

CREATE INDEX [IX_Chat_SessionUnit_InviterUnitId] ON [Chat_SessionUnit] ([InviterUnitId]);
GO

ALTER TABLE [Chat_SessionUnit] ADD CONSTRAINT [FK_Chat_SessionUnit_Chat_SessionUnit_InviterUnitId] FOREIGN KEY ([InviterUnitId]) REFERENCES [Chat_SessionUnit] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230407075840_SessionUnit_AddProp_InviterUnitId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [KillerUnitId] uniqueidentifier NULL;
GO

CREATE INDEX [IX_Chat_SessionUnit_KillerUnitId] ON [Chat_SessionUnit] ([KillerUnitId]);
GO

ALTER TABLE [Chat_SessionUnit] ADD CONSTRAINT [FK_Chat_SessionUnit_Chat_SessionUnit_KillerUnitId] FOREIGN KEY ([KillerUnitId]) REFERENCES [Chat_SessionUnit] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230407080452_SessionUnit_AddProp_KillerUnitId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_SessionOrganization] (
    [Id] bigint NOT NULL IDENTITY,
    [SessionId] uniqueidentifier NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [Name] nvarchar(64) NOT NULL,
    [ParentId] bigint NULL,
    [FullPath] nvarchar(1000) NULL,
    [FullPathName] nvarchar(1000) NULL,
    [Depth] int NOT NULL,
    [Sorting] float NOT NULL,
    [Description] nvarchar(500) NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_SessionOrganization] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_SessionOrganization_Chat_SessionOrganization_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Chat_SessionOrganization] ([Id]),
    CONSTRAINT [FK_Chat_SessionOrganization_Chat_Session_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Chat_Session] ([Id])
);
GO

CREATE TABLE [Chat_SessionUnitOrganization] (
    [SessionUnitId] uniqueidentifier NOT NULL,
    [SessionOrganizationId] bigint NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_SessionUnitOrganization] PRIMARY KEY ([SessionUnitId], [SessionOrganizationId]),
    CONSTRAINT [FK_Chat_SessionUnitOrganization_Chat_SessionOrganization_SessionOrganizationId] FOREIGN KEY ([SessionOrganizationId]) REFERENCES [Chat_SessionOrganization] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_SessionUnitOrganization_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Chat_SessionOrganization_ParentId] ON [Chat_SessionOrganization] ([ParentId]);
GO

CREATE INDEX [IX_Chat_SessionOrganization_SessionId] ON [Chat_SessionOrganization] ([SessionId]);
GO

CREATE INDEX [IX_Chat_SessionUnitOrganization_SessionOrganizationId] ON [Chat_SessionUnitOrganization] ([SessionOrganizationId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230407085928_SessionUnitOrganization_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_SessionUnit] ADD [IsInputEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230408075900_SessionUnit_AddProp_IsInputEnabled_IsEnabled', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Message] ADD [IsPrivate] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230410070528_Message_AddProp_IsPrivate', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [IsCreator] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230412010454_SessionUnit_AddProp_IsCreator', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionRole] ADD [Description] nvarchar(500) NULL;
GO

ALTER TABLE [Chat_SessionRole] ADD [IsDefault] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

CREATE TABLE [Chat_SessionPermissionGroup] (
    [Id] bigint NOT NULL IDENTITY,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [Name] nvarchar(64) NOT NULL,
    [ParentId] bigint NULL,
    [FullPath] nvarchar(1000) NULL,
    [FullPathName] nvarchar(1000) NULL,
    [Depth] int NOT NULL,
    [Sorting] float NOT NULL,
    [Description] nvarchar(500) NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_SessionPermissionGroup] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_SessionPermissionGroup_Chat_SessionPermissionGroup_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Chat_SessionPermissionGroup] ([Id])
);
GO

CREATE TABLE [Chat_SessionRequest] (
    [Id] uniqueidentifier NOT NULL,
    [DeviceId] nvarchar(36) NULL,
    [RequestMessage] nvarchar(200) NULL,
    [IsHandled] bit NOT NULL,
    [IsAgreed] bit NULL,
    [HandleMessage] nvarchar(200) NULL,
    [HandleTime] datetime2 NULL,
    [IsEnabled] bit NOT NULL,
    [HandlerId] uniqueidentifier NULL,
    [IsExpired] bit NOT NULL,
    [ExpirationTime] datetime2 NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    [OwnerId] bigint NOT NULL,
    [DestinationId] bigint NULL,
    CONSTRAINT [PK_Chat_SessionRequest] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_SessionRequest_Chat_ChatObject_DestinationId] FOREIGN KEY ([DestinationId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_SessionRequest_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_SessionRequest_Chat_SessionUnit_HandlerId] FOREIGN KEY ([HandlerId]) REFERENCES [Chat_SessionUnit] ([Id])
);
GO

CREATE TABLE [Chat_SessionPermissionDefinition] (
    [Id] nvarchar(450) NOT NULL,
    [GroupId] bigint NULL,
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NULL,
    [DefaultValue] bigint NOT NULL,
    [MaxValue] bigint NOT NULL,
    [MinValue] bigint NOT NULL,
    [Sorting] bigint NOT NULL,
    [IsEnabled] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_SessionPermissionDefinition] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_SessionPermissionDefinition_Chat_SessionPermissionGroup_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [Chat_SessionPermissionGroup] ([Id])
);
GO

CREATE TABLE [Chat_SessionPermissionRoleGrant] (
    [DefinitionId] nvarchar(450) NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [Value] bigint NOT NULL,
    [IsEnabled] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_SessionPermissionRoleGrant] PRIMARY KEY ([DefinitionId], [RoleId]),
    CONSTRAINT [FK_Chat_SessionPermissionRoleGrant_Chat_SessionPermissionDefinition_DefinitionId] FOREIGN KEY ([DefinitionId]) REFERENCES [Chat_SessionPermissionDefinition] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_SessionPermissionRoleGrant_Chat_SessionRole_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Chat_SessionRole] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_SessionPermissionUnitGrant] (
    [DefinitionId] nvarchar(450) NOT NULL,
    [SessionUnitId] uniqueidentifier NOT NULL,
    [Value] bigint NOT NULL,
    [IsEnabled] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_SessionPermissionUnitGrant] PRIMARY KEY ([DefinitionId], [SessionUnitId]),
    CONSTRAINT [FK_Chat_SessionPermissionUnitGrant_Chat_SessionPermissionDefinition_DefinitionId] FOREIGN KEY ([DefinitionId]) REFERENCES [Chat_SessionPermissionDefinition] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_SessionPermissionUnitGrant_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Chat_SessionRole_IsDefault] ON [Chat_SessionRole] ([IsDefault] DESC);
GO

CREATE INDEX [IX_Chat_ChatObject_Code] ON [Chat_ChatObject] ([Code]);
GO

CREATE INDEX [IX_Chat_ChatObject_FullPath] ON [Chat_ChatObject] ([FullPath]);
GO

CREATE INDEX [IX_Chat_ChatObject_Name] ON [Chat_ChatObject] ([Name]);
GO

CREATE INDEX [IX_Chat_SessionPermissionDefinition_GroupId] ON [Chat_SessionPermissionDefinition] ([GroupId]);
GO

CREATE INDEX [IX_Chat_SessionPermissionDefinition_Sorting] ON [Chat_SessionPermissionDefinition] ([Sorting] DESC);
GO

CREATE INDEX [IX_Chat_SessionPermissionGroup_FullPath] ON [Chat_SessionPermissionGroup] ([FullPath]);
GO

CREATE INDEX [IX_Chat_SessionPermissionGroup_ParentId] ON [Chat_SessionPermissionGroup] ([ParentId]);
GO

CREATE INDEX [IX_Chat_SessionPermissionGroup_Sorting] ON [Chat_SessionPermissionGroup] ([Sorting] DESC);
GO

CREATE INDEX [IX_Chat_SessionPermissionRoleGrant_RoleId] ON [Chat_SessionPermissionRoleGrant] ([RoleId]);
GO

CREATE INDEX [IX_Chat_SessionPermissionUnitGrant_SessionUnitId] ON [Chat_SessionPermissionUnitGrant] ([SessionUnitId]);
GO

CREATE INDEX [IX_Chat_SessionRequest_DestinationId] ON [Chat_SessionRequest] ([DestinationId]);
GO

CREATE INDEX [IX_Chat_SessionRequest_HandlerId] ON [Chat_SessionRequest] ([HandlerId]);
GO

CREATE INDEX [IX_Chat_SessionRequest_OwnerId] ON [Chat_SessionRequest] ([OwnerId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230418070510_SessionPermission_SessionRequest_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_SessionUnit_OwnerId] ON [Chat_SessionUnit];
GO

CREATE INDEX [IX_Chat_SessionUnit_OwnerId_DestinationId] ON [Chat_SessionUnit] ([OwnerId] DESC, [DestinationId] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230419055706_SessionUnit_AddIndex_OwnerId_DestinationId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ChatObject] ADD [MottoId] uniqueidentifier NULL;
GO

CREATE TABLE [Chat_Motto] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(500) NULL,
    [OwnerId] bigint NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_Motto] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Motto_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Chat_ChatObject_MottoId] ON [Chat_ChatObject] ([MottoId]);
GO

CREATE INDEX [IX_Chat_Motto_OwnerId] ON [Chat_Motto] ([OwnerId]);
GO

ALTER TABLE [Chat_ChatObject] ADD CONSTRAINT [FK_Chat_ChatObject_Chat_Motto_MottoId] FOREIGN KEY ([MottoId]) REFERENCES [Chat_Motto] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230419061752_Motto_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ChatObject] ADD [NameSpelling] nvarchar(300) NULL;
GO

ALTER TABLE [Chat_ChatObject] ADD [NameSpellingAbbreviation] nvarchar(100) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230420035140_ChatObject_AddProp_NameSpelling_NameSpellingAbbreviation', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [MemberNameSpelling] nvarchar(300) NULL;
GO

ALTER TABLE [Chat_SessionUnit] ADD [MemberNameSpellingAbbreviation] nvarchar(50) NULL;
GO

ALTER TABLE [Chat_SessionUnit] ADD [RenameSpelling] nvarchar(300) NULL;
GO

ALTER TABLE [Chat_SessionUnit] ADD [RenameSpellingAbbreviation] nvarchar(50) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230420040117_SessionUnit_AddProp_RenameSpelling_RenameSpellingAbbreviation_MemberNameSpelling_MemberNameSpellingAbbreviation', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_MessageReminder] ADD [ReminderType] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230421090821_MessageReminder_AddProp_ReminderType', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ChatObject] ADD [VerificationMethod] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230423081641_ChatObject_AddProp_VerificationMethod', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ChatObject] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230424081941_ChatObject_AddProp_IsEnabled', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Message] ADD [ReminderType] int NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230427023524_Message_AddProp_ReminderType', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ChatObject] ADD [IsDefault] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230502013509_ChatObject_AddProp_IsDefault', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [Key] nvarchar(450) NULL;
GO

CREATE INDEX [IX_Chat_SessionUnit_IsPublic] ON [Chat_SessionUnit] ([IsPublic] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_IsStatic] ON [Chat_SessionUnit] ([IsStatic] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_Key] ON [Chat_SessionUnit] ([Key] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230502082028_SessionUnit_AddProp_Key_AndIndex', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_Chat_SessionUnit_DestinationObjectType] ON [Chat_SessionUnit] ([DestinationObjectType] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230502084109_SessionUnit_AddIndex_DestinationObjectType', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ChatObject] ADD [Gender] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230503025914_ChatObject_AddProp_Gender', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_Follow] (
    [OwnerId] uniqueidentifier NOT NULL,
    [DestinationId] uniqueidentifier NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_Follow] PRIMARY KEY ([OwnerId], [DestinationId]),
    CONSTRAINT [FK_Chat_Follow_Chat_SessionUnit_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230503075841_Follow_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [FK_Chat_OpenedRecorder_Chat_ChatObject_OwnerId];
GO

ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [FK_Chat_OpenedRecorder_Chat_Message_MessageId];
GO

ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [PK_Chat_OpenedRecorder];
GO

DROP INDEX [IX_Chat_OpenedRecorder_MessageId] ON [Chat_OpenedRecorder];
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_OpenedRecorder]') AND [c].[name] = N'DeleterId');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [Chat_OpenedRecorder] DROP COLUMN [DeleterId];
GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_OpenedRecorder]') AND [c].[name] = N'DeletionTime');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [Chat_OpenedRecorder] DROP COLUMN [DeletionTime];
GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_OpenedRecorder]') AND [c].[name] = N'IsDeleted');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Chat_OpenedRecorder] DROP COLUMN [IsDeleted];
GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_OpenedRecorder]') AND [c].[name] = N'MessageAutoId');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [Chat_OpenedRecorder] DROP COLUMN [MessageAutoId];
GO

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_OpenedRecorder]') AND [c].[name] = N'TenantId');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [Chat_OpenedRecorder] DROP COLUMN [TenantId];
GO

EXEC sp_rename N'[Chat_OpenedRecorder].[Id]', N'SessionUnitId', N'COLUMN';
GO

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_OpenedRecorder]') AND [c].[name] = N'OwnerId');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [Chat_OpenedRecorder] ALTER COLUMN [OwnerId] bigint NULL;
GO

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_OpenedRecorder]') AND [c].[name] = N'MessageId');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [' + @var15 + '];');
UPDATE [Chat_OpenedRecorder] SET [MessageId] = CAST(0 AS bigint) WHERE [MessageId] IS NULL;
ALTER TABLE [Chat_OpenedRecorder] ALTER COLUMN [MessageId] bigint NOT NULL;
ALTER TABLE [Chat_OpenedRecorder] ADD DEFAULT CAST(0 AS bigint) FOR [MessageId];
GO

ALTER TABLE [Chat_OpenedRecorder] ADD CONSTRAINT [PK_Chat_OpenedRecorder] PRIMARY KEY ([MessageId], [SessionUnitId]);
GO

CREATE INDEX [IX_Chat_OpenedRecorder_SessionUnitId] ON [Chat_OpenedRecorder] ([SessionUnitId]);
GO

ALTER TABLE [Chat_OpenedRecorder] ADD CONSTRAINT [FK_Chat_OpenedRecorder_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]);
GO

ALTER TABLE [Chat_OpenedRecorder] ADD CONSTRAINT [FK_Chat_OpenedRecorder_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [Chat_OpenedRecorder] ADD CONSTRAINT [FK_Chat_OpenedRecorder_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230504021140_OpenedRecorder_Fix', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [FK_Chat_ReadedRecorder_Chat_ChatObject_OwnerId];
GO

ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [FK_Chat_ReadedRecorder_Chat_Message_MessageId];
GO

ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [PK_Chat_ReadedRecorder];
GO

DROP INDEX [IX_Chat_ReadedRecorder_MessageId] ON [Chat_ReadedRecorder];
GO

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'DeleterId');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var16 + '];');
ALTER TABLE [Chat_ReadedRecorder] DROP COLUMN [DeleterId];
GO

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'DeletionTime');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [Chat_ReadedRecorder] DROP COLUMN [DeletionTime];
GO

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'IsDeleted');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [Chat_ReadedRecorder] DROP COLUMN [IsDeleted];
GO

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'MessageAutoId');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var19 + '];');
ALTER TABLE [Chat_ReadedRecorder] DROP COLUMN [MessageAutoId];
GO

DECLARE @var20 sysname;
SELECT @var20 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'SessionId');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var20 + '];');
ALTER TABLE [Chat_ReadedRecorder] DROP COLUMN [SessionId];
GO

DECLARE @var21 sysname;
SELECT @var21 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'TenantId');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var21 + '];');
ALTER TABLE [Chat_ReadedRecorder] DROP COLUMN [TenantId];
GO

EXEC sp_rename N'[Chat_ReadedRecorder].[Id]', N'SessionUnitId', N'COLUMN';
GO

DECLARE @var22 sysname;
SELECT @var22 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'OwnerId');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var22 + '];');
ALTER TABLE [Chat_ReadedRecorder] ALTER COLUMN [OwnerId] bigint NULL;
GO

DECLARE @var23 sysname;
SELECT @var23 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'MessageId');
IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var23 + '];');
UPDATE [Chat_ReadedRecorder] SET [MessageId] = CAST(0 AS bigint) WHERE [MessageId] IS NULL;
ALTER TABLE [Chat_ReadedRecorder] ALTER COLUMN [MessageId] bigint NOT NULL;
ALTER TABLE [Chat_ReadedRecorder] ADD DEFAULT CAST(0 AS bigint) FOR [MessageId];
GO

ALTER TABLE [Chat_ReadedRecorder] ADD CONSTRAINT [PK_Chat_ReadedRecorder] PRIMARY KEY ([MessageId], [SessionUnitId]);
GO

CREATE INDEX [IX_Chat_ReadedRecorder_SessionUnitId] ON [Chat_ReadedRecorder] ([SessionUnitId]);
GO

ALTER TABLE [Chat_ReadedRecorder] ADD CONSTRAINT [FK_Chat_ReadedRecorder_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]);
GO

ALTER TABLE [Chat_ReadedRecorder] ADD CONSTRAINT [FK_Chat_ReadedRecorder_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [Chat_ReadedRecorder] ADD CONSTRAINT [FK_Chat_ReadedRecorder_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230504033941_ReadedRecorder_Fix', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [BadgePrivate] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_SessionUnit] ADD [BadgePublic] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_SessionUnit] ADD [FollowingCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_SessionUnit] ADD [ReminderAllCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_SessionUnit] ADD [ReminderMeCount] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230510035523_SessionUnit_AddStatProp_BadgePublic_BadgePrivate_ReminderAllCount_ReminderMeCount_FollowingCount', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

EXEC sp_rename N'[Chat_SessionUnit].[ReminderMeCount]', N'RemindMeCount', N'COLUMN';
GO

EXEC sp_rename N'[Chat_SessionUnit].[ReminderAllCount]', N'RemindAllCount', N'COLUMN';
GO

EXEC sp_rename N'[Chat_SessionUnit].[BadgePublic]', N'PublicBadge', N'COLUMN';
GO

EXEC sp_rename N'[Chat_SessionUnit].[BadgePrivate]', N'PrivateBadge', N'COLUMN';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230510073042_SessionUnit_FixStatProp', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [AppUserId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_SessionUnit] ADD [DestinationName] nvarchar(50) NULL;
GO

ALTER TABLE [Chat_SessionUnit] ADD [DestinationNameSpellingAbbreviation] nvarchar(50) NULL;
GO

ALTER TABLE [Chat_SessionUnit] ADD [OwnerName] nvarchar(50) NULL;
GO

ALTER TABLE [Chat_SessionUnit] ADD [OwnerNameSpellingAbbreviation] nvarchar(50) NULL;
GO

CREATE INDEX [IX_Chat_SessionUnit_DestinationName] ON [Chat_SessionUnit] ([DestinationName] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_DestinationNameSpellingAbbreviation] ON [Chat_SessionUnit] ([DestinationNameSpellingAbbreviation] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_MemberName] ON [Chat_SessionUnit] ([MemberName] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_MemberNameSpellingAbbreviation] ON [Chat_SessionUnit] ([MemberNameSpellingAbbreviation] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_OwnerName] ON [Chat_SessionUnit] ([OwnerName] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_OwnerNameSpellingAbbreviation] ON [Chat_SessionUnit] ([OwnerNameSpellingAbbreviation] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_Rename] ON [Chat_SessionUnit] ([Rename] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_RenameSpellingAbbreviation] ON [Chat_SessionUnit] ([RenameSpellingAbbreviation] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230515064007_SessionUnit_AddProp_OwnerName_DestinationName_AppUserId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var24 sysname;
SELECT @var24 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObject]') AND [c].[name] = N'Portrait');
IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObject] DROP CONSTRAINT [' + @var24 + '];');
ALTER TABLE [Chat_ChatObject] ALTER COLUMN [Portrait] nvarchar(1000) NULL;
GO

DECLARE @var25 sysname;
SELECT @var25 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObject]') AND [c].[name] = N'NameSpellingAbbreviation');
IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObject] DROP CONSTRAINT [' + @var25 + '];');
ALTER TABLE [Chat_ChatObject] ALTER COLUMN [NameSpellingAbbreviation] nvarchar(50) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230515064425_ChatObject_FixSize_NameSpellingAbbreviation', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [OwnerObjectType] int NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230515072304_SessionUnit_AddProp_OwnerObjectType', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Favorite] DROP CONSTRAINT [FK_Chat_Favorite_Chat_ChatObject_OwnerId];
GO

DROP TABLE [Chat_FavoriteMessage];
GO

ALTER TABLE [Chat_Favorite] DROP CONSTRAINT [PK_Chat_Favorite];
GO

DECLARE @var26 sysname;
SELECT @var26 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Favorite]') AND [c].[name] = N'DeleterId');
IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Favorite] DROP CONSTRAINT [' + @var26 + '];');
ALTER TABLE [Chat_Favorite] DROP COLUMN [DeleterId];
GO

DECLARE @var27 sysname;
SELECT @var27 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Favorite]') AND [c].[name] = N'DeletionTime');
IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Favorite] DROP CONSTRAINT [' + @var27 + '];');
ALTER TABLE [Chat_Favorite] DROP COLUMN [DeletionTime];
GO

DECLARE @var28 sysname;
SELECT @var28 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Favorite]') AND [c].[name] = N'IsDeleted');
IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Favorite] DROP CONSTRAINT [' + @var28 + '];');
ALTER TABLE [Chat_Favorite] DROP COLUMN [IsDeleted];
GO

DECLARE @var29 sysname;
SELECT @var29 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Favorite]') AND [c].[name] = N'TenantId');
IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Favorite] DROP CONSTRAINT [' + @var29 + '];');
ALTER TABLE [Chat_Favorite] DROP COLUMN [TenantId];
GO

EXEC sp_rename N'[Chat_Favorite].[Id]', N'SessionUnitId', N'COLUMN';
GO

DECLARE @var30 sysname;
SELECT @var30 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Favorite]') AND [c].[name] = N'OwnerId');
IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Favorite] DROP CONSTRAINT [' + @var30 + '];');
ALTER TABLE [Chat_Favorite] ALTER COLUMN [OwnerId] bigint NULL;
GO

ALTER TABLE [Chat_Favorite] ADD [MessageId] bigint NOT NULL DEFAULT CAST(0 AS bigint);
GO

ALTER TABLE [Chat_Favorite] ADD [DestinationId] bigint NULL;
GO

ALTER TABLE [Chat_Favorite] ADD [DeviceId] nvarchar(36) NULL;
GO

ALTER TABLE [Chat_Favorite] ADD CONSTRAINT [PK_Chat_Favorite] PRIMARY KEY ([SessionUnitId], [MessageId]);
GO

CREATE INDEX [IX_Chat_Favorite_DestinationId] ON [Chat_Favorite] ([DestinationId]);
GO

CREATE INDEX [IX_Chat_Favorite_MessageId] ON [Chat_Favorite] ([MessageId]);
GO

ALTER TABLE [Chat_Favorite] ADD CONSTRAINT [FK_Chat_Favorite_Chat_ChatObject_DestinationId] FOREIGN KEY ([DestinationId]) REFERENCES [Chat_ChatObject] ([Id]);
GO

ALTER TABLE [Chat_Favorite] ADD CONSTRAINT [FK_Chat_Favorite_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]);
GO

ALTER TABLE [Chat_Favorite] ADD CONSTRAINT [FK_Chat_Favorite_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [Chat_Favorite] ADD CONSTRAINT [FK_Chat_Favorite_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230516100935_Favorite_Fix', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Message] ADD [Size] bigint NOT NULL DEFAULT CAST(0 AS bigint);
GO

ALTER TABLE [Chat_Favorite] ADD [MessageType] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_Favorite] ADD [Size] bigint NOT NULL DEFAULT CAST(0 AS bigint);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230517025211_Favorite_Message_AddProp_Size', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Message_Template_SoundContent] ADD [Size] bigint NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230517032113_SoundContent_AddProp_Size', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [PK_Chat_ReadedRecorder];
GO

DROP INDEX [IX_Chat_ReadedRecorder_SessionUnitId] ON [Chat_ReadedRecorder];
GO

ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [PK_Chat_OpenedRecorder];
GO

DROP INDEX [IX_Chat_OpenedRecorder_SessionUnitId] ON [Chat_OpenedRecorder];
GO

ALTER TABLE [Chat_MessageReminder] DROP CONSTRAINT [PK_Chat_MessageReminder];
GO

DROP INDEX [IX_Chat_MessageReminder_SessionUnitId] ON [Chat_MessageReminder];
GO

ALTER TABLE [Chat_ReadedRecorder] ADD CONSTRAINT [PK_Chat_ReadedRecorder] PRIMARY KEY ([SessionUnitId], [MessageId]);
GO

ALTER TABLE [Chat_OpenedRecorder] ADD CONSTRAINT [PK_Chat_OpenedRecorder] PRIMARY KEY ([SessionUnitId], [MessageId]);
GO

ALTER TABLE [Chat_MessageReminder] ADD CONSTRAINT [PK_Chat_MessageReminder] PRIMARY KEY ([SessionUnitId], [MessageId]);
GO

CREATE INDEX [IX_Chat_ReadedRecorder_MessageId] ON [Chat_ReadedRecorder] ([MessageId]);
GO

CREATE INDEX [IX_Chat_OpenedRecorder_MessageId] ON [Chat_OpenedRecorder] ([MessageId]);
GO

CREATE INDEX [IX_Chat_MessageReminder_MessageId] ON [Chat_MessageReminder] ([MessageId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230518024704_BaseRecord_Fix_Index', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [IsScoped] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message] ADD [IsScoped] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

CREATE TABLE [Chat_Scoped] (
    [SessionUnitId] uniqueidentifier NOT NULL,
    [MessageId] bigint NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_Scoped] PRIMARY KEY ([SessionUnitId], [MessageId]),
    CONSTRAINT [FK_Chat_Scoped_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Scoped_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Chat_Scoped_MessageId] ON [Chat_Scoped] ([MessageId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230518025625_Scoped_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Message] ADD [FavoritedCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_Message] ADD [OpenedCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_Message] ADD [ReadedCount] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230518060712_Message_AddProp_ReadedCount_OpenedCount_FavoritedCount', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var31 sysname;
SELECT @var31 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'ServiceStatus');
IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var31 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [ServiceStatus];
GO

ALTER TABLE [Chat_ChatObject] ADD [ServiceStatus] int NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230519055746_ServiceStatus_Fix', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionPermissionGroup] ADD [ChildrenCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_SessionOrganization] ADD [ChildrenCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_ChatObjectCategory] ADD [ChildrenCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_ChatObject] ADD [ChildrenCount] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230523074935_TreeEntity_AddProp_ChildrenCount', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Message_Template_VideoContent] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_VideoContent] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_TextContent] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_TextContent] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_SoundContent] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_SoundContent] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_LocationContent] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_LocationContent] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_LinkContent] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_LinkContent] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_ImageContent] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_ImageContent] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_HtmlContent] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_HtmlContent] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_HistoryContent] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_HistoryContent] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_FileContent] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_FileContent] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_ContactsContent] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_ContactsContent] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_CmdContent] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_CmdContent] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_ArticleContent] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Message_Template_ArticleContent] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

CREATE TABLE [Chat_Word] (
    [Id] uniqueidentifier NOT NULL,
    [Value] nvarchar(36) NULL,
    [IsEnabled] bit NOT NULL,
    [IsDirty] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_Word] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Chat_TextContentWord] (
    [TextContentId] uniqueidentifier NOT NULL,
    [WordId] uniqueidentifier NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_TextContentWord] PRIMARY KEY ([TextContentId], [WordId]),
    CONSTRAINT [FK_Chat_TextContentWord_Chat_Message_Template_TextContent_TextContentId] FOREIGN KEY ([TextContentId]) REFERENCES [Chat_Message_Template_TextContent] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_TextContentWord_Chat_Word_WordId] FOREIGN KEY ([WordId]) REFERENCES [Chat_Word] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Chat_TextContentWord_WordId] ON [Chat_TextContentWord] ([WordId]);
GO

CREATE INDEX [IX_Chat_Word_Value] ON [Chat_Word] ([Value] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230524030951_Word_Init_And_IContentEntity_AddProp_IsEnabled', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_FavoritedValue] (
    [MessageId] bigint NOT NULL,
    [Value] bigint NOT NULL,
    [CreationTime] datetime2 NOT NULL,
    [LastModificationTime] datetime2 NULL,
    CONSTRAINT [PK_Chat_FavoritedValue] PRIMARY KEY ([MessageId]),
    CONSTRAINT [FK_Chat_FavoritedValue_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_OpenedValue] (
    [MessageId] bigint NOT NULL,
    [Value] bigint NOT NULL,
    [CreationTime] datetime2 NOT NULL,
    [LastModificationTime] datetime2 NULL,
    CONSTRAINT [PK_Chat_OpenedValue] PRIMARY KEY ([MessageId]),
    CONSTRAINT [FK_Chat_OpenedValue_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_ReadedValue] (
    [MessageId] bigint NOT NULL,
    [Value] bigint NOT NULL,
    [CreationTime] datetime2 NOT NULL,
    [LastModificationTime] datetime2 NULL,
    CONSTRAINT [PK_Chat_ReadedValue] PRIMARY KEY ([MessageId]),
    CONSTRAINT [FK_Chat_ReadedValue_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230524101435_MessageRecoreder_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_SessionUnitCounter] (
    [SessionUnitId] uniqueidentifier NOT NULL,
    [PublicBadge] int NOT NULL,
    [PrivateBadge] int NOT NULL,
    [RemindAllCount] int NOT NULL,
    [RemindMeCount] int NOT NULL,
    [FollowingCount] int NOT NULL,
    [CreationTime] datetime2 NOT NULL,
    [LastModificationTime] datetime2 NULL,
    CONSTRAINT [PK_Chat_SessionUnitCounter] PRIMARY KEY ([SessionUnitId]),
    CONSTRAINT [FK_Chat_SessionUnitCounter_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230525034933_SessionUnitCounter_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnitCounter] ADD [LastMessageId] bigint NULL;
GO

CREATE INDEX [IX_Chat_SessionUnitCounter_LastMessageId] ON [Chat_SessionUnitCounter] ([LastMessageId]);
GO

ALTER TABLE [Chat_SessionUnitCounter] ADD CONSTRAINT [FK_Chat_SessionUnitCounter_Chat_Message_LastMessageId] FOREIGN KEY ([LastMessageId]) REFERENCES [Chat_Message] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230525035644_SessionUnitCounter_AddProp_LastMessageId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_SessionUnitCounter_LastMessageId] ON [Chat_SessionUnitCounter];
GO

CREATE INDEX [IX_Chat_SessionUnitCounter_LastMessageId] ON [Chat_SessionUnitCounter] ([LastMessageId] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230526015114_SessionUnitCounter_AddIndex_LastMessageId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_Chat_SessionUnitCounter_SessionUnitId_LastMessageId] ON [Chat_SessionUnitCounter] ([SessionUnitId] DESC, [LastMessageId] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230526015736_SessionUnitCounter_AddIndex_SessionUnitId_LastMessageId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_SessionUnitSetting] (
    [SessionUnitId] uniqueidentifier NOT NULL,
    [ReadedMessageId] bigint NULL,
    [HistoryFristTime] datetime2 NULL,
    [HistoryLastTime] datetime2 NULL,
    [ClearTime] datetime2 NULL,
    [RemoveTime] datetime2 NULL,
    [MemberName] nvarchar(50) NULL,
    [Rename] nvarchar(50) NULL,
    [Remarks] nvarchar(500) NULL,
    [IsCantacts] bit NOT NULL,
    [IsImmersed] bit NOT NULL,
    [IsShowMemberName] bit NOT NULL,
    [IsShowReaded] bit NOT NULL,
    [IsStatic] bit NOT NULL,
    [IsPublic] bit NOT NULL,
    [IsInputEnabled] bit NOT NULL,
    [IsEnabled] bit NOT NULL,
    [IsCreator] bit NOT NULL,
    [InviterId] uniqueidentifier NULL,
    [IsKilled] bit NOT NULL,
    [KillType] int NULL,
    [KillTime] datetime2 NULL,
    [KillerId] uniqueidentifier NULL,
    [CreationTime] datetime2 NOT NULL,
    [LastModificationTime] datetime2 NULL,
    CONSTRAINT [PK_Chat_SessionUnitSetting] PRIMARY KEY ([SessionUnitId]),
    CONSTRAINT [FK_Chat_SessionUnitSetting_Chat_Message_ReadedMessageId] FOREIGN KEY ([ReadedMessageId]) REFERENCES [Chat_Message] ([Id]),
    CONSTRAINT [FK_Chat_SessionUnitSetting_Chat_SessionUnit_InviterId] FOREIGN KEY ([InviterId]) REFERENCES [Chat_SessionUnit] ([Id]),
    CONSTRAINT [FK_Chat_SessionUnitSetting_Chat_SessionUnit_KillerId] FOREIGN KEY ([KillerId]) REFERENCES [Chat_SessionUnit] ([Id]),
    CONSTRAINT [FK_Chat_SessionUnitSetting_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE
);
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'已读的消息';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'ReadedMessageId';
SET @description = N'查看历史消息起始时间,为null时则不限';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'HistoryFristTime';
SET @description = N'查看历史消息截止时间,为null时则不限';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'HistoryLastTime';
SET @description = N'清除历史消息最后时间,为null时则不限';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'ClearTime';
SET @description = N'不显示消息会话(不退群,不删除消息)';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'RemoveTime';
SET @description = N'会话内的名称';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'MemberName';
SET @description = N'备注名称';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'Rename';
SET @description = N'备注其他';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'Remarks';
SET @description = N'是否保存通讯录';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'IsCantacts';
SET @description = N'消息免打扰，默认为 false';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'IsImmersed';
SET @description = N'是否显示成员名称';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'IsShowMemberName';
SET @description = N'是否显示已读';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'IsShowReaded';
SET @description = N'是否固定成员';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'IsStatic';
SET @description = N'是否公有成员';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'IsPublic';
SET @description = N'是否启用输入框';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'IsInputEnabled';
SET @description = N'是否可用';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'IsEnabled';
SET @description = N'是否创建者（群主等）';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'IsCreator';
SET @description = N'删除会话(退出群等)，但是不删除会话(用于查看历史消息)';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'IsKilled';
SET @description = N'删除会话时间';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'KillTime';
SET @description = N'创建时间';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'CreationTime';
SET @description = N'修改时间';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'LastModificationTime';
GO

CREATE INDEX [IX_Chat_SessionUnitSetting_InviterId] ON [Chat_SessionUnitSetting] ([InviterId]);
GO

CREATE INDEX [IX_Chat_SessionUnitSetting_KillerId] ON [Chat_SessionUnitSetting] ([KillerId]);
GO

CREATE INDEX [IX_Chat_SessionUnitSetting_ReadedMessageId] ON [Chat_SessionUnitSetting] ([ReadedMessageId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230526055737_SessionUnitSetting_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [Chat_FavoritedValue];
GO

DROP TABLE [Chat_OpenedValue];
GO

DROP TABLE [Chat_ReadedValue];
GO

CREATE TABLE [Chat_FavoritedCounter] (
    [MessageId] bigint NOT NULL,
    [Count] bigint NOT NULL,
    [CreationTime] datetime2 NOT NULL,
    [LastModificationTime] datetime2 NULL,
    CONSTRAINT [PK_Chat_FavoritedCounter] PRIMARY KEY ([MessageId]),
    CONSTRAINT [FK_Chat_FavoritedCounter_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_OpenedCounter] (
    [MessageId] bigint NOT NULL,
    [Count] bigint NOT NULL,
    [CreationTime] datetime2 NOT NULL,
    [LastModificationTime] datetime2 NULL,
    CONSTRAINT [PK_Chat_OpenedCounter] PRIMARY KEY ([MessageId]),
    CONSTRAINT [FK_Chat_OpenedCounter_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_ReadedCounter] (
    [MessageId] bigint NOT NULL,
    [Count] bigint NOT NULL,
    [CreationTime] datetime2 NOT NULL,
    [LastModificationTime] datetime2 NULL,
    CONSTRAINT [PK_Chat_ReadedCounter] PRIMARY KEY ([MessageId]),
    CONSTRAINT [FK_Chat_ReadedCounter_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230526070728_MessageCounter_Rename', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc] ON [Chat_SessionUnit] ([Sorting] DESC, [LastMessageId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230526083128_SessionUnit_AddIndex_Sorting_Desc_LastMessageId_Asc', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [FK_Chat_SessionUnit_Chat_Message_ReadedMessageId];
GO

DROP INDEX [IX_Chat_SessionUnit_DestinationName] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_DestinationNameSpellingAbbreviation] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_IsPublic] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_IsStatic] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_MemberName] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_MemberNameSpellingAbbreviation] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_OwnerId_DestinationId] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_OwnerName] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_OwnerNameSpellingAbbreviation] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_ReadedMessageId] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_Rename] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_RenameSpellingAbbreviation] ON [Chat_SessionUnit];
GO

CREATE INDEX [IX_Chat_SessionUnit_OwnerId] ON [Chat_SessionUnit] ([OwnerId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230526084608_SessionUnit_Remove_1', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [FK_Chat_SessionUnit_Chat_ChatObject_InviterId];
GO

ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [FK_Chat_SessionUnit_Chat_ChatObject_KillerId];
GO

ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [FK_Chat_SessionUnit_Chat_SessionUnit_InviterUnitId];
GO

ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [FK_Chat_SessionUnit_Chat_SessionUnit_KillerUnitId];
GO

DROP INDEX [IX_Chat_SessionUnit_InviterId] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_InviterUnitId] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_KillerId] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_KillerUnitId] ON [Chat_SessionUnit];
GO

DECLARE @var32 sysname;
SELECT @var32 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'ClearTime');
IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var32 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [ClearTime];
GO

DECLARE @var33 sysname;
SELECT @var33 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'DestinationName');
IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var33 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [DestinationName];
GO

DECLARE @var34 sysname;
SELECT @var34 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'DestinationNameSpellingAbbreviation');
IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var34 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [DestinationNameSpellingAbbreviation];
GO

DECLARE @var35 sysname;
SELECT @var35 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'HistoryFristTime');
IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var35 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [HistoryFristTime];
GO

DECLARE @var36 sysname;
SELECT @var36 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'HistoryLastTime');
IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var36 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [HistoryLastTime];
GO

DECLARE @var37 sysname;
SELECT @var37 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'InviterId');
IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var37 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [InviterId];
GO

DECLARE @var38 sysname;
SELECT @var38 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'InviterUnitId');
IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var38 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [InviterUnitId];
GO

DECLARE @var39 sysname;
SELECT @var39 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'IsCantacts');
IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var39 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [IsCantacts];
GO

DECLARE @var40 sysname;
SELECT @var40 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'IsImmersed');
IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var40 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [IsImmersed];
GO

DECLARE @var41 sysname;
SELECT @var41 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'IsImportant');
IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var41 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [IsImportant];
GO

DECLARE @var42 sysname;
SELECT @var42 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'IsKilled');
IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var42 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [IsKilled];
GO

DECLARE @var43 sysname;
SELECT @var43 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'IsShowMemberName');
IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var43 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [IsShowMemberName];
GO

DECLARE @var44 sysname;
SELECT @var44 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'IsShowReaded');
IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var44 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [IsShowReaded];
GO

DECLARE @var45 sysname;
SELECT @var45 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'KillTime');
IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var45 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [KillTime];
GO

DECLARE @var46 sysname;
SELECT @var46 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'KillType');
IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var46 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [KillType];
GO

DECLARE @var47 sysname;
SELECT @var47 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'KillerId');
IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var47 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [KillerId];
GO

DECLARE @var48 sysname;
SELECT @var48 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'KillerUnitId');
IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var48 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [KillerUnitId];
GO

DECLARE @var49 sysname;
SELECT @var49 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'MemberName');
IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var49 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [MemberName];
GO

DECLARE @var50 sysname;
SELECT @var50 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'MemberNameSpelling');
IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var50 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [MemberNameSpelling];
GO

DECLARE @var51 sysname;
SELECT @var51 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'MemberNameSpellingAbbreviation');
IF @var51 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var51 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [MemberNameSpellingAbbreviation];
GO

DECLARE @var52 sysname;
SELECT @var52 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'OwnerName');
IF @var52 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var52 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [OwnerName];
GO

DECLARE @var53 sysname;
SELECT @var53 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'OwnerNameSpellingAbbreviation');
IF @var53 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var53 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [OwnerNameSpellingAbbreviation];
GO

DECLARE @var54 sysname;
SELECT @var54 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'Remarks');
IF @var54 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var54 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [Remarks];
GO

DECLARE @var55 sysname;
SELECT @var55 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'RemoveTime');
IF @var55 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var55 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [RemoveTime];
GO

DECLARE @var56 sysname;
SELECT @var56 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'Rename');
IF @var56 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var56 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [Rename];
GO

DECLARE @var57 sysname;
SELECT @var57 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'RenameSpelling');
IF @var57 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var57 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [RenameSpelling];
GO

DECLARE @var58 sysname;
SELECT @var58 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'RenameSpellingAbbreviation');
IF @var58 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var58 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [RenameSpellingAbbreviation];
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [BackgroundImage] nvarchar(500) NULL;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [JoinWay] int NULL;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [MemberNameSpelling] nvarchar(300) NULL;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [MemberNameSpellingAbbreviation] nvarchar(50) NULL;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [RenameSpelling] nvarchar(300) NULL;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [RenameSpellingAbbreviation] nvarchar(50) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230526093949_SessionUnit_Remove_2', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var59 sysname;
SELECT @var59 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'BackgroundImage');
IF @var59 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var59 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [BackgroundImage];
GO

DECLARE @var60 sysname;
SELECT @var60 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'IsCreator');
IF @var60 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var60 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [IsCreator];
GO

DECLARE @var61 sysname;
SELECT @var61 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'IsEnabled');
IF @var61 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var61 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [IsEnabled];
GO

DECLARE @var62 sysname;
SELECT @var62 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'IsInputEnabled');
IF @var62 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var62 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [IsInputEnabled];
GO

DECLARE @var63 sysname;
SELECT @var63 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'IsScoped');
IF @var63 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var63 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [IsScoped];
GO

DECLARE @var64 sysname;
SELECT @var64 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'JoinWay');
IF @var64 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var64 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [JoinWay];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230526095558_SessionUnit_Remove_3', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var65 sysname;
SELECT @var65 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'IsPublic');
IF @var65 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var65 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [IsPublic];
GO

DECLARE @var66 sysname;
SELECT @var66 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'IsStatic');
IF @var66 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var66 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [IsStatic];
GO

DECLARE @var67 sysname;
SELECT @var67 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'ReadedMessageId');
IF @var67 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var67 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [ReadedMessageId];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230529021153_SessionUnit_DropColumn_IsPublic_IsStatic', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE UNIQUE INDEX [IX_Chat_SessionUnitCounter_SessionUnitId] ON [Chat_SessionUnitCounter] ([SessionUnitId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230529022949_SessionUnitCounter_AddIndex_SessionUnitId_IsUnique', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [ConcurrencyStamp] nvarchar(40) NULL;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [CreatorId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [ExtraProperties] nvarchar(max) NULL;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [LastModifierId] uniqueidentifier NULL;
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'修改时间';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitCounter', 'COLUMN', N'LastModificationTime';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'创建时间';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitCounter', 'COLUMN', N'CreationTime';
GO

ALTER TABLE [Chat_SessionUnitCounter] ADD [ConcurrencyStamp] nvarchar(40) NULL;
GO

ALTER TABLE [Chat_SessionUnitCounter] ADD [CreatorId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_SessionUnitCounter] ADD [ExtraProperties] nvarchar(max) NULL;
GO

ALTER TABLE [Chat_SessionUnitCounter] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_SessionUnitCounter] ADD [LastModifierId] uniqueidentifier NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230529033749_SessionUnit_Setting_Fix', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ReadedCounter] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_OpenedCounter] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_FavoritedCounter] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230529064539_MessageCounter_AddProp_IsDeleted', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var68 sysname;
SELECT @var68 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message]') AND [c].[name] = N'FavoritedCount');
IF @var68 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message] DROP CONSTRAINT [' + @var68 + '];');
ALTER TABLE [Chat_Message] DROP COLUMN [FavoritedCount];
GO

DECLARE @var69 sysname;
SELECT @var69 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message]') AND [c].[name] = N'OpenedCount');
IF @var69 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message] DROP CONSTRAINT [' + @var69 + '];');
ALTER TABLE [Chat_Message] DROP COLUMN [OpenedCount];
GO

DECLARE @var70 sysname;
SELECT @var70 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message]') AND [c].[name] = N'ReadedCount');
IF @var70 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message] DROP CONSTRAINT [' + @var70 + '];');
ALTER TABLE [Chat_Message] DROP COLUMN [ReadedCount];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230529082716_MessageCounter_DropColumn_ReadedCount_OpenedCount_FavoritedCount', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [Chat_Favorite];
GO

CREATE TABLE [Chat_FavoritedRecorder] (
    [MessageId] bigint NOT NULL,
    [SessionUnitId] uniqueidentifier NOT NULL,
    [Size] bigint NOT NULL,
    [MessageType] int NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [OwnerId] bigint NULL,
    [DestinationId] bigint NULL,
    [DeviceId] nvarchar(36) NULL,
    CONSTRAINT [PK_Chat_FavoritedRecorder] PRIMARY KEY ([SessionUnitId], [MessageId]),
    CONSTRAINT [FK_Chat_FavoritedRecorder_Chat_ChatObject_DestinationId] FOREIGN KEY ([DestinationId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_FavoritedRecorder_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_FavoritedRecorder_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_FavoritedRecorder_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Chat_FavoritedRecorder_DestinationId] ON [Chat_FavoritedRecorder] ([DestinationId]);
GO

CREATE INDEX [IX_Chat_FavoritedRecorder_MessageId] ON [Chat_FavoritedRecorder] ([MessageId]);
GO

CREATE INDEX [IX_Chat_FavoritedRecorder_OwnerId] ON [Chat_FavoritedRecorder] ([OwnerId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230529092348_Favorite_Rename_FavoritedRecorder', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_Chat_Message_IsPrivate] ON [Chat_Message] ([IsPrivate]);
GO

CREATE INDEX [IX_Chat_Message_MessageType] ON [Chat_Message] ([MessageType]);
GO

CREATE INDEX [IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId] ON [Chat_Message] ([SessionId], [IsPrivate], [SenderId], [ReceiverId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230530011524_Message_AddIndex_IsPrivate', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_TextContentWord] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_SessionUnitTag] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_SessionUnitRole] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_SessionUnitOrganization] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_SessionUnitCounter] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_SessionPermissionUnitGrant] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_SessionPermissionRoleGrant] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_Scoped] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_ReadedRecorder] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_OpenedRecorder] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_MessageReminder] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_HistoryMessage] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_FriendshipTagUnit] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_Follow] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_FavoritedRecorder] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_ChatObjectCategoryUnit] ADD [TenantId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_ArticleMessage] ADD [TenantId] uniqueidentifier NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230530024919_BaseEntity_IMultiTenant', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_Menu] (
    [Id] uniqueidentifier NOT NULL,
    [OwnerId] bigint NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [Name] nvarchar(64) NOT NULL,
    [ParentId] uniqueidentifier NULL,
    [FullPath] nvarchar(1000) NULL,
    [FullPathName] nvarchar(1000) NULL,
    [Depth] int NOT NULL,
    [ChildrenCount] int NOT NULL,
    [Sorting] float NOT NULL,
    [Description] nvarchar(500) NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_Menu] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Menu_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_Menu_Chat_Menu_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Chat_Menu] ([Id])
);
GO

CREATE INDEX [IX_Chat_Menu_FullPath] ON [Chat_Menu] ([FullPath]);
GO

CREATE INDEX [IX_Chat_Menu_OwnerId] ON [Chat_Menu] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_Menu_OwnerId_ParentId_Sorting] ON [Chat_Menu] ([OwnerId], [ParentId], [Sorting] DESC);
GO

CREATE INDEX [IX_Chat_Menu_ParentId] ON [Chat_Menu] ([ParentId]);
GO

CREATE INDEX [IX_Chat_Menu_Sorting] ON [Chat_Menu] ([Sorting] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230530063034_Menu_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ChatObject] ADD [IsDeveloper] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

CREATE TABLE [Chat_Developer] (
    [OwnerId] bigint NOT NULL,
    [ApiUrl] nvarchar(100) NULL,
    [IsEnabled] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_Developer] PRIMARY KEY ([OwnerId]),
    CONSTRAINT [FK_Chat_Developer_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230530103938_Developer_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var71 sysname;
SELECT @var71 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Developer]') AND [c].[name] = N'ApiUrl');
IF @var71 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Developer] DROP CONSTRAINT [' + @var71 + '];');
ALTER TABLE [Chat_Developer] DROP COLUMN [ApiUrl];
GO

ALTER TABLE [Chat_Developer] ADD [EncodingAesKey] nvarchar(43) NULL;
GO

ALTER TABLE [Chat_Developer] ADD [PostUrl] nvarchar(256) NULL;
GO

ALTER TABLE [Chat_Developer] ADD [Token] nvarchar(50) NULL;
GO

CREATE TABLE [Chat_HttpRequest] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NULL,
    [HttpMethod] nvarchar(10) NULL,
    [Host] nvarchar(500) NULL,
    [Scheme] nvarchar(10) NULL,
    [Port] int NOT NULL,
    [IsDefaultPort] bit NOT NULL,
    [Query] nvarchar(500) NULL,
    [Fragment] nvarchar(500) NULL,
    [AbsolutePath] nvarchar(500) NULL,
    [Url] nvarchar(max) NULL,
    [Parameters] nvarchar(max) NULL,
    [Timeout] int NOT NULL,
    [UserAgent] nvarchar(500) NULL,
    [Cookies] nvarchar(500) NULL,
    [Referer] nvarchar(500) NULL,
    [Headers] nvarchar(500) NULL,
    [IsSuccess] bit NOT NULL,
    [Message] nvarchar(max) NULL,
    [StatusCode] int NOT NULL,
    [StartTime] bigint NOT NULL,
    [EndTime] bigint NOT NULL,
    [ContentLength] int NOT NULL,
    [Duration] bigint NOT NULL,
    CONSTRAINT [PK_Chat_HttpRequest] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Chat_HttpResponse] (
    [HttpRequestId] uniqueidentifier NOT NULL,
    [Content] nvarchar(max) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_HttpResponse] PRIMARY KEY ([HttpRequestId]),
    CONSTRAINT [FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId] FOREIGN KEY ([HttpRequestId]) REFERENCES [Chat_HttpRequest] ([Id])
);
GO

CREATE INDEX [IX_Chat_HttpRequest_AbsolutePath] ON [Chat_HttpRequest] ([AbsolutePath]);
GO

CREATE INDEX [IX_Chat_HttpRequest_Host] ON [Chat_HttpRequest] ([Host]);
GO

CREATE INDEX [IX_Chat_HttpRequest_HttpMethod] ON [Chat_HttpRequest] ([HttpMethod]);
GO

CREATE INDEX [IX_Chat_HttpRequest_IsSuccess] ON [Chat_HttpRequest] ([IsSuccess]);
GO

CREATE INDEX [IX_Chat_HttpRequest_Port] ON [Chat_HttpRequest] ([Port]);
GO

CREATE INDEX [IX_Chat_HttpRequest_Scheme] ON [Chat_HttpRequest] ([Scheme]);
GO

CREATE INDEX [IX_Chat_HttpRequest_StatusCode] ON [Chat_HttpRequest] ([StatusCode]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230531033543_Developer_HttpRequest_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_HttpRequest] ADD [ConcurrencyStamp] nvarchar(40) NULL;
GO

ALTER TABLE [Chat_HttpRequest] ADD [CreationTime] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO

ALTER TABLE [Chat_HttpRequest] ADD [CreatorId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_HttpRequest] ADD [DeleterId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_HttpRequest] ADD [DeletionTime] datetime2 NULL;
GO

ALTER TABLE [Chat_HttpRequest] ADD [ExtraProperties] nvarchar(max) NULL;
GO

ALTER TABLE [Chat_HttpRequest] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_HttpRequest] ADD [LastModificationTime] datetime2 NULL;
GO

ALTER TABLE [Chat_HttpRequest] ADD [LastModifierId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_HttpRequest] ADD [TenantId] uniqueidentifier NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230531112004_HttpRequest_fix', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_HttpResponse] DROP CONSTRAINT [FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId];
GO

ALTER TABLE [Chat_HttpResponse] ADD CONSTRAINT [FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId] FOREIGN KEY ([HttpRequestId]) REFERENCES [Chat_HttpRequest] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230531112532_FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Developer] DROP CONSTRAINT [FK_Chat_Developer_Chat_ChatObject_OwnerId];
GO

ALTER TABLE [Chat_Developer] ADD CONSTRAINT [FK_Chat_Developer_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230531112728_FK_Developer', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnit] ADD [Ticks] float NOT NULL DEFAULT 0.0E0;
GO

CREATE INDEX [IX_Chat_SessionUnit_Ticks] ON [Chat_SessionUnit] ([Ticks] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230602021954_SessionUnit_AddProp_Ticks', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_Chat_SessionUnit_Sorting_Ticks] ON [Chat_SessionUnit] ([Sorting] DESC, [Ticks] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230602022223_SessionUnit_AddIndex_Sorting_Ticks', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_Chat_Message_CreationTime] ON [Chat_Message] ([CreationTime] DESC);
GO

CREATE INDEX [IX_Chat_Message_CreationTime_Asc] ON [Chat_Message] ([CreationTime]);
GO

CREATE INDEX [IX_Chat_Message_Id_Asc] ON [Chat_Message] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230603025319_Message_AddIndex_Id_Asc_CreationTime_Asc', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_EntryName] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(20) NULL,
    [Code] nvarchar(20) NULL,
    [IsStatic] bit NOT NULL,
    [IsPublic] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_EntryName] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Chat_EntryValue] (
    [Id] uniqueidentifier NOT NULL,
    [EntryNameId] uniqueidentifier NOT NULL,
    [Value] nvarchar(500) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_EntryValue] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_EntryValue_Chat_EntryName_EntryNameId] FOREIGN KEY ([EntryNameId]) REFERENCES [Chat_EntryName] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_ChatObjectEntryValue] (
    [OwnerId] bigint NOT NULL,
    [EntryValueId] uniqueidentifier NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_ChatObjectEntryValue] PRIMARY KEY ([OwnerId], [EntryValueId]),
    CONSTRAINT [FK_Chat_ChatObjectEntryValue_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_ChatObjectEntryValue_Chat_EntryValue_EntryValueId] FOREIGN KEY ([EntryValueId]) REFERENCES [Chat_EntryValue] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Chat_SessionUnitEntryValue] (
    [SessionUnitId] uniqueidentifier NOT NULL,
    [EntryValueId] uniqueidentifier NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_SessionUnitEntryValue] PRIMARY KEY ([SessionUnitId], [EntryValueId]),
    CONSTRAINT [FK_Chat_SessionUnitEntryValue_Chat_EntryValue_EntryValueId] FOREIGN KEY ([EntryValueId]) REFERENCES [Chat_EntryValue] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_SessionUnitEntryValue_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Chat_ChatObjectEntryValue_CreationTime] ON [Chat_ChatObjectEntryValue] ([CreationTime]);
GO

CREATE INDEX [IX_Chat_ChatObjectEntryValue_EntryValueId] ON [Chat_ChatObjectEntryValue] ([EntryValueId]);
GO

CREATE INDEX [IX_Chat_EntryName_Code] ON [Chat_EntryName] ([Code]);
GO

CREATE INDEX [IX_Chat_EntryName_IsStatic] ON [Chat_EntryName] ([IsStatic]);
GO

CREATE INDEX [IX_Chat_EntryName_Name] ON [Chat_EntryName] ([Name]);
GO

CREATE INDEX [IX_Chat_EntryValue_CreationTime] ON [Chat_EntryValue] ([CreationTime]);
GO

CREATE INDEX [IX_Chat_EntryValue_EntryNameId] ON [Chat_EntryValue] ([EntryNameId]);
GO

CREATE INDEX [IX_Chat_EntryValue_Value] ON [Chat_EntryValue] ([Value]);
GO

CREATE INDEX [IX_Chat_SessionUnitEntryValue_CreationTime] ON [Chat_SessionUnitEntryValue] ([CreationTime]);
GO

CREATE INDEX [IX_Chat_SessionUnitEntryValue_EntryValueId] ON [Chat_SessionUnitEntryValue] ([EntryValueId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230605022057_Entry_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var72 sysname;
SELECT @var72 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionDefinition]') AND [c].[name] = N'Name');
IF @var72 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionDefinition] DROP CONSTRAINT [' + @var72 + '];');
ALTER TABLE [Chat_SessionPermissionDefinition] ALTER COLUMN [Name] nvarchar(120) NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230605030444_SessionPermissionDefinition_AlertColumn_Name', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ChatObjectEntryValue] ADD [DestinationId] bigint NULL;
GO

CREATE TABLE [Chat_ChatObjectTargetEntryValue] (
    [OwnerId] bigint NOT NULL,
    [EntryValueId] uniqueidentifier NOT NULL,
    [DestinationId] bigint NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_ChatObjectTargetEntryValue] PRIMARY KEY ([OwnerId], [EntryValueId]),
    CONSTRAINT [FK_Chat_ChatObjectTargetEntryValue_Chat_ChatObject_DestinationId] FOREIGN KEY ([DestinationId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_ChatObjectTargetEntryValue_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_ChatObjectTargetEntryValue_Chat_EntryValue_EntryValueId] FOREIGN KEY ([EntryValueId]) REFERENCES [Chat_EntryValue] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Chat_ChatObjectEntryValue_DestinationId] ON [Chat_ChatObjectEntryValue] ([DestinationId]);
GO

CREATE INDEX [IX_Chat_ChatObjectTargetEntryValue_CreationTime] ON [Chat_ChatObjectTargetEntryValue] ([CreationTime]);
GO

CREATE INDEX [IX_Chat_ChatObjectTargetEntryValue_DestinationId] ON [Chat_ChatObjectTargetEntryValue] ([DestinationId]);
GO

CREATE INDEX [IX_Chat_ChatObjectTargetEntryValue_EntryValueId] ON [Chat_ChatObjectTargetEntryValue] ([EntryValueId]);
GO

ALTER TABLE [Chat_ChatObjectEntryValue] ADD CONSTRAINT [FK_Chat_ChatObjectEntryValue_Chat_ChatObject_DestinationId] FOREIGN KEY ([DestinationId]) REFERENCES [Chat_ChatObject] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230605035643_ChatObjectTargetEntryValue_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_EntryName] ADD [Description] nvarchar(200) NULL;
GO

ALTER TABLE [Chat_EntryName] ADD [ErrorMessage] nvarchar(200) NULL;
GO

ALTER TABLE [Chat_EntryName] ADD [IsRequired] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_EntryName] ADD [IsUniqued] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_EntryName] ADD [MaxCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_EntryName] ADD [MaxLenth] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_EntryName] ADD [MinCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_EntryName] ADD [MinLenth] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_EntryName] ADD [Regex] nvarchar(100) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230606011811_Entry_AddProp_XXX', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [Chat_ChatObjectTargetEntryValue];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230606094810_Drop_ChatObjectTargetEntryValue', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ChatObjectEntryValue] DROP CONSTRAINT [FK_Chat_ChatObjectEntryValue_Chat_ChatObject_DestinationId];
GO

DROP INDEX [IX_Chat_ChatObjectEntryValue_DestinationId] ON [Chat_ChatObjectEntryValue];
GO

DECLARE @var73 sysname;
SELECT @var73 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectEntryValue]') AND [c].[name] = N'DestinationId');
IF @var73 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectEntryValue] DROP CONSTRAINT [' + @var73 + '];');
ALTER TABLE [Chat_ChatObjectEntryValue] DROP COLUMN [DestinationId];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230606100526_ChatObjectEntryValue_DropProp_DestinationId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_EntryValue_EntryNameId] ON [Chat_EntryValue];
GO

CREATE INDEX [IX_Chat_EntryValue_EntryNameId_Value] ON [Chat_EntryValue] ([EntryNameId], [Value]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230606100905_EntryValue_AddIndex_EntryNameId_Value_Asc', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_EntryValue] ADD [IsOption] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_EntryValue] ADD [IsPublic] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_EntryValue] ADD [IsStatic] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230606120311_EntryValue_AddProp_IsStatic_IsPublic_IsOption', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_EntryName] ADD [IsChoice] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230606122518_EntryName_AddProp_IsChoice', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_EntryName_Name] ON [Chat_EntryName];
DECLARE @var74 sysname;
SELECT @var74 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryName]') AND [c].[name] = N'Name');
IF @var74 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryName] DROP CONSTRAINT [' + @var74 + '];');
UPDATE [Chat_EntryName] SET [Name] = N'' WHERE [Name] IS NULL;
ALTER TABLE [Chat_EntryName] ALTER COLUMN [Name] nvarchar(64) NOT NULL;
ALTER TABLE [Chat_EntryName] ADD DEFAULT N'' FOR [Name];
CREATE INDEX [IX_Chat_EntryName_Name] ON [Chat_EntryName] ([Name]);
GO

DECLARE @var75 sysname;
SELECT @var75 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryName]') AND [c].[name] = N'Description');
IF @var75 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryName] DROP CONSTRAINT [' + @var75 + '];');
ALTER TABLE [Chat_EntryName] ALTER COLUMN [Description] nvarchar(500) NULL;
GO

ALTER TABLE [Chat_EntryName] ADD [ChildrenCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_EntryName] ADD [Depth] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Chat_EntryName] ADD [FullPath] nvarchar(1000) NULL;
GO

ALTER TABLE [Chat_EntryName] ADD [FullPathName] nvarchar(1000) NULL;
GO

ALTER TABLE [Chat_EntryName] ADD [ParentId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_EntryName] ADD [Sorting] float NOT NULL DEFAULT 0.0E0;
GO

CREATE INDEX [IX_Chat_EntryName_FullPath] ON [Chat_EntryName] ([FullPath]);
GO

CREATE INDEX [IX_Chat_EntryName_ParentId] ON [Chat_EntryName] ([ParentId]);
GO

ALTER TABLE [Chat_EntryName] ADD CONSTRAINT [FK_Chat_EntryName_Chat_EntryName_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Chat_EntryName] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230607010757_Entry_Tree_Fix', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [Chat_FriendshipTagUnit];
GO

DROP TABLE [Chat_FriendshipTag];
GO

DROP TABLE [Chat_Friendship];
GO

DROP TABLE [Chat_FriendshipRequest];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230612064332_Drop_Friendships', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

EXEC sp_rename N'[Chat_SessionUnitSetting].[IsCantacts]', N'IsContacts', N'COLUMN';
GO

EXEC sp_rename N'[Chat_SessionSetting].[IsCantacts]', N'IsContacts', N'COLUMN';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230613064643_IsContacts_Fix', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [Chat_SessionSetting];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230613065242_Drop_SessionSetting', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_Blob] (
    [Id] uniqueidentifier NOT NULL,
    [Container] nvarchar(50) NOT NULL,
    [Name] nvarchar(256) NULL,
    [FileName] nvarchar(256) NULL,
    [FileSize] bigint NOT NULL,
    [MimeType] nvarchar(50) NULL,
    [Suffix] nvarchar(10) NULL,
    [IsPublic] bit NOT NULL,
    [IsStatic] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_Blob] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Chat_BlobContent] (
    [BlobId] uniqueidentifier NOT NULL,
    [Content] varbinary(max) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_BlobContent] PRIMARY KEY ([BlobId]),
    CONSTRAINT [FK_Chat_BlobContent_Chat_Blob_BlobId] FOREIGN KEY ([BlobId]) REFERENCES [Chat_Blob] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Chat_Blob_Container] ON [Chat_Blob] ([Container]);
GO

CREATE INDEX [IX_Chat_Blob_MimeType] ON [Chat_Blob] ([MimeType]);
GO

CREATE INDEX [IX_Chat_Blob_Name] ON [Chat_Blob] ([Name]);
GO

CREATE INDEX [IX_Chat_Blob_Suffix] ON [Chat_Blob] ([Suffix]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230614024606_Blob_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_ContactTag] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(20) NULL,
    [Index] nvarchar(1) NULL,
    [OwnerId] bigint NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_ContactTag] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_ContactTag_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE TABLE [Chat_SessionUnitContactTag] (
    [SessionUnitId] uniqueidentifier NOT NULL,
    [TagId] uniqueidentifier NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_SessionUnitContactTag] PRIMARY KEY ([SessionUnitId], [TagId]),
    CONSTRAINT [FK_Chat_SessionUnitContactTag_Chat_ContactTag_TagId] FOREIGN KEY ([TagId]) REFERENCES [Chat_ContactTag] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_SessionUnitContactTag_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Chat_ContactTag_OwnerId] ON [Chat_ContactTag] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_SessionUnitContactTag_TagId] ON [Chat_SessionUnitContactTag] ([TagId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230614032826_ContactTag_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_Sorting_LastMessageId] ON [Chat_SessionUnit];
GO

CREATE INDEX [IX_Chat_SessionUnit_CreationTime] ON [Chat_SessionUnit] ([CreationTime] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_IsDeleted] ON [Chat_SessionUnit] ([IsDeleted]);
GO

CREATE INDEX [IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc] ON [Chat_SessionUnit] ([Sorting] DESC, [LastMessageId], [IsDeleted] DESC, [TenantId] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_Sorting_LastMessageId_IsDeleted_TenantId] ON [Chat_SessionUnit] ([Sorting] DESC, [LastMessageId] DESC, [IsDeleted] DESC, [TenantId] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_TenantId] ON [Chat_SessionUnit] ([TenantId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230614080137_SessionUnit_AddIndex_TenantId_IsDeleted_CreationTime', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId] ON [Chat_Message];
GO

CREATE INDEX [IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_TenantId] ON [Chat_Message] ([SessionId], [IsPrivate], [SenderId], [ReceiverId], [IsDeleted], [TenantId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230614100800_Message_FixIndex_IsDeleted_TenantId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'删除人';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'KillerId';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'删除渠道';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'KillType';
GO

ALTER TABLE [Chat_Session] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230615130133_Session_AddProp_IsEnabled', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'钱包Id';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletRecorder', 'COLUMN', N'WalletId';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'钱包业务Id';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletRecorder', 'COLUMN', N'WalletBusinessId';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'变化前-总金额';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletRecorder', 'COLUMN', N'TotalAmountBeforeChange';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'总金额';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletRecorder', 'COLUMN', N'TotalAmount';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'变化前-冻结金额';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletRecorder', 'COLUMN', N'LockAmountBeforeChange';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'冻结金额';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletRecorder', 'COLUMN', N'LockAmount';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'说明';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletRecorder', 'COLUMN', N'Description';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'变化前-可用金额';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletRecorder', 'COLUMN', N'AvailableAmountBeforeChange';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'可用金额';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletRecorder', 'COLUMN', N'AvailableAmount';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'变更金额';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletRecorder', 'COLUMN', N'Amount';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'总金额=可用金额+冻结金额';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Wallet', 'COLUMN', N'TotalAmount';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'密码';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Wallet', 'COLUMN', N'Password';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'冻结金额';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Wallet', 'COLUMN', N'LockAmount';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'可用金额';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Wallet', 'COLUMN', N'AvailableAmount';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230617033057_Wallet_Fix_Comment', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Wallet] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Chat_Wallet] ADD [IsLocked] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230617073602_Wallet_AddProp_IsEnabled_IsLocked', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_WalletOrder] (
    [Id] uniqueidentifier NOT NULL,
    [OrderNo] nvarchar(40) NULL,
    [AppUserId] uniqueidentifier NULL,
    [OwnerId] bigint NULL,
    [WalletId] uniqueidentifier NOT NULL,
    [BusinessId] nvarchar(450) NOT NULL,
    [BusinessType] int NOT NULL,
    [BusinessTypeName] nvarchar(100) NULL,
    [Title] nvarchar(100) NULL,
    [Description] nvarchar(100) NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [ExpireTime] datetime2 NULL,
    [IsExpired] bit NOT NULL,
    [IsEnabled] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_WalletOrder] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_WalletOrder_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id]),
    CONSTRAINT [FK_Chat_WalletOrder_Chat_WalletBusiness_BusinessId] FOREIGN KEY ([BusinessId]) REFERENCES [Chat_WalletBusiness] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_WalletOrder_Chat_Wallet_WalletId] FOREIGN KEY ([WalletId]) REFERENCES [Chat_Wallet] ([Id]) ON DELETE CASCADE
);
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'钱包Id';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletOrder', 'COLUMN', N'WalletId';
SET @description = N'钱包业务Id';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletOrder', 'COLUMN', N'BusinessId';
SET @description = N'标题';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletOrder', 'COLUMN', N'Title';
SET @description = N'说明';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletOrder', 'COLUMN', N'Description';
SET @description = N'变更金额';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletOrder', 'COLUMN', N'Amount';
SET @description = N'订单状态';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletOrder', 'COLUMN', N'Status';
SET @description = N'到期时间';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletOrder', 'COLUMN', N'ExpireTime';
SET @description = N'是否到期';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletOrder', 'COLUMN', N'IsExpired';
SET @description = N'是否有效';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_WalletOrder', 'COLUMN', N'IsEnabled';
GO

CREATE INDEX [IX_Chat_WalletOrder_BusinessId] ON [Chat_WalletOrder] ([BusinessId]);
GO

CREATE INDEX [IX_Chat_WalletOrder_OwnerId] ON [Chat_WalletOrder] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_WalletOrder_WalletId] ON [Chat_WalletOrder] ([WalletId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230617101408_WalletOrder_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_WalletOrder] ADD [RecorderId] uniqueidentifier NULL;
GO

CREATE INDEX [IX_Chat_WalletRecorder_AvailableAmount] ON [Chat_WalletRecorder] ([AvailableAmount]);
GO

CREATE INDEX [IX_Chat_WalletRecorder_LockAmount] ON [Chat_WalletRecorder] ([LockAmount]);
GO

CREATE INDEX [IX_Chat_WalletRecorder_TotalAmount] ON [Chat_WalletRecorder] ([TotalAmount]);
GO

CREATE INDEX [IX_Chat_WalletRecorder_WalletBusinessType] ON [Chat_WalletRecorder] ([WalletBusinessType]);
GO

CREATE INDEX [IX_Chat_WalletOrder_Amount] ON [Chat_WalletOrder] ([Amount]);
GO

CREATE INDEX [IX_Chat_WalletOrder_BusinessType] ON [Chat_WalletOrder] ([BusinessType]);
GO

CREATE INDEX [IX_Chat_WalletOrder_IsEnabled] ON [Chat_WalletOrder] ([IsEnabled]);
GO

CREATE INDEX [IX_Chat_WalletOrder_IsExpired] ON [Chat_WalletOrder] ([IsExpired]);
GO

CREATE INDEX [IX_Chat_WalletOrder_OrderNo] ON [Chat_WalletOrder] ([OrderNo] DESC);
GO

CREATE INDEX [IX_Chat_WalletOrder_RecorderId] ON [Chat_WalletOrder] ([RecorderId]);
GO

CREATE INDEX [IX_Chat_WalletOrder_Status] ON [Chat_WalletOrder] ([Status]);
GO

CREATE INDEX [IX_Chat_Wallet_AppUserId] ON [Chat_Wallet] ([AppUserId]);
GO

CREATE INDEX [IX_Chat_Wallet_AvailableAmount] ON [Chat_Wallet] ([AvailableAmount]);
GO

CREATE INDEX [IX_Chat_Wallet_IsEnabled] ON [Chat_Wallet] ([IsEnabled]);
GO

CREATE INDEX [IX_Chat_Wallet_IsLocked] ON [Chat_Wallet] ([IsLocked]);
GO

CREATE INDEX [IX_Chat_Wallet_LockAmount] ON [Chat_Wallet] ([LockAmount]);
GO

CREATE INDEX [IX_Chat_Wallet_TotalAmount] ON [Chat_Wallet] ([TotalAmount]);
GO

ALTER TABLE [Chat_WalletOrder] ADD CONSTRAINT [FK_Chat_WalletOrder_Chat_WalletRecorder_RecorderId] FOREIGN KEY ([RecorderId]) REFERENCES [Chat_WalletRecorder] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230619022501_Wallet_AddIndex', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_WalletRecorder] DROP CONSTRAINT [FK_Chat_WalletRecorder_Chat_WalletBusiness_WalletBusinessId];
GO

EXEC sp_rename N'[Chat_WalletRecorder].[WalletBusinessType]', N'BusinessType', N'COLUMN';
GO

EXEC sp_rename N'[Chat_WalletRecorder].[WalletBusinessId]', N'BusinessId', N'COLUMN';
GO

EXEC sp_rename N'[Chat_WalletRecorder].[IX_Chat_WalletRecorder_WalletBusinessType]', N'IX_Chat_WalletRecorder_BusinessType', N'INDEX';
GO

EXEC sp_rename N'[Chat_WalletRecorder].[IX_Chat_WalletRecorder_WalletBusinessId]', N'IX_Chat_WalletRecorder_BusinessId', N'INDEX';
GO

ALTER TABLE [Chat_WalletRecorder] ADD CONSTRAINT [FK_Chat_WalletRecorder_Chat_WalletBusiness_BusinessId] FOREIGN KEY ([BusinessId]) REFERENCES [Chat_WalletBusiness] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230619033821_WalletRecorder_RenameProp', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Session] ADD [IsEnableSetImmersed] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230627091703_Session_AddProp_IsEnableSetImmersed', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_Chat_SessionRequest_CreationTime] ON [Chat_SessionRequest] ([CreationTime]);
GO

CREATE INDEX [IX_Chat_SessionRequest_CreationTime_Desc] ON [Chat_SessionRequest] ([CreationTime] DESC);
GO

CREATE INDEX [IX_Chat_SessionRequest_HandleTime] ON [Chat_SessionRequest] ([HandleTime]);
GO

CREATE INDEX [IX_Chat_SessionRequest_HandleTime_Desc] ON [Chat_SessionRequest] ([HandleTime] DESC);
GO

CREATE INDEX [IX_Chat_SessionRequest_IsAgreed] ON [Chat_SessionRequest] ([IsAgreed]);
GO

CREATE INDEX [IX_Chat_SessionRequest_IsHandled] ON [Chat_SessionRequest] ([IsHandled]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230628054613_SessionRequest_AddIndex_CreationTime', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_Chat_ChatObject_AppUserId] ON [Chat_ChatObject] ([AppUserId]);
GO

CREATE INDEX [IX_Chat_ChatObject_CreationTime] ON [Chat_ChatObject] ([CreationTime] DESC);
GO

CREATE INDEX [IX_Chat_ChatObject_Gender] ON [Chat_ChatObject] ([Gender]);
GO

CREATE INDEX [IX_Chat_ChatObject_IsDefault] ON [Chat_ChatObject] ([IsDefault]);
GO

CREATE INDEX [IX_Chat_ChatObject_IsDeveloper] ON [Chat_ChatObject] ([IsDeveloper]);
GO

CREATE INDEX [IX_Chat_ChatObject_IsEnabled] ON [Chat_ChatObject] ([IsEnabled]);
GO

CREATE INDEX [IX_Chat_ChatObject_IsPublic] ON [Chat_ChatObject] ([IsPublic]);
GO

CREATE INDEX [IX_Chat_ChatObject_IsStatic] ON [Chat_ChatObject] ([IsStatic]);
GO

CREATE INDEX [IX_Chat_ChatObject_ObjectType] ON [Chat_ChatObject] ([ObjectType]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230629033918_ChatObject_AddIndex_AppUserId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [MuteExpireTime] datetime2 NULL;
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'禁言过期时间，为空则不禁言';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'MuteExpireTime';
GO

CREATE INDEX [IX_Chat_SessionUnitSetting_MuteExpireTime] ON [Chat_SessionUnitSetting] ([MuteExpireTime]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230630085906_SessionUnitSetting_AddProp_MuteExpireTime', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_Sorting_LastMessageId_IsDeleted_TenantId] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_SessionUnit_TenantId] ON [Chat_SessionUnit];
GO

DROP INDEX [IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_TenantId] ON [Chat_Message];
GO

DECLARE @var76 sysname;
SELECT @var76 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Word]') AND [c].[name] = N'TenantId');
IF @var76 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Word] DROP CONSTRAINT [' + @var76 + '];');
ALTER TABLE [Chat_Word] DROP COLUMN [TenantId];
GO

DECLARE @var77 sysname;
SELECT @var77 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletRequest]') AND [c].[name] = N'TenantId');
IF @var77 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletRequest] DROP CONSTRAINT [' + @var77 + '];');
ALTER TABLE [Chat_WalletRequest] DROP COLUMN [TenantId];
GO

DECLARE @var78 sysname;
SELECT @var78 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletRecorder]') AND [c].[name] = N'TenantId');
IF @var78 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletRecorder] DROP CONSTRAINT [' + @var78 + '];');
ALTER TABLE [Chat_WalletRecorder] DROP COLUMN [TenantId];
GO

DECLARE @var79 sysname;
SELECT @var79 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletOrder]') AND [c].[name] = N'TenantId');
IF @var79 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletOrder] DROP CONSTRAINT [' + @var79 + '];');
ALTER TABLE [Chat_WalletOrder] DROP COLUMN [TenantId];
GO

DECLARE @var80 sysname;
SELECT @var80 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletBusiness]') AND [c].[name] = N'TenantId');
IF @var80 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletBusiness] DROP CONSTRAINT [' + @var80 + '];');
ALTER TABLE [Chat_WalletBusiness] DROP COLUMN [TenantId];
GO

DECLARE @var81 sysname;
SELECT @var81 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Wallet]') AND [c].[name] = N'TenantId');
IF @var81 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Wallet] DROP CONSTRAINT [' + @var81 + '];');
ALTER TABLE [Chat_Wallet] DROP COLUMN [TenantId];
GO

DECLARE @var82 sysname;
SELECT @var82 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_TextContentWord]') AND [c].[name] = N'TenantId');
IF @var82 IS NOT NULL EXEC(N'ALTER TABLE [Chat_TextContentWord] DROP CONSTRAINT [' + @var82 + '];');
ALTER TABLE [Chat_TextContentWord] DROP COLUMN [TenantId];
GO

DECLARE @var83 sysname;
SELECT @var83 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitTag]') AND [c].[name] = N'TenantId');
IF @var83 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitTag] DROP CONSTRAINT [' + @var83 + '];');
ALTER TABLE [Chat_SessionUnitTag] DROP COLUMN [TenantId];
GO

DECLARE @var84 sysname;
SELECT @var84 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitSetting]') AND [c].[name] = N'TenantId');
IF @var84 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitSetting] DROP CONSTRAINT [' + @var84 + '];');
ALTER TABLE [Chat_SessionUnitSetting] DROP COLUMN [TenantId];
GO

DECLARE @var85 sysname;
SELECT @var85 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitRole]') AND [c].[name] = N'TenantId');
IF @var85 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitRole] DROP CONSTRAINT [' + @var85 + '];');
ALTER TABLE [Chat_SessionUnitRole] DROP COLUMN [TenantId];
GO

DECLARE @var86 sysname;
SELECT @var86 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitOrganization]') AND [c].[name] = N'TenantId');
IF @var86 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitOrganization] DROP CONSTRAINT [' + @var86 + '];');
ALTER TABLE [Chat_SessionUnitOrganization] DROP COLUMN [TenantId];
GO

DECLARE @var87 sysname;
SELECT @var87 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitEntryValue]') AND [c].[name] = N'TenantId');
IF @var87 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitEntryValue] DROP CONSTRAINT [' + @var87 + '];');
ALTER TABLE [Chat_SessionUnitEntryValue] DROP COLUMN [TenantId];
GO

DECLARE @var88 sysname;
SELECT @var88 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitCounter]') AND [c].[name] = N'TenantId');
IF @var88 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitCounter] DROP CONSTRAINT [' + @var88 + '];');
ALTER TABLE [Chat_SessionUnitCounter] DROP COLUMN [TenantId];
GO

DECLARE @var89 sysname;
SELECT @var89 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitContactTag]') AND [c].[name] = N'TenantId');
IF @var89 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitContactTag] DROP CONSTRAINT [' + @var89 + '];');
ALTER TABLE [Chat_SessionUnitContactTag] DROP COLUMN [TenantId];
GO

DECLARE @var90 sysname;
SELECT @var90 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'TenantId');
IF @var90 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var90 + '];');
ALTER TABLE [Chat_SessionUnit] DROP COLUMN [TenantId];
GO

DECLARE @var91 sysname;
SELECT @var91 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionTag]') AND [c].[name] = N'TenantId');
IF @var91 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionTag] DROP CONSTRAINT [' + @var91 + '];');
ALTER TABLE [Chat_SessionTag] DROP COLUMN [TenantId];
GO

DECLARE @var92 sysname;
SELECT @var92 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionRole]') AND [c].[name] = N'TenantId');
IF @var92 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionRole] DROP CONSTRAINT [' + @var92 + '];');
ALTER TABLE [Chat_SessionRole] DROP COLUMN [TenantId];
GO

DECLARE @var93 sysname;
SELECT @var93 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionRequest]') AND [c].[name] = N'TenantId');
IF @var93 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionRequest] DROP CONSTRAINT [' + @var93 + '];');
ALTER TABLE [Chat_SessionRequest] DROP COLUMN [TenantId];
GO

DECLARE @var94 sysname;
SELECT @var94 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionUnitGrant]') AND [c].[name] = N'TenantId');
IF @var94 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionUnitGrant] DROP CONSTRAINT [' + @var94 + '];');
ALTER TABLE [Chat_SessionPermissionUnitGrant] DROP COLUMN [TenantId];
GO

DECLARE @var95 sysname;
SELECT @var95 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionRoleGrant]') AND [c].[name] = N'TenantId');
IF @var95 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionRoleGrant] DROP CONSTRAINT [' + @var95 + '];');
ALTER TABLE [Chat_SessionPermissionRoleGrant] DROP COLUMN [TenantId];
GO

DECLARE @var96 sysname;
SELECT @var96 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionDefinition]') AND [c].[name] = N'TenantId');
IF @var96 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionDefinition] DROP CONSTRAINT [' + @var96 + '];');
ALTER TABLE [Chat_SessionPermissionDefinition] DROP COLUMN [TenantId];
GO

DECLARE @var97 sysname;
SELECT @var97 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Session]') AND [c].[name] = N'TenantId');
IF @var97 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Session] DROP CONSTRAINT [' + @var97 + '];');
ALTER TABLE [Chat_Session] DROP COLUMN [TenantId];
GO

DECLARE @var98 sysname;
SELECT @var98 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Scoped]') AND [c].[name] = N'TenantId');
IF @var98 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Scoped] DROP CONSTRAINT [' + @var98 + '];');
ALTER TABLE [Chat_Scoped] DROP COLUMN [TenantId];
GO

DECLARE @var99 sysname;
SELECT @var99 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_RedEnvelopeUnit]') AND [c].[name] = N'TenantId');
IF @var99 IS NOT NULL EXEC(N'ALTER TABLE [Chat_RedEnvelopeUnit] DROP CONSTRAINT [' + @var99 + '];');
ALTER TABLE [Chat_RedEnvelopeUnit] DROP COLUMN [TenantId];
GO

DECLARE @var100 sysname;
SELECT @var100 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'TenantId');
IF @var100 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var100 + '];');
ALTER TABLE [Chat_ReadedRecorder] DROP COLUMN [TenantId];
GO

DECLARE @var101 sysname;
SELECT @var101 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_PaymentPlatform]') AND [c].[name] = N'TenantId');
IF @var101 IS NOT NULL EXEC(N'ALTER TABLE [Chat_PaymentPlatform] DROP CONSTRAINT [' + @var101 + '];');
ALTER TABLE [Chat_PaymentPlatform] DROP COLUMN [TenantId];
GO

DECLARE @var102 sysname;
SELECT @var102 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_OpenedRecorder]') AND [c].[name] = N'TenantId');
IF @var102 IS NOT NULL EXEC(N'ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [' + @var102 + '];');
ALTER TABLE [Chat_OpenedRecorder] DROP COLUMN [TenantId];
GO

DECLARE @var103 sysname;
SELECT @var103 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Motto]') AND [c].[name] = N'TenantId');
IF @var103 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Motto] DROP CONSTRAINT [' + @var103 + '];');
ALTER TABLE [Chat_Motto] DROP COLUMN [TenantId];
GO

DECLARE @var104 sysname;
SELECT @var104 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageReminder]') AND [c].[name] = N'TenantId');
IF @var104 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageReminder] DROP CONSTRAINT [' + @var104 + '];');
ALTER TABLE [Chat_MessageReminder] DROP COLUMN [TenantId];
GO

DECLARE @var105 sysname;
SELECT @var105 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageContent]') AND [c].[name] = N'TenantId');
IF @var105 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageContent] DROP CONSTRAINT [' + @var105 + '];');
ALTER TABLE [Chat_MessageContent] DROP COLUMN [TenantId];
GO

DECLARE @var106 sysname;
SELECT @var106 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_VideoContent]') AND [c].[name] = N'TenantId');
IF @var106 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_VideoContent] DROP CONSTRAINT [' + @var106 + '];');
ALTER TABLE [Chat_Message_Template_VideoContent] DROP COLUMN [TenantId];
GO

DECLARE @var107 sysname;
SELECT @var107 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_TextContent]') AND [c].[name] = N'TenantId');
IF @var107 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_TextContent] DROP CONSTRAINT [' + @var107 + '];');
ALTER TABLE [Chat_Message_Template_TextContent] DROP COLUMN [TenantId];
GO

DECLARE @var108 sysname;
SELECT @var108 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_SoundContent]') AND [c].[name] = N'TenantId');
IF @var108 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_SoundContent] DROP CONSTRAINT [' + @var108 + '];');
ALTER TABLE [Chat_Message_Template_SoundContent] DROP COLUMN [TenantId];
GO

DECLARE @var109 sysname;
SELECT @var109 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_RedEnvelopeContent]') AND [c].[name] = N'TenantId');
IF @var109 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] DROP CONSTRAINT [' + @var109 + '];');
ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] DROP COLUMN [TenantId];
GO

DECLARE @var110 sysname;
SELECT @var110 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_LocationContent]') AND [c].[name] = N'TenantId');
IF @var110 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_LocationContent] DROP CONSTRAINT [' + @var110 + '];');
ALTER TABLE [Chat_Message_Template_LocationContent] DROP COLUMN [TenantId];
GO

DECLARE @var111 sysname;
SELECT @var111 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_LinkContent]') AND [c].[name] = N'TenantId');
IF @var111 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_LinkContent] DROP CONSTRAINT [' + @var111 + '];');
ALTER TABLE [Chat_Message_Template_LinkContent] DROP COLUMN [TenantId];
GO

DECLARE @var112 sysname;
SELECT @var112 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ImageContent]') AND [c].[name] = N'TenantId');
IF @var112 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ImageContent] DROP CONSTRAINT [' + @var112 + '];');
ALTER TABLE [Chat_Message_Template_ImageContent] DROP COLUMN [TenantId];
GO

DECLARE @var113 sysname;
SELECT @var113 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_HtmlContent]') AND [c].[name] = N'TenantId');
IF @var113 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_HtmlContent] DROP CONSTRAINT [' + @var113 + '];');
ALTER TABLE [Chat_Message_Template_HtmlContent] DROP COLUMN [TenantId];
GO

DECLARE @var114 sysname;
SELECT @var114 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_HistoryContent]') AND [c].[name] = N'TenantId');
IF @var114 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_HistoryContent] DROP CONSTRAINT [' + @var114 + '];');
ALTER TABLE [Chat_Message_Template_HistoryContent] DROP COLUMN [TenantId];
GO

DECLARE @var115 sysname;
SELECT @var115 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_FileContent]') AND [c].[name] = N'TenantId');
IF @var115 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_FileContent] DROP CONSTRAINT [' + @var115 + '];');
ALTER TABLE [Chat_Message_Template_FileContent] DROP COLUMN [TenantId];
GO

DECLARE @var116 sysname;
SELECT @var116 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ContactsContent]') AND [c].[name] = N'TenantId');
IF @var116 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ContactsContent] DROP CONSTRAINT [' + @var116 + '];');
ALTER TABLE [Chat_Message_Template_ContactsContent] DROP COLUMN [TenantId];
GO

DECLARE @var117 sysname;
SELECT @var117 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_CmdContent]') AND [c].[name] = N'TenantId');
IF @var117 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_CmdContent] DROP CONSTRAINT [' + @var117 + '];');
ALTER TABLE [Chat_Message_Template_CmdContent] DROP COLUMN [TenantId];
GO

DECLARE @var118 sysname;
SELECT @var118 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ArticleContent]') AND [c].[name] = N'TenantId');
IF @var118 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ArticleContent] DROP CONSTRAINT [' + @var118 + '];');
ALTER TABLE [Chat_Message_Template_ArticleContent] DROP COLUMN [TenantId];
GO

DECLARE @var119 sysname;
SELECT @var119 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message]') AND [c].[name] = N'TenantId');
IF @var119 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message] DROP CONSTRAINT [' + @var119 + '];');
ALTER TABLE [Chat_Message] DROP COLUMN [TenantId];
GO

DECLARE @var120 sysname;
SELECT @var120 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HttpResponse]') AND [c].[name] = N'TenantId');
IF @var120 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HttpResponse] DROP CONSTRAINT [' + @var120 + '];');
ALTER TABLE [Chat_HttpResponse] DROP COLUMN [TenantId];
GO

DECLARE @var121 sysname;
SELECT @var121 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HttpRequest]') AND [c].[name] = N'TenantId');
IF @var121 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HttpRequest] DROP CONSTRAINT [' + @var121 + '];');
ALTER TABLE [Chat_HttpRequest] DROP COLUMN [TenantId];
GO

DECLARE @var122 sysname;
SELECT @var122 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HistoryMessage]') AND [c].[name] = N'TenantId');
IF @var122 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HistoryMessage] DROP CONSTRAINT [' + @var122 + '];');
ALTER TABLE [Chat_HistoryMessage] DROP COLUMN [TenantId];
GO

DECLARE @var123 sysname;
SELECT @var123 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Follow]') AND [c].[name] = N'TenantId');
IF @var123 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Follow] DROP CONSTRAINT [' + @var123 + '];');
ALTER TABLE [Chat_Follow] DROP COLUMN [TenantId];
GO

DECLARE @var124 sysname;
SELECT @var124 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_FavoritedRecorder]') AND [c].[name] = N'TenantId');
IF @var124 IS NOT NULL EXEC(N'ALTER TABLE [Chat_FavoritedRecorder] DROP CONSTRAINT [' + @var124 + '];');
ALTER TABLE [Chat_FavoritedRecorder] DROP COLUMN [TenantId];
GO

DECLARE @var125 sysname;
SELECT @var125 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryValue]') AND [c].[name] = N'TenantId');
IF @var125 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryValue] DROP CONSTRAINT [' + @var125 + '];');
ALTER TABLE [Chat_EntryValue] DROP COLUMN [TenantId];
GO

DECLARE @var126 sysname;
SELECT @var126 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Developer]') AND [c].[name] = N'TenantId');
IF @var126 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Developer] DROP CONSTRAINT [' + @var126 + '];');
ALTER TABLE [Chat_Developer] DROP COLUMN [TenantId];
GO

DECLARE @var127 sysname;
SELECT @var127 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ContactTag]') AND [c].[name] = N'TenantId');
IF @var127 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ContactTag] DROP CONSTRAINT [' + @var127 + '];');
ALTER TABLE [Chat_ContactTag] DROP COLUMN [TenantId];
GO

DECLARE @var128 sysname;
SELECT @var128 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Connection]') AND [c].[name] = N'TenantId');
IF @var128 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Connection] DROP CONSTRAINT [' + @var128 + '];');
ALTER TABLE [Chat_Connection] DROP COLUMN [TenantId];
GO

DECLARE @var129 sysname;
SELECT @var129 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectType]') AND [c].[name] = N'TenantId');
IF @var129 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectType] DROP CONSTRAINT [' + @var129 + '];');
ALTER TABLE [Chat_ChatObjectType] DROP COLUMN [TenantId];
GO

DECLARE @var130 sysname;
SELECT @var130 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectEntryValue]') AND [c].[name] = N'TenantId');
IF @var130 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectEntryValue] DROP CONSTRAINT [' + @var130 + '];');
ALTER TABLE [Chat_ChatObjectEntryValue] DROP COLUMN [TenantId];
GO

DECLARE @var131 sysname;
SELECT @var131 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectCategoryUnit]') AND [c].[name] = N'TenantId');
IF @var131 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectCategoryUnit] DROP CONSTRAINT [' + @var131 + '];');
ALTER TABLE [Chat_ChatObjectCategoryUnit] DROP COLUMN [TenantId];
GO

DECLARE @var132 sysname;
SELECT @var132 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_BlobContent]') AND [c].[name] = N'TenantId');
IF @var132 IS NOT NULL EXEC(N'ALTER TABLE [Chat_BlobContent] DROP CONSTRAINT [' + @var132 + '];');
ALTER TABLE [Chat_BlobContent] DROP COLUMN [TenantId];
GO

DECLARE @var133 sysname;
SELECT @var133 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Blob]') AND [c].[name] = N'TenantId');
IF @var133 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Blob] DROP CONSTRAINT [' + @var133 + '];');
ALTER TABLE [Chat_Blob] DROP COLUMN [TenantId];
GO

DECLARE @var134 sysname;
SELECT @var134 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ArticleMessage]') AND [c].[name] = N'TenantId');
IF @var134 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ArticleMessage] DROP CONSTRAINT [' + @var134 + '];');
ALTER TABLE [Chat_ArticleMessage] DROP COLUMN [TenantId];
GO

DECLARE @var135 sysname;
SELECT @var135 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Article]') AND [c].[name] = N'TenantId');
IF @var135 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Article] DROP CONSTRAINT [' + @var135 + '];');
ALTER TABLE [Chat_Article] DROP COLUMN [TenantId];
GO

CREATE INDEX [IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc] ON [Chat_SessionUnit] ([Sorting] DESC, [LastMessageId], [IsDeleted] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnit_Sorting_LastMessageId_IsDeleted] ON [Chat_SessionUnit] ([Sorting] DESC, [LastMessageId] DESC, [IsDeleted] DESC);
GO

CREATE INDEX [IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted] ON [Chat_Message] ([SessionId], [IsPrivate], [SenderId], [ReceiverId], [IsDeleted]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230703092552_Remove_TenantId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

EXEC sp_rename N'[Chat_SessionUnitCounter].[IX_Chat_SessionUnitCounter_LastMessageId]', N'IX_Chat_SessionUnitCounter_LastMessageId_Desc', N'INDEX';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'会话设置';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'备注拼音简写';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'RenameSpellingAbbreviation';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'备注拼音简写';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'RenameSpelling';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'会话内的名称拼音简写';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'MemberNameSpellingAbbreviation';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'会话内的名称拼音';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'MemberNameSpelling';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'KillerId';
SET @description = N'删除人Id';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'KillerId';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'加入方式';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'JoinWay';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'邀请人Id';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'InviterId';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'聊天背景，默认为 null';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'BackgroundImage';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'时间';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnit', 'COLUMN', N'Ticks';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'置顶(排序)';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnit', 'COLUMN', N'Sorting';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'会话Id';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnit', 'COLUMN', N'SessionId';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'提醒器数量(@我)';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnit', 'COLUMN', N'RemindMeCount';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'提醒器数量(@所有人)';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnit', 'COLUMN', N'RemindAllCount';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'最后一条消息Id';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnit', 'COLUMN', N'PublicBadge';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'私有消息角标(未读消息数量)';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnit', 'COLUMN', N'PrivateBadge';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'最后一条消息Id';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnit', 'COLUMN', N'LastMessageId';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'特别关注数量';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnit', 'COLUMN', N'FollowingCount';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'用户Id';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnit', 'COLUMN', N'AppUserId';
GO

CREATE INDEX [IX_Chat_SessionUnitCounter_LastMessageId] ON [Chat_SessionUnitCounter] ([LastMessageId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230713065011_SessionUnit_Session_Comment', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_Chat_Message_IsDeleted] ON [Chat_Message] ([IsDeleted]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230713065150_Message_AddIndex_IsDeleted', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_Message_SenderId] ON [Chat_Message];
GO

CREATE INDEX [IX_Chat_Message_SenderId_ReceiverId] ON [Chat_Message] ([SenderId], [ReceiverId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230713070508_Message_AddIndex_SenderId_ReceiverId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_Chat_Message_IsPrivate_SenderId_ReceiverId] ON [Chat_Message] ([IsPrivate], [SenderId], [ReceiverId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230713070700_Message_AddIndex_SenderId_ReceiverId_IsPrivate', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_Message_IsPrivate_SenderId_ReceiverId] ON [Chat_Message];
GO

DROP INDEX [IX_Chat_Message_SenderId_ReceiverId] ON [Chat_Message];
GO

DROP INDEX [IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted] ON [Chat_Message];
GO

CREATE INDEX [IX_Chat_Message_SenderId] ON [Chat_Message] ([SenderId]);
GO

CREATE INDEX [IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_CreationTime] ON [Chat_Message] ([SessionId], [IsPrivate], [SenderId], [ReceiverId], [IsDeleted], [CreationTime]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230713071024_Message_FixIndex_SenderId_ReceiverId_IsPrivate', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'消息大小kb';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'Size';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'会话单元Id';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'SessionUnitId';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'会话Id';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'SessionId';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'发送者类型';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'SenderType';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'发送者';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'SenderId';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'撤回消息时间';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'RollbackTime';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'接收者类型';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'ReceiverType';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'接收者';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'ReceiverId';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'消息类型';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'MessageType';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'扩展（键值）根据业务自义';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'KeyValue';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'扩展（键名）根据业务自义';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'KeyName';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'是否撤回';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'IsRollbacked';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'私有消息(只有发送人[senderId]和接收人[receiverId]才能看)';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'IsPrivate';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'ContentJson';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'ContentJson';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'消息通道';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Message', 'COLUMN', N'Channel';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230714040433_Message_AddComment', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_CreationTime] ON [Chat_Message];
GO

CREATE INDEX [IX_Chat_Message_ForwardDepth] ON [Chat_Message] ([ForwardDepth]);
GO

CREATE INDEX [IX_Chat_Message_QuoteDepth] ON [Chat_Message] ([QuoteDepth]);
GO

CREATE INDEX [IX_Chat_Message_SessionId_IsPrivate_SenderId_ReceiverId_IsDeleted_CreationTime_ForwardDepth_QuoteDepth] ON [Chat_Message] ([SessionId], [IsPrivate], [SenderId], [ReceiverId], [IsDeleted], [CreationTime], [ForwardDepth], [QuoteDepth]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230715024922_Message_FixIndex_ForwardDepth_QuoteDepth', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Follow] DROP CONSTRAINT [FK_Chat_Follow_Chat_SessionUnit_OwnerId];
GO

EXEC sp_rename N'[Chat_Follow].[OwnerId]', N'SessionUnitId', N'COLUMN';
GO

ALTER TABLE [Chat_Follow] ADD CONSTRAINT [FK_Chat_Follow_Chat_SessionUnit_SessionUnitId] FOREIGN KEY ([SessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230717022541_Follow_AlertProp_SessionUnitId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_Chat_SessionUnit_OwnerObjectType] ON [Chat_SessionUnit] ([OwnerObjectType] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230901091649_SessionUnit_AddIndex_OwnerObjectType', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'SessionKey';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Session', 'COLUMN', N'SessionKey';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'是否可以设置为''免打扰''';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Session', 'COLUMN', N'IsEnableSetImmersed';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Description';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Session', 'COLUMN', N'Description';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Channel';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Session', 'COLUMN', N'Channel';
GO

ALTER TABLE [Chat_Session] ADD [MessageTotalCount] int NOT NULL DEFAULT 0;
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'消息总数量';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Session', 'COLUMN', N'MessageTotalCount';
GO

ALTER TABLE [Chat_Session] ADD [MessageTotalCountUpdateTime] datetime2 NULL;
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'更新消息总数量时间';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_Session', 'COLUMN', N'MessageTotalCountUpdateTime';
GO

CREATE INDEX [IX_Chat_Session_MessageTotalCount] ON [Chat_Session] ([MessageTotalCount] DESC);
GO

CREATE INDEX [IX_Chat_Session_MessageTotalCountUpdateTime] ON [Chat_Session] ([MessageTotalCountUpdateTime] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230926030852_Session_AddProp_MessageTotalCount', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_InvitationCode] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NULL,
    [ExpirationTime] datetime2 NOT NULL,
    [OwnerId] bigint NULL,
    [IsEnabled] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    CONSTRAINT [PK_Chat_InvitationCode] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_InvitationCode_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'邀请码';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_InvitationCode', 'COLUMN', N'Code';
SET @description = N'过期时间';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_InvitationCode', 'COLUMN', N'ExpirationTime';
SET @description = N'所属聊天对象';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_InvitationCode', 'COLUMN', N'OwnerId';
SET @description = N'是否可用';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_InvitationCode', 'COLUMN', N'IsEnabled';
GO

CREATE INDEX [IX_Chat_InvitationCode_Code] ON [Chat_InvitationCode] ([Code]);
GO

CREATE INDEX [IX_Chat_InvitationCode_OwnerId] ON [Chat_InvitationCode] ([OwnerId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230926092445_InvitationCode_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_InvitationCode] ADD [Title] nvarchar(200) NULL;
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'标题';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_InvitationCode', 'COLUMN', N'Title';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230926092820_InvitationCode_AddProp_Title', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_MessageWord] (
    [MessageId] bigint NOT NULL,
    [WordId] uniqueidentifier NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_MessageWord] PRIMARY KEY ([MessageId], [WordId]),
    CONSTRAINT [FK_Chat_MessageWord_Chat_Message_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Chat_Message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_MessageWord_Chat_Word_WordId] FOREIGN KEY ([WordId]) REFERENCES [Chat_Word] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Chat_MessageWord_WordId] ON [Chat_MessageWord] ([WordId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230927085713_MessageWord_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Message] DROP CONSTRAINT [FK_Chat_Message_Chat_SessionUnit_SessionUnitId];
GO

EXEC sp_rename N'[Chat_Message].[SessionUnitId]', N'SenderSessionUnitId', N'COLUMN';
GO

EXEC sp_rename N'[Chat_Message].[IX_Chat_Message_SessionUnitId]', N'IX_Chat_Message_SenderSessionUnitId', N'INDEX';
GO

ALTER TABLE [Chat_Message] ADD CONSTRAINT [FK_Chat_Message_Chat_SessionUnit_SenderSessionUnitId] FOREIGN KEY ([SenderSessionUnitId]) REFERENCES [Chat_SessionUnit] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20231116015929_Message_SenderSessionUnitId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [LastSendMessageId] bigint NULL;
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'最后发言的消息';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'LastSendMessageId';
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [LastSendTime] datetime2 NULL;
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'最后发言时间';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'LastSendTime';
GO

DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnit', 'COLUMN', N'PublicBadge';
SET @description = N'消息角标,包含了私有消息 PrivateBadge (未读消息数量)';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnit', 'COLUMN', N'PublicBadge';
GO

CREATE INDEX [IX_Chat_SessionUnitSetting_LastSendMessageId] ON [Chat_SessionUnitSetting] ([LastSendMessageId]);
GO

CREATE INDEX [IX_Chat_Message_SessionId_Id] ON [Chat_Message] ([SessionId] DESC, [Id] DESC);
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD CONSTRAINT [FK_Chat_SessionUnitSetting_Chat_Message_LastSendMessageId] FOREIGN KEY ([LastSendMessageId]) REFERENCES [Chat_Message] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240117015210_SessionUnitSetting_AddLastSendMessage', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_SessionUnitSetting_LastSendMessageId] ON [Chat_SessionUnitSetting];
GO

CREATE INDEX [IX_Chat_SessionUnitSetting_LastSendMessageId] ON [Chat_SessionUnitSetting] ([LastSendMessageId] DESC);
GO

CREATE INDEX [IX_Chat_SessionUnitSetting_LastSendTime] ON [Chat_SessionUnitSetting] ([LastSendTime] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240117015832_SessionUnitSetting_AddIndex_LastSendTime', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [SessionId] uniqueidentifier NULL;
GO

CREATE INDEX [IX_Chat_SessionUnitSetting_SessionId] ON [Chat_SessionUnitSetting] ([SessionId]);
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD CONSTRAINT [FK_Chat_SessionUnitSetting_Chat_Session_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Chat_Session] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240117021012_SessionUnitSetting_Add_SessionId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var136 sysname;
SELECT @var136 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryName]') AND [c].[name] = N'MinLenth');
IF @var136 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryName] DROP CONSTRAINT [' + @var136 + '];');
ALTER TABLE [Chat_EntryName] ALTER COLUMN [MinLenth] int NULL;
GO

DECLARE @var137 sysname;
SELECT @var137 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryName]') AND [c].[name] = N'MinCount');
IF @var137 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryName] DROP CONSTRAINT [' + @var137 + '];');
ALTER TABLE [Chat_EntryName] ALTER COLUMN [MinCount] int NULL;
GO

DECLARE @var138 sysname;
SELECT @var138 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryName]') AND [c].[name] = N'MaxLenth');
IF @var138 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryName] DROP CONSTRAINT [' + @var138 + '];');
ALTER TABLE [Chat_EntryName] ALTER COLUMN [MaxLenth] int NULL;
GO

DECLARE @var139 sysname;
SELECT @var139 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryName]') AND [c].[name] = N'MaxCount');
IF @var139 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryName] DROP CONSTRAINT [' + @var139 + '];');
ALTER TABLE [Chat_EntryName] ALTER COLUMN [MaxCount] int NULL;
GO

ALTER TABLE [Chat_EntryName] ADD [DefaultValue] nvarchar(500) NULL;
GO

ALTER TABLE [Chat_EntryName] ADD [InputType] nvarchar(20) NULL;
GO

CREATE INDEX [IX_Chat_EntryName_InputType] ON [Chat_EntryName] ([InputType]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240125012122_EntryName_Add_InputType', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_EntryName] ADD [Help] nvarchar(500) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240125014322_EntryName_Add_Help', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [IsDisplay] bit NOT NULL DEFAULT CAST(0 AS bit);
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'是否显示';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'IsDisplay';
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [IsVisible] bit NOT NULL DEFAULT CAST(0 AS bit);
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'是否可见的';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'IsVisible';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240131020954_SessionUnitSetting_Add_IsVisible_IsDisplay', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionUnitSetting] ADD [IsHideBadge] bit NOT NULL DEFAULT CAST(0 AS bit);
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'是否隐藏角标';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Chat_SessionUnitSetting', 'COLUMN', N'IsHideBadge';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240202095222_SessionUnitSetting_Add_IsHideBadge', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var140 sysname;
SELECT @var140 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObject]') AND [c].[name] = N'ServiceStatus');
IF @var140 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObject] DROP CONSTRAINT [' + @var140 + '];');
ALTER TABLE [Chat_ChatObject] DROP COLUMN [ServiceStatus];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240205085720_ChatObject_Drop_ServiceStatus', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Chat_Blob_Name] ON [Chat_Blob];
DECLARE @var141 sysname;
SELECT @var141 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Blob]') AND [c].[name] = N'Name');
IF @var141 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Blob] DROP CONSTRAINT [' + @var141 + '];');
ALTER TABLE [Chat_Blob] ALTER COLUMN [Name] nvarchar(500) NULL;
CREATE INDEX [IX_Chat_Blob_Name] ON [Chat_Blob] ([Name]);
GO

DROP INDEX [IX_Chat_Blob_MimeType] ON [Chat_Blob];
DECLARE @var142 sysname;
SELECT @var142 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Blob]') AND [c].[name] = N'MimeType');
IF @var142 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Blob] DROP CONSTRAINT [' + @var142 + '];');
ALTER TABLE [Chat_Blob] ALTER COLUMN [MimeType] nvarchar(100) NULL;
CREATE INDEX [IX_Chat_Blob_MimeType] ON [Chat_Blob] ([MimeType]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240227072113_Blob_Name', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Message_Template_VideoContent] ADD [BlobId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_Message_Template_VideoContent] ADD [ContentLength] bigint NULL;
GO

ALTER TABLE [Chat_Message_Template_VideoContent] ADD [ContentType] nvarchar(100) NULL;
GO

ALTER TABLE [Chat_Message_Template_VideoContent] ADD [Suffix] nvarchar(10) NULL;
GO

ALTER TABLE [Chat_Message_Template_SoundContent] ADD [BlobId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_Message_Template_SoundContent] ADD [ContentLength] bigint NULL;
GO

ALTER TABLE [Chat_Message_Template_SoundContent] ADD [ContentType] nvarchar(100) NULL;
GO

ALTER TABLE [Chat_Message_Template_SoundContent] ADD [Suffix] nvarchar(10) NULL;
GO

ALTER TABLE [Chat_Message_Template_ImageContent] ADD [BlobId] uniqueidentifier NULL;
GO

ALTER TABLE [Chat_Message_Template_ImageContent] ADD [ContentLength] bigint NULL;
GO

ALTER TABLE [Chat_Message_Template_ImageContent] ADD [ContentType] nvarchar(100) NULL;
GO

ALTER TABLE [Chat_Message_Template_ImageContent] ADD [Suffix] nvarchar(10) NULL;
GO

ALTER TABLE [Chat_Message_Template_FileContent] ADD [BlobId] uniqueidentifier NULL;
GO

CREATE INDEX [IX_Chat_Message_Template_VideoContent_BlobId] ON [Chat_Message_Template_VideoContent] ([BlobId]);
GO

CREATE INDEX [IX_Chat_Message_Template_SoundContent_BlobId] ON [Chat_Message_Template_SoundContent] ([BlobId]);
GO

CREATE INDEX [IX_Chat_Message_Template_ImageContent_BlobId] ON [Chat_Message_Template_ImageContent] ([BlobId]);
GO

CREATE INDEX [IX_Chat_Message_Template_FileContent_BlobId] ON [Chat_Message_Template_FileContent] ([BlobId]);
GO

ALTER TABLE [Chat_Message_Template_FileContent] ADD CONSTRAINT [FK_Chat_Message_Template_FileContent_Chat_Blob_BlobId] FOREIGN KEY ([BlobId]) REFERENCES [Chat_Blob] ([Id]);
GO

ALTER TABLE [Chat_Message_Template_ImageContent] ADD CONSTRAINT [FK_Chat_Message_Template_ImageContent_Chat_Blob_BlobId] FOREIGN KEY ([BlobId]) REFERENCES [Chat_Blob] ([Id]);
GO

ALTER TABLE [Chat_Message_Template_SoundContent] ADD CONSTRAINT [FK_Chat_Message_Template_SoundContent_Chat_Blob_BlobId] FOREIGN KEY ([BlobId]) REFERENCES [Chat_Blob] ([Id]);
GO

ALTER TABLE [Chat_Message_Template_VideoContent] ADD CONSTRAINT [FK_Chat_Message_Template_VideoContent_Chat_Blob_BlobId] FOREIGN KEY ([BlobId]) REFERENCES [Chat_Blob] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240227095913_MessageContentBase_Alert', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var143 sysname;
SELECT @var143 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_VideoContent]') AND [c].[name] = N'ContentLength');
IF @var143 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_VideoContent] DROP CONSTRAINT [' + @var143 + '];');
ALTER TABLE [Chat_Message_Template_VideoContent] DROP COLUMN [ContentLength];
GO

DECLARE @var144 sysname;
SELECT @var144 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_SoundContent]') AND [c].[name] = N'ContentLength');
IF @var144 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_SoundContent] DROP CONSTRAINT [' + @var144 + '];');
ALTER TABLE [Chat_Message_Template_SoundContent] DROP COLUMN [ContentLength];
GO

DECLARE @var145 sysname;
SELECT @var145 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ImageContent]') AND [c].[name] = N'ContentLength');
IF @var145 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ImageContent] DROP CONSTRAINT [' + @var145 + '];');
ALTER TABLE [Chat_Message_Template_ImageContent] DROP COLUMN [ContentLength];
GO

EXEC sp_rename N'[Chat_Message_Template_FileContent].[ContentLength]', N'Size', N'COLUMN';
GO

DECLARE @var146 sysname;
SELECT @var146 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_VideoContent]') AND [c].[name] = N'Size');
IF @var146 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_VideoContent] DROP CONSTRAINT [' + @var146 + '];');
ALTER TABLE [Chat_Message_Template_VideoContent] ALTER COLUMN [Size] bigint NULL;
GO

DECLARE @var147 sysname;
SELECT @var147 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ImageContent]') AND [c].[name] = N'Size');
IF @var147 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ImageContent] DROP CONSTRAINT [' + @var147 + '];');
ALTER TABLE [Chat_Message_Template_ImageContent] ALTER COLUMN [Size] bigint NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240227100823_MessageContentBase_Size', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_SessionRole] ADD [PermissionCount] int NOT NULL DEFAULT 0;
GO

CREATE INDEX [IX_Chat_SessionRole_PermissionCount] ON [Chat_SessionRole] ([PermissionCount] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240229021649_SessionRole_Add_PermissionCount', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_ChatObject] ADD [Thumbnail] nvarchar(1000) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240301064626_ChatObject_Add_Thumbnail', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var148 sysname;
SELECT @var148 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_VideoContent]') AND [c].[name] = N'Url');
IF @var148 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_VideoContent] DROP CONSTRAINT [' + @var148 + '];');
ALTER TABLE [Chat_Message_Template_VideoContent] ALTER COLUMN [Url] nvarchar(500) NOT NULL;
GO

DECLARE @var149 sysname;
SELECT @var149 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_VideoContent]') AND [c].[name] = N'ImageUrl');
IF @var149 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_VideoContent] DROP CONSTRAINT [' + @var149 + '];');
ALTER TABLE [Chat_Message_Template_VideoContent] ALTER COLUMN [ImageUrl] nvarchar(500) NULL;
GO

ALTER TABLE [Chat_Message_Template_VideoContent] ADD [Description] nvarchar(500) NULL;
GO

ALTER TABLE [Chat_Message_Template_VideoContent] ADD [GifUrl] nvarchar(500) NULL;
GO

ALTER TABLE [Chat_Message_Template_SoundContent] ADD [Description] nvarchar(500) NULL;
GO

ALTER TABLE [Chat_Message_Template_ImageContent] ADD [Description] nvarchar(500) NULL;
GO

ALTER TABLE [Chat_Message_Template_FileContent] ADD [Description] nvarchar(500) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240308073949_MessageContentAttachments_Add_Description', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Message_Template_ImageContent] ADD [Profile] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240315022504_ImageConetnt_Add_Profile', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Chat_Message_Template_VideoContent] ADD [FileName] nvarchar(256) NULL;
GO

ALTER TABLE [Chat_Message_Template_SoundContent] ADD [FileName] nvarchar(256) NULL;
GO

ALTER TABLE [Chat_Message_Template_ImageContent] ADD [FileName] nvarchar(256) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240318014231_MessageTempConetnt_Add_FileName', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

EXEC sp_rename N'[Chat_Message_Template_VideoContent].[ImageUrl]', N'SnapshotUrl', N'COLUMN';
GO

ALTER TABLE [Chat_Message_Template_VideoContent] ADD [SnapshotThumbnailUrl] nvarchar(500) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240318092554_VideoContent_Add_SnapshotThumbnailUrl', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var150 sysname;
SELECT @var150 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionRequest]') AND [c].[name] = N'DeviceId');
IF @var150 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionRequest] DROP CONSTRAINT [' + @var150 + '];');
ALTER TABLE [Chat_SessionRequest] ALTER COLUMN [DeviceId] nvarchar(128) NULL;
GO

DECLARE @var151 sysname;
SELECT @var151 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'DeviceId');
IF @var151 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var151 + '];');
ALTER TABLE [Chat_ReadedRecorder] ALTER COLUMN [DeviceId] nvarchar(128) NULL;
GO

DECLARE @var152 sysname;
SELECT @var152 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_OpenedRecorder]') AND [c].[name] = N'DeviceId');
IF @var152 IS NOT NULL EXEC(N'ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [' + @var152 + '];');
ALTER TABLE [Chat_OpenedRecorder] ALTER COLUMN [DeviceId] nvarchar(128) NULL;
GO

DECLARE @var153 sysname;
SELECT @var153 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_FavoritedRecorder]') AND [c].[name] = N'DeviceId');
IF @var153 IS NOT NULL EXEC(N'ALTER TABLE [Chat_FavoritedRecorder] DROP CONSTRAINT [' + @var153 + '];');
ALTER TABLE [Chat_FavoritedRecorder] ALTER COLUMN [DeviceId] nvarchar(128) NULL;
GO

DECLARE @var154 sysname;
SELECT @var154 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Connection]') AND [c].[name] = N'DeviceId');
IF @var154 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Connection] DROP CONSTRAINT [' + @var154 + '];');
ALTER TABLE [Chat_Connection] ALTER COLUMN [DeviceId] nvarchar(128) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240326005204_DeviceId_MaxLength_128', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_ClientConfig] (
    [Id] uniqueidentifier NOT NULL,
    [OwnerId] bigint NULL,
    [DeviceId] nvarchar(128) NULL,
    [AppUserId] uniqueidentifier NULL,
    [Platform] nvarchar(64) NULL,
    [Title] nvarchar(64) NULL,
    [Description] nvarchar(512) NULL,
    [DataType] nvarchar(64) NULL,
    [Key] nvarchar(128) NULL,
    [Value] nvarchar(max) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    CONSTRAINT [PK_Chat_ClientConfig] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_ClientConfig_Chat_ChatObject_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Chat_ChatObject] ([Id])
);
GO

CREATE INDEX [IX_Chat_ClientConfig_Description] ON [Chat_ClientConfig] ([Description]);
GO

CREATE INDEX [IX_Chat_ClientConfig_DeviceId_AppUserId_Key_Platform] ON [Chat_ClientConfig] ([DeviceId] DESC, [AppUserId] DESC, [Key] DESC, [Platform] DESC);
GO

CREATE INDEX [IX_Chat_ClientConfig_OwnerId] ON [Chat_ClientConfig] ([OwnerId]);
GO

CREATE INDEX [IX_Chat_ClientConfig_Platform] ON [Chat_ClientConfig] ([Platform]);
GO

CREATE INDEX [IX_Chat_ClientConfig_Title] ON [Chat_ClientConfig] ([Title]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240424035734_ClientConfig_Init', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [Chat_Connection];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240507073427_Connection_Drop', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Chat_ServerHost] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(128) NULL,
    [ActiveTime] datetime2 NULL,
    [IsEnabled] bit NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    CONSTRAINT [PK_Chat_ServerHost] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Chat_Connection] (
    [Id] nvarchar(64) NOT NULL,
    [ServerHostId] nvarchar(450) NULL,
    [AppUserId] uniqueidentifier NULL,
    [ChatObjects] nvarchar(1000) NULL,
    [DeviceId] nvarchar(128) NULL,
    [IpAddress] nvarchar(36) NULL,
    [BrowserInfo] nvarchar(300) NULL,
    [ActiveTime] datetime2 NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    CONSTRAINT [PK_Chat_Connection] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Chat_Connection_Chat_ServerHost_ServerHostId] FOREIGN KEY ([ServerHostId]) REFERENCES [Chat_ServerHost] ([Id])
);
GO

CREATE TABLE [Chat_ConnectionChatObject] (
    [ConnectionId] nvarchar(64) NOT NULL,
    [ChatObjectId] bigint NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(40) NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    CONSTRAINT [PK_Chat_ConnectionChatObject] PRIMARY KEY ([ChatObjectId], [ConnectionId]),
    CONSTRAINT [FK_Chat_ConnectionChatObject_Chat_ChatObject_ChatObjectId] FOREIGN KEY ([ChatObjectId]) REFERENCES [Chat_ChatObject] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Chat_ConnectionChatObject_Chat_Connection_ConnectionId] FOREIGN KEY ([ConnectionId]) REFERENCES [Chat_Connection] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Chat_Connection_ActiveTime] ON [Chat_Connection] ([ActiveTime] DESC);
GO

CREATE INDEX [IX_Chat_Connection_ChatObjects] ON [Chat_Connection] ([ChatObjects]);
GO

CREATE INDEX [IX_Chat_Connection_DeviceId] ON [Chat_Connection] ([DeviceId]);
GO

CREATE INDEX [IX_Chat_Connection_IpAddress] ON [Chat_Connection] ([IpAddress]);
GO

CREATE INDEX [IX_Chat_Connection_ServerHostId] ON [Chat_Connection] ([ServerHostId]);
GO

CREATE INDEX [IX_Chat_ConnectionChatObject_ConnectionId] ON [Chat_ConnectionChatObject] ([ConnectionId]);
GO

CREATE INDEX [IX_Chat_ServerHost_ActiveTime] ON [Chat_ServerHost] ([ActiveTime] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240507080309_Connection_Reconstruction', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE INDEX [IX_Chat_ServerHost_CreationTime] ON [Chat_ServerHost] ([CreationTime] DESC);
GO

CREATE INDEX [IX_Chat_Connection_CreationTime] ON [Chat_Connection] ([CreationTime] DESC);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240507081716_Connection_AddIndex_CreationTime', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var155 sysname;
SELECT @var155 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Word]') AND [c].[name] = N'ExtraProperties');
IF @var155 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Word] DROP CONSTRAINT [' + @var155 + '];');
UPDATE [Chat_Word] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Word] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Word] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var156 sysname;
SELECT @var156 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Word]') AND [c].[name] = N'ConcurrencyStamp');
IF @var156 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Word] DROP CONSTRAINT [' + @var156 + '];');
UPDATE [Chat_Word] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Word] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Word] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var157 sysname;
SELECT @var157 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletRequest]') AND [c].[name] = N'ExtraProperties');
IF @var157 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletRequest] DROP CONSTRAINT [' + @var157 + '];');
UPDATE [Chat_WalletRequest] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_WalletRequest] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_WalletRequest] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var158 sysname;
SELECT @var158 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletRequest]') AND [c].[name] = N'ConcurrencyStamp');
IF @var158 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletRequest] DROP CONSTRAINT [' + @var158 + '];');
UPDATE [Chat_WalletRequest] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_WalletRequest] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_WalletRequest] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var159 sysname;
SELECT @var159 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletRecorder]') AND [c].[name] = N'ExtraProperties');
IF @var159 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletRecorder] DROP CONSTRAINT [' + @var159 + '];');
UPDATE [Chat_WalletRecorder] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_WalletRecorder] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_WalletRecorder] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var160 sysname;
SELECT @var160 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletRecorder]') AND [c].[name] = N'ConcurrencyStamp');
IF @var160 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletRecorder] DROP CONSTRAINT [' + @var160 + '];');
UPDATE [Chat_WalletRecorder] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_WalletRecorder] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_WalletRecorder] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var161 sysname;
SELECT @var161 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletOrder]') AND [c].[name] = N'ExtraProperties');
IF @var161 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletOrder] DROP CONSTRAINT [' + @var161 + '];');
UPDATE [Chat_WalletOrder] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_WalletOrder] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_WalletOrder] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var162 sysname;
SELECT @var162 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletOrder]') AND [c].[name] = N'ConcurrencyStamp');
IF @var162 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletOrder] DROP CONSTRAINT [' + @var162 + '];');
UPDATE [Chat_WalletOrder] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_WalletOrder] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_WalletOrder] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var163 sysname;
SELECT @var163 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletBusiness]') AND [c].[name] = N'ExtraProperties');
IF @var163 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletBusiness] DROP CONSTRAINT [' + @var163 + '];');
UPDATE [Chat_WalletBusiness] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_WalletBusiness] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_WalletBusiness] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var164 sysname;
SELECT @var164 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_WalletBusiness]') AND [c].[name] = N'ConcurrencyStamp');
IF @var164 IS NOT NULL EXEC(N'ALTER TABLE [Chat_WalletBusiness] DROP CONSTRAINT [' + @var164 + '];');
UPDATE [Chat_WalletBusiness] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_WalletBusiness] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_WalletBusiness] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var165 sysname;
SELECT @var165 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Wallet]') AND [c].[name] = N'ExtraProperties');
IF @var165 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Wallet] DROP CONSTRAINT [' + @var165 + '];');
UPDATE [Chat_Wallet] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Wallet] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Wallet] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var166 sysname;
SELECT @var166 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Wallet]') AND [c].[name] = N'ConcurrencyStamp');
IF @var166 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Wallet] DROP CONSTRAINT [' + @var166 + '];');
UPDATE [Chat_Wallet] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Wallet] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Wallet] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var167 sysname;
SELECT @var167 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_TextContentWord]') AND [c].[name] = N'ExtraProperties');
IF @var167 IS NOT NULL EXEC(N'ALTER TABLE [Chat_TextContentWord] DROP CONSTRAINT [' + @var167 + '];');
UPDATE [Chat_TextContentWord] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_TextContentWord] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_TextContentWord] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var168 sysname;
SELECT @var168 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_TextContentWord]') AND [c].[name] = N'ConcurrencyStamp');
IF @var168 IS NOT NULL EXEC(N'ALTER TABLE [Chat_TextContentWord] DROP CONSTRAINT [' + @var168 + '];');
UPDATE [Chat_TextContentWord] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_TextContentWord] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_TextContentWord] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var169 sysname;
SELECT @var169 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitTag]') AND [c].[name] = N'ExtraProperties');
IF @var169 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitTag] DROP CONSTRAINT [' + @var169 + '];');
UPDATE [Chat_SessionUnitTag] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitTag] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitTag] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var170 sysname;
SELECT @var170 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitTag]') AND [c].[name] = N'ConcurrencyStamp');
IF @var170 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitTag] DROP CONSTRAINT [' + @var170 + '];');
UPDATE [Chat_SessionUnitTag] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitTag] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitTag] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var171 sysname;
SELECT @var171 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitSetting]') AND [c].[name] = N'ExtraProperties');
IF @var171 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitSetting] DROP CONSTRAINT [' + @var171 + '];');
UPDATE [Chat_SessionUnitSetting] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitSetting] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitSetting] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var172 sysname;
SELECT @var172 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitSetting]') AND [c].[name] = N'ConcurrencyStamp');
IF @var172 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitSetting] DROP CONSTRAINT [' + @var172 + '];');
UPDATE [Chat_SessionUnitSetting] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitSetting] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitSetting] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var173 sysname;
SELECT @var173 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitRole]') AND [c].[name] = N'ExtraProperties');
IF @var173 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitRole] DROP CONSTRAINT [' + @var173 + '];');
UPDATE [Chat_SessionUnitRole] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitRole] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitRole] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var174 sysname;
SELECT @var174 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitRole]') AND [c].[name] = N'ConcurrencyStamp');
IF @var174 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitRole] DROP CONSTRAINT [' + @var174 + '];');
UPDATE [Chat_SessionUnitRole] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitRole] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitRole] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var175 sysname;
SELECT @var175 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitOrganization]') AND [c].[name] = N'ExtraProperties');
IF @var175 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitOrganization] DROP CONSTRAINT [' + @var175 + '];');
UPDATE [Chat_SessionUnitOrganization] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitOrganization] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitOrganization] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var176 sysname;
SELECT @var176 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitOrganization]') AND [c].[name] = N'ConcurrencyStamp');
IF @var176 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitOrganization] DROP CONSTRAINT [' + @var176 + '];');
UPDATE [Chat_SessionUnitOrganization] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitOrganization] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitOrganization] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var177 sysname;
SELECT @var177 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitEntryValue]') AND [c].[name] = N'ExtraProperties');
IF @var177 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitEntryValue] DROP CONSTRAINT [' + @var177 + '];');
UPDATE [Chat_SessionUnitEntryValue] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitEntryValue] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitEntryValue] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var178 sysname;
SELECT @var178 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitEntryValue]') AND [c].[name] = N'ConcurrencyStamp');
IF @var178 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitEntryValue] DROP CONSTRAINT [' + @var178 + '];');
UPDATE [Chat_SessionUnitEntryValue] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitEntryValue] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitEntryValue] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var179 sysname;
SELECT @var179 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitCounter]') AND [c].[name] = N'ExtraProperties');
IF @var179 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitCounter] DROP CONSTRAINT [' + @var179 + '];');
UPDATE [Chat_SessionUnitCounter] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitCounter] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitCounter] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var180 sysname;
SELECT @var180 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitCounter]') AND [c].[name] = N'ConcurrencyStamp');
IF @var180 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitCounter] DROP CONSTRAINT [' + @var180 + '];');
UPDATE [Chat_SessionUnitCounter] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitCounter] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitCounter] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var181 sysname;
SELECT @var181 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitContactTag]') AND [c].[name] = N'ExtraProperties');
IF @var181 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitContactTag] DROP CONSTRAINT [' + @var181 + '];');
UPDATE [Chat_SessionUnitContactTag] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnitContactTag] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnitContactTag] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var182 sysname;
SELECT @var182 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnitContactTag]') AND [c].[name] = N'ConcurrencyStamp');
IF @var182 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnitContactTag] DROP CONSTRAINT [' + @var182 + '];');
UPDATE [Chat_SessionUnitContactTag] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnitContactTag] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnitContactTag] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var183 sysname;
SELECT @var183 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'ExtraProperties');
IF @var183 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var183 + '];');
UPDATE [Chat_SessionUnit] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionUnit] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionUnit] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var184 sysname;
SELECT @var184 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionUnit]') AND [c].[name] = N'ConcurrencyStamp');
IF @var184 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionUnit] DROP CONSTRAINT [' + @var184 + '];');
UPDATE [Chat_SessionUnit] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionUnit] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionUnit] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var185 sysname;
SELECT @var185 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionTag]') AND [c].[name] = N'ExtraProperties');
IF @var185 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionTag] DROP CONSTRAINT [' + @var185 + '];');
UPDATE [Chat_SessionTag] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionTag] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionTag] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var186 sysname;
SELECT @var186 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionTag]') AND [c].[name] = N'ConcurrencyStamp');
IF @var186 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionTag] DROP CONSTRAINT [' + @var186 + '];');
UPDATE [Chat_SessionTag] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionTag] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionTag] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var187 sysname;
SELECT @var187 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionRole]') AND [c].[name] = N'ExtraProperties');
IF @var187 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionRole] DROP CONSTRAINT [' + @var187 + '];');
UPDATE [Chat_SessionRole] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionRole] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionRole] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var188 sysname;
SELECT @var188 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionRole]') AND [c].[name] = N'ConcurrencyStamp');
IF @var188 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionRole] DROP CONSTRAINT [' + @var188 + '];');
UPDATE [Chat_SessionRole] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionRole] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionRole] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var189 sysname;
SELECT @var189 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionRequest]') AND [c].[name] = N'ExtraProperties');
IF @var189 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionRequest] DROP CONSTRAINT [' + @var189 + '];');
UPDATE [Chat_SessionRequest] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionRequest] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionRequest] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var190 sysname;
SELECT @var190 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionRequest]') AND [c].[name] = N'ConcurrencyStamp');
IF @var190 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionRequest] DROP CONSTRAINT [' + @var190 + '];');
UPDATE [Chat_SessionRequest] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionRequest] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionRequest] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var191 sysname;
SELECT @var191 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionUnitGrant]') AND [c].[name] = N'ExtraProperties');
IF @var191 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionUnitGrant] DROP CONSTRAINT [' + @var191 + '];');
UPDATE [Chat_SessionPermissionUnitGrant] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionPermissionUnitGrant] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionPermissionUnitGrant] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var192 sysname;
SELECT @var192 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionUnitGrant]') AND [c].[name] = N'ConcurrencyStamp');
IF @var192 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionUnitGrant] DROP CONSTRAINT [' + @var192 + '];');
UPDATE [Chat_SessionPermissionUnitGrant] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionPermissionUnitGrant] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionPermissionUnitGrant] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var193 sysname;
SELECT @var193 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionRoleGrant]') AND [c].[name] = N'ExtraProperties');
IF @var193 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionRoleGrant] DROP CONSTRAINT [' + @var193 + '];');
UPDATE [Chat_SessionPermissionRoleGrant] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionPermissionRoleGrant] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionPermissionRoleGrant] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var194 sysname;
SELECT @var194 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionRoleGrant]') AND [c].[name] = N'ConcurrencyStamp');
IF @var194 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionRoleGrant] DROP CONSTRAINT [' + @var194 + '];');
UPDATE [Chat_SessionPermissionRoleGrant] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionPermissionRoleGrant] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionPermissionRoleGrant] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var195 sysname;
SELECT @var195 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionGroup]') AND [c].[name] = N'ExtraProperties');
IF @var195 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionGroup] DROP CONSTRAINT [' + @var195 + '];');
UPDATE [Chat_SessionPermissionGroup] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionPermissionGroup] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionPermissionGroup] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var196 sysname;
SELECT @var196 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionGroup]') AND [c].[name] = N'ConcurrencyStamp');
IF @var196 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionGroup] DROP CONSTRAINT [' + @var196 + '];');
UPDATE [Chat_SessionPermissionGroup] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionPermissionGroup] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionPermissionGroup] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var197 sysname;
SELECT @var197 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionDefinition]') AND [c].[name] = N'ExtraProperties');
IF @var197 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionDefinition] DROP CONSTRAINT [' + @var197 + '];');
UPDATE [Chat_SessionPermissionDefinition] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionPermissionDefinition] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionPermissionDefinition] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var198 sysname;
SELECT @var198 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionPermissionDefinition]') AND [c].[name] = N'ConcurrencyStamp');
IF @var198 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionPermissionDefinition] DROP CONSTRAINT [' + @var198 + '];');
UPDATE [Chat_SessionPermissionDefinition] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionPermissionDefinition] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionPermissionDefinition] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var199 sysname;
SELECT @var199 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionOrganization]') AND [c].[name] = N'ExtraProperties');
IF @var199 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionOrganization] DROP CONSTRAINT [' + @var199 + '];');
UPDATE [Chat_SessionOrganization] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_SessionOrganization] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_SessionOrganization] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var200 sysname;
SELECT @var200 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_SessionOrganization]') AND [c].[name] = N'ConcurrencyStamp');
IF @var200 IS NOT NULL EXEC(N'ALTER TABLE [Chat_SessionOrganization] DROP CONSTRAINT [' + @var200 + '];');
UPDATE [Chat_SessionOrganization] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_SessionOrganization] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_SessionOrganization] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var201 sysname;
SELECT @var201 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Session]') AND [c].[name] = N'ExtraProperties');
IF @var201 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Session] DROP CONSTRAINT [' + @var201 + '];');
UPDATE [Chat_Session] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Session] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Session] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var202 sysname;
SELECT @var202 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Session]') AND [c].[name] = N'ConcurrencyStamp');
IF @var202 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Session] DROP CONSTRAINT [' + @var202 + '];');
UPDATE [Chat_Session] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Session] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Session] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var203 sysname;
SELECT @var203 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ServerHost]') AND [c].[name] = N'ExtraProperties');
IF @var203 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ServerHost] DROP CONSTRAINT [' + @var203 + '];');
UPDATE [Chat_ServerHost] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ServerHost] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ServerHost] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var204 sysname;
SELECT @var204 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ServerHost]') AND [c].[name] = N'ConcurrencyStamp');
IF @var204 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ServerHost] DROP CONSTRAINT [' + @var204 + '];');
UPDATE [Chat_ServerHost] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ServerHost] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ServerHost] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var205 sysname;
SELECT @var205 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Scoped]') AND [c].[name] = N'ExtraProperties');
IF @var205 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Scoped] DROP CONSTRAINT [' + @var205 + '];');
UPDATE [Chat_Scoped] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Scoped] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Scoped] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var206 sysname;
SELECT @var206 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Scoped]') AND [c].[name] = N'ConcurrencyStamp');
IF @var206 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Scoped] DROP CONSTRAINT [' + @var206 + '];');
UPDATE [Chat_Scoped] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Scoped] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Scoped] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var207 sysname;
SELECT @var207 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_RedEnvelopeUnit]') AND [c].[name] = N'ExtraProperties');
IF @var207 IS NOT NULL EXEC(N'ALTER TABLE [Chat_RedEnvelopeUnit] DROP CONSTRAINT [' + @var207 + '];');
UPDATE [Chat_RedEnvelopeUnit] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_RedEnvelopeUnit] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_RedEnvelopeUnit] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var208 sysname;
SELECT @var208 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_RedEnvelopeUnit]') AND [c].[name] = N'ConcurrencyStamp');
IF @var208 IS NOT NULL EXEC(N'ALTER TABLE [Chat_RedEnvelopeUnit] DROP CONSTRAINT [' + @var208 + '];');
UPDATE [Chat_RedEnvelopeUnit] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_RedEnvelopeUnit] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_RedEnvelopeUnit] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var209 sysname;
SELECT @var209 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'ExtraProperties');
IF @var209 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var209 + '];');
UPDATE [Chat_ReadedRecorder] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ReadedRecorder] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ReadedRecorder] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var210 sysname;
SELECT @var210 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ReadedRecorder]') AND [c].[name] = N'ConcurrencyStamp');
IF @var210 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ReadedRecorder] DROP CONSTRAINT [' + @var210 + '];');
UPDATE [Chat_ReadedRecorder] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ReadedRecorder] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ReadedRecorder] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var211 sysname;
SELECT @var211 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_PaymentPlatform]') AND [c].[name] = N'ExtraProperties');
IF @var211 IS NOT NULL EXEC(N'ALTER TABLE [Chat_PaymentPlatform] DROP CONSTRAINT [' + @var211 + '];');
UPDATE [Chat_PaymentPlatform] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_PaymentPlatform] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_PaymentPlatform] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var212 sysname;
SELECT @var212 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_PaymentPlatform]') AND [c].[name] = N'ConcurrencyStamp');
IF @var212 IS NOT NULL EXEC(N'ALTER TABLE [Chat_PaymentPlatform] DROP CONSTRAINT [' + @var212 + '];');
UPDATE [Chat_PaymentPlatform] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_PaymentPlatform] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_PaymentPlatform] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var213 sysname;
SELECT @var213 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_OpenedRecorder]') AND [c].[name] = N'ExtraProperties');
IF @var213 IS NOT NULL EXEC(N'ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [' + @var213 + '];');
UPDATE [Chat_OpenedRecorder] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_OpenedRecorder] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_OpenedRecorder] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var214 sysname;
SELECT @var214 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_OpenedRecorder]') AND [c].[name] = N'ConcurrencyStamp');
IF @var214 IS NOT NULL EXEC(N'ALTER TABLE [Chat_OpenedRecorder] DROP CONSTRAINT [' + @var214 + '];');
UPDATE [Chat_OpenedRecorder] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_OpenedRecorder] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_OpenedRecorder] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var215 sysname;
SELECT @var215 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Motto]') AND [c].[name] = N'ExtraProperties');
IF @var215 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Motto] DROP CONSTRAINT [' + @var215 + '];');
UPDATE [Chat_Motto] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Motto] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Motto] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var216 sysname;
SELECT @var216 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Motto]') AND [c].[name] = N'ConcurrencyStamp');
IF @var216 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Motto] DROP CONSTRAINT [' + @var216 + '];');
UPDATE [Chat_Motto] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Motto] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Motto] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var217 sysname;
SELECT @var217 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageWord]') AND [c].[name] = N'ExtraProperties');
IF @var217 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageWord] DROP CONSTRAINT [' + @var217 + '];');
UPDATE [Chat_MessageWord] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_MessageWord] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_MessageWord] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var218 sysname;
SELECT @var218 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageWord]') AND [c].[name] = N'ConcurrencyStamp');
IF @var218 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageWord] DROP CONSTRAINT [' + @var218 + '];');
UPDATE [Chat_MessageWord] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_MessageWord] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_MessageWord] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var219 sysname;
SELECT @var219 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageReminder]') AND [c].[name] = N'ExtraProperties');
IF @var219 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageReminder] DROP CONSTRAINT [' + @var219 + '];');
UPDATE [Chat_MessageReminder] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_MessageReminder] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_MessageReminder] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var220 sysname;
SELECT @var220 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageReminder]') AND [c].[name] = N'ConcurrencyStamp');
IF @var220 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageReminder] DROP CONSTRAINT [' + @var220 + '];');
UPDATE [Chat_MessageReminder] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_MessageReminder] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_MessageReminder] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var221 sysname;
SELECT @var221 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageContent]') AND [c].[name] = N'ExtraProperties');
IF @var221 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageContent] DROP CONSTRAINT [' + @var221 + '];');
UPDATE [Chat_MessageContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_MessageContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_MessageContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var222 sysname;
SELECT @var222 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_MessageContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var222 IS NOT NULL EXEC(N'ALTER TABLE [Chat_MessageContent] DROP CONSTRAINT [' + @var222 + '];');
UPDATE [Chat_MessageContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_MessageContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_MessageContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var223 sysname;
SELECT @var223 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_VideoContent]') AND [c].[name] = N'ExtraProperties');
IF @var223 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_VideoContent] DROP CONSTRAINT [' + @var223 + '];');
UPDATE [Chat_Message_Template_VideoContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_VideoContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_VideoContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var224 sysname;
SELECT @var224 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_VideoContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var224 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_VideoContent] DROP CONSTRAINT [' + @var224 + '];');
UPDATE [Chat_Message_Template_VideoContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_VideoContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_VideoContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var225 sysname;
SELECT @var225 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_TextContent]') AND [c].[name] = N'ExtraProperties');
IF @var225 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_TextContent] DROP CONSTRAINT [' + @var225 + '];');
UPDATE [Chat_Message_Template_TextContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_TextContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_TextContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var226 sysname;
SELECT @var226 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_TextContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var226 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_TextContent] DROP CONSTRAINT [' + @var226 + '];');
UPDATE [Chat_Message_Template_TextContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_TextContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_TextContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var227 sysname;
SELECT @var227 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_SoundContent]') AND [c].[name] = N'ExtraProperties');
IF @var227 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_SoundContent] DROP CONSTRAINT [' + @var227 + '];');
UPDATE [Chat_Message_Template_SoundContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_SoundContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_SoundContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var228 sysname;
SELECT @var228 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_SoundContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var228 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_SoundContent] DROP CONSTRAINT [' + @var228 + '];');
UPDATE [Chat_Message_Template_SoundContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_SoundContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_SoundContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var229 sysname;
SELECT @var229 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_RedEnvelopeContent]') AND [c].[name] = N'ExtraProperties');
IF @var229 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] DROP CONSTRAINT [' + @var229 + '];');
UPDATE [Chat_Message_Template_RedEnvelopeContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var230 sysname;
SELECT @var230 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_RedEnvelopeContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var230 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] DROP CONSTRAINT [' + @var230 + '];');
UPDATE [Chat_Message_Template_RedEnvelopeContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_RedEnvelopeContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var231 sysname;
SELECT @var231 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_LocationContent]') AND [c].[name] = N'ExtraProperties');
IF @var231 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_LocationContent] DROP CONSTRAINT [' + @var231 + '];');
UPDATE [Chat_Message_Template_LocationContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_LocationContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_LocationContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var232 sysname;
SELECT @var232 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_LocationContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var232 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_LocationContent] DROP CONSTRAINT [' + @var232 + '];');
UPDATE [Chat_Message_Template_LocationContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_LocationContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_LocationContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var233 sysname;
SELECT @var233 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_LinkContent]') AND [c].[name] = N'ExtraProperties');
IF @var233 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_LinkContent] DROP CONSTRAINT [' + @var233 + '];');
UPDATE [Chat_Message_Template_LinkContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_LinkContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_LinkContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var234 sysname;
SELECT @var234 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_LinkContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var234 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_LinkContent] DROP CONSTRAINT [' + @var234 + '];');
UPDATE [Chat_Message_Template_LinkContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_LinkContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_LinkContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var235 sysname;
SELECT @var235 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ImageContent]') AND [c].[name] = N'ExtraProperties');
IF @var235 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ImageContent] DROP CONSTRAINT [' + @var235 + '];');
UPDATE [Chat_Message_Template_ImageContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_ImageContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_ImageContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var236 sysname;
SELECT @var236 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ImageContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var236 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ImageContent] DROP CONSTRAINT [' + @var236 + '];');
UPDATE [Chat_Message_Template_ImageContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_ImageContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_ImageContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var237 sysname;
SELECT @var237 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_HtmlContent]') AND [c].[name] = N'ExtraProperties');
IF @var237 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_HtmlContent] DROP CONSTRAINT [' + @var237 + '];');
UPDATE [Chat_Message_Template_HtmlContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_HtmlContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_HtmlContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var238 sysname;
SELECT @var238 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_HtmlContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var238 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_HtmlContent] DROP CONSTRAINT [' + @var238 + '];');
UPDATE [Chat_Message_Template_HtmlContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_HtmlContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_HtmlContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var239 sysname;
SELECT @var239 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_HistoryContent]') AND [c].[name] = N'ExtraProperties');
IF @var239 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_HistoryContent] DROP CONSTRAINT [' + @var239 + '];');
UPDATE [Chat_Message_Template_HistoryContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_HistoryContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_HistoryContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var240 sysname;
SELECT @var240 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_HistoryContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var240 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_HistoryContent] DROP CONSTRAINT [' + @var240 + '];');
UPDATE [Chat_Message_Template_HistoryContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_HistoryContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_HistoryContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var241 sysname;
SELECT @var241 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_FileContent]') AND [c].[name] = N'ExtraProperties');
IF @var241 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_FileContent] DROP CONSTRAINT [' + @var241 + '];');
UPDATE [Chat_Message_Template_FileContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_FileContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_FileContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var242 sysname;
SELECT @var242 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_FileContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var242 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_FileContent] DROP CONSTRAINT [' + @var242 + '];');
UPDATE [Chat_Message_Template_FileContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_FileContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_FileContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var243 sysname;
SELECT @var243 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ContactsContent]') AND [c].[name] = N'ExtraProperties');
IF @var243 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ContactsContent] DROP CONSTRAINT [' + @var243 + '];');
UPDATE [Chat_Message_Template_ContactsContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_ContactsContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_ContactsContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var244 sysname;
SELECT @var244 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ContactsContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var244 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ContactsContent] DROP CONSTRAINT [' + @var244 + '];');
UPDATE [Chat_Message_Template_ContactsContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_ContactsContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_ContactsContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var245 sysname;
SELECT @var245 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_CmdContent]') AND [c].[name] = N'ExtraProperties');
IF @var245 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_CmdContent] DROP CONSTRAINT [' + @var245 + '];');
UPDATE [Chat_Message_Template_CmdContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_CmdContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_CmdContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var246 sysname;
SELECT @var246 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_CmdContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var246 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_CmdContent] DROP CONSTRAINT [' + @var246 + '];');
UPDATE [Chat_Message_Template_CmdContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_CmdContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_CmdContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var247 sysname;
SELECT @var247 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ArticleContent]') AND [c].[name] = N'ExtraProperties');
IF @var247 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ArticleContent] DROP CONSTRAINT [' + @var247 + '];');
UPDATE [Chat_Message_Template_ArticleContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message_Template_ArticleContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message_Template_ArticleContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var248 sysname;
SELECT @var248 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message_Template_ArticleContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var248 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message_Template_ArticleContent] DROP CONSTRAINT [' + @var248 + '];');
UPDATE [Chat_Message_Template_ArticleContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message_Template_ArticleContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message_Template_ArticleContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var249 sysname;
SELECT @var249 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message]') AND [c].[name] = N'ExtraProperties');
IF @var249 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message] DROP CONSTRAINT [' + @var249 + '];');
UPDATE [Chat_Message] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Message] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Message] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var250 sysname;
SELECT @var250 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Message]') AND [c].[name] = N'ConcurrencyStamp');
IF @var250 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Message] DROP CONSTRAINT [' + @var250 + '];');
UPDATE [Chat_Message] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Message] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Message] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var251 sysname;
SELECT @var251 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Menu]') AND [c].[name] = N'ExtraProperties');
IF @var251 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Menu] DROP CONSTRAINT [' + @var251 + '];');
UPDATE [Chat_Menu] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Menu] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Menu] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var252 sysname;
SELECT @var252 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Menu]') AND [c].[name] = N'ConcurrencyStamp');
IF @var252 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Menu] DROP CONSTRAINT [' + @var252 + '];');
UPDATE [Chat_Menu] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Menu] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Menu] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var253 sysname;
SELECT @var253 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_InvitationCode]') AND [c].[name] = N'ExtraProperties');
IF @var253 IS NOT NULL EXEC(N'ALTER TABLE [Chat_InvitationCode] DROP CONSTRAINT [' + @var253 + '];');
UPDATE [Chat_InvitationCode] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_InvitationCode] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_InvitationCode] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var254 sysname;
SELECT @var254 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_InvitationCode]') AND [c].[name] = N'ConcurrencyStamp');
IF @var254 IS NOT NULL EXEC(N'ALTER TABLE [Chat_InvitationCode] DROP CONSTRAINT [' + @var254 + '];');
UPDATE [Chat_InvitationCode] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_InvitationCode] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_InvitationCode] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var255 sysname;
SELECT @var255 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HttpResponse]') AND [c].[name] = N'ExtraProperties');
IF @var255 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HttpResponse] DROP CONSTRAINT [' + @var255 + '];');
UPDATE [Chat_HttpResponse] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_HttpResponse] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_HttpResponse] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var256 sysname;
SELECT @var256 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HttpResponse]') AND [c].[name] = N'ConcurrencyStamp');
IF @var256 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HttpResponse] DROP CONSTRAINT [' + @var256 + '];');
UPDATE [Chat_HttpResponse] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_HttpResponse] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_HttpResponse] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var257 sysname;
SELECT @var257 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HttpRequest]') AND [c].[name] = N'ExtraProperties');
IF @var257 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HttpRequest] DROP CONSTRAINT [' + @var257 + '];');
UPDATE [Chat_HttpRequest] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_HttpRequest] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_HttpRequest] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var258 sysname;
SELECT @var258 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HttpRequest]') AND [c].[name] = N'ConcurrencyStamp');
IF @var258 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HttpRequest] DROP CONSTRAINT [' + @var258 + '];');
UPDATE [Chat_HttpRequest] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_HttpRequest] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_HttpRequest] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var259 sysname;
SELECT @var259 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HistoryMessage]') AND [c].[name] = N'ExtraProperties');
IF @var259 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HistoryMessage] DROP CONSTRAINT [' + @var259 + '];');
UPDATE [Chat_HistoryMessage] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_HistoryMessage] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_HistoryMessage] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var260 sysname;
SELECT @var260 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_HistoryMessage]') AND [c].[name] = N'ConcurrencyStamp');
IF @var260 IS NOT NULL EXEC(N'ALTER TABLE [Chat_HistoryMessage] DROP CONSTRAINT [' + @var260 + '];');
UPDATE [Chat_HistoryMessage] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_HistoryMessage] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_HistoryMessage] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var261 sysname;
SELECT @var261 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Follow]') AND [c].[name] = N'ExtraProperties');
IF @var261 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Follow] DROP CONSTRAINT [' + @var261 + '];');
UPDATE [Chat_Follow] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Follow] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Follow] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var262 sysname;
SELECT @var262 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Follow]') AND [c].[name] = N'ConcurrencyStamp');
IF @var262 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Follow] DROP CONSTRAINT [' + @var262 + '];');
UPDATE [Chat_Follow] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Follow] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Follow] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var263 sysname;
SELECT @var263 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_FavoritedRecorder]') AND [c].[name] = N'ExtraProperties');
IF @var263 IS NOT NULL EXEC(N'ALTER TABLE [Chat_FavoritedRecorder] DROP CONSTRAINT [' + @var263 + '];');
UPDATE [Chat_FavoritedRecorder] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_FavoritedRecorder] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_FavoritedRecorder] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var264 sysname;
SELECT @var264 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_FavoritedRecorder]') AND [c].[name] = N'ConcurrencyStamp');
IF @var264 IS NOT NULL EXEC(N'ALTER TABLE [Chat_FavoritedRecorder] DROP CONSTRAINT [' + @var264 + '];');
UPDATE [Chat_FavoritedRecorder] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_FavoritedRecorder] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_FavoritedRecorder] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var265 sysname;
SELECT @var265 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryValue]') AND [c].[name] = N'ExtraProperties');
IF @var265 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryValue] DROP CONSTRAINT [' + @var265 + '];');
UPDATE [Chat_EntryValue] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_EntryValue] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_EntryValue] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var266 sysname;
SELECT @var266 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryValue]') AND [c].[name] = N'ConcurrencyStamp');
IF @var266 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryValue] DROP CONSTRAINT [' + @var266 + '];');
UPDATE [Chat_EntryValue] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_EntryValue] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_EntryValue] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var267 sysname;
SELECT @var267 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryName]') AND [c].[name] = N'ExtraProperties');
IF @var267 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryName] DROP CONSTRAINT [' + @var267 + '];');
UPDATE [Chat_EntryName] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_EntryName] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_EntryName] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var268 sysname;
SELECT @var268 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_EntryName]') AND [c].[name] = N'ConcurrencyStamp');
IF @var268 IS NOT NULL EXEC(N'ALTER TABLE [Chat_EntryName] DROP CONSTRAINT [' + @var268 + '];');
UPDATE [Chat_EntryName] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_EntryName] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_EntryName] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var269 sysname;
SELECT @var269 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Developer]') AND [c].[name] = N'ExtraProperties');
IF @var269 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Developer] DROP CONSTRAINT [' + @var269 + '];');
UPDATE [Chat_Developer] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Developer] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Developer] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var270 sysname;
SELECT @var270 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Developer]') AND [c].[name] = N'ConcurrencyStamp');
IF @var270 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Developer] DROP CONSTRAINT [' + @var270 + '];');
UPDATE [Chat_Developer] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Developer] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Developer] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var271 sysname;
SELECT @var271 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ContactTag]') AND [c].[name] = N'ExtraProperties');
IF @var271 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ContactTag] DROP CONSTRAINT [' + @var271 + '];');
UPDATE [Chat_ContactTag] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ContactTag] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ContactTag] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var272 sysname;
SELECT @var272 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ContactTag]') AND [c].[name] = N'ConcurrencyStamp');
IF @var272 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ContactTag] DROP CONSTRAINT [' + @var272 + '];');
UPDATE [Chat_ContactTag] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ContactTag] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ContactTag] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var273 sysname;
SELECT @var273 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ConnectionChatObject]') AND [c].[name] = N'ExtraProperties');
IF @var273 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ConnectionChatObject] DROP CONSTRAINT [' + @var273 + '];');
UPDATE [Chat_ConnectionChatObject] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ConnectionChatObject] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ConnectionChatObject] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var274 sysname;
SELECT @var274 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ConnectionChatObject]') AND [c].[name] = N'ConcurrencyStamp');
IF @var274 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ConnectionChatObject] DROP CONSTRAINT [' + @var274 + '];');
UPDATE [Chat_ConnectionChatObject] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ConnectionChatObject] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ConnectionChatObject] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var275 sysname;
SELECT @var275 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Connection]') AND [c].[name] = N'ExtraProperties');
IF @var275 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Connection] DROP CONSTRAINT [' + @var275 + '];');
UPDATE [Chat_Connection] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Connection] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Connection] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var276 sysname;
SELECT @var276 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Connection]') AND [c].[name] = N'ConcurrencyStamp');
IF @var276 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Connection] DROP CONSTRAINT [' + @var276 + '];');
UPDATE [Chat_Connection] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Connection] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Connection] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var277 sysname;
SELECT @var277 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ClientConfig]') AND [c].[name] = N'ExtraProperties');
IF @var277 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ClientConfig] DROP CONSTRAINT [' + @var277 + '];');
UPDATE [Chat_ClientConfig] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ClientConfig] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ClientConfig] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var278 sysname;
SELECT @var278 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ClientConfig]') AND [c].[name] = N'ConcurrencyStamp');
IF @var278 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ClientConfig] DROP CONSTRAINT [' + @var278 + '];');
UPDATE [Chat_ClientConfig] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ClientConfig] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ClientConfig] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var279 sysname;
SELECT @var279 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectType]') AND [c].[name] = N'ExtraProperties');
IF @var279 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectType] DROP CONSTRAINT [' + @var279 + '];');
UPDATE [Chat_ChatObjectType] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ChatObjectType] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ChatObjectType] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var280 sysname;
SELECT @var280 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectType]') AND [c].[name] = N'ConcurrencyStamp');
IF @var280 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectType] DROP CONSTRAINT [' + @var280 + '];');
UPDATE [Chat_ChatObjectType] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ChatObjectType] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ChatObjectType] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var281 sysname;
SELECT @var281 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectEntryValue]') AND [c].[name] = N'ExtraProperties');
IF @var281 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectEntryValue] DROP CONSTRAINT [' + @var281 + '];');
UPDATE [Chat_ChatObjectEntryValue] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ChatObjectEntryValue] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ChatObjectEntryValue] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var282 sysname;
SELECT @var282 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectEntryValue]') AND [c].[name] = N'ConcurrencyStamp');
IF @var282 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectEntryValue] DROP CONSTRAINT [' + @var282 + '];');
UPDATE [Chat_ChatObjectEntryValue] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ChatObjectEntryValue] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ChatObjectEntryValue] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var283 sysname;
SELECT @var283 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectCategoryUnit]') AND [c].[name] = N'ExtraProperties');
IF @var283 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectCategoryUnit] DROP CONSTRAINT [' + @var283 + '];');
UPDATE [Chat_ChatObjectCategoryUnit] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ChatObjectCategoryUnit] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ChatObjectCategoryUnit] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var284 sysname;
SELECT @var284 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectCategoryUnit]') AND [c].[name] = N'ConcurrencyStamp');
IF @var284 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectCategoryUnit] DROP CONSTRAINT [' + @var284 + '];');
UPDATE [Chat_ChatObjectCategoryUnit] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ChatObjectCategoryUnit] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ChatObjectCategoryUnit] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var285 sysname;
SELECT @var285 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectCategory]') AND [c].[name] = N'ExtraProperties');
IF @var285 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectCategory] DROP CONSTRAINT [' + @var285 + '];');
UPDATE [Chat_ChatObjectCategory] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ChatObjectCategory] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ChatObjectCategory] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var286 sysname;
SELECT @var286 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObjectCategory]') AND [c].[name] = N'ConcurrencyStamp');
IF @var286 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObjectCategory] DROP CONSTRAINT [' + @var286 + '];');
UPDATE [Chat_ChatObjectCategory] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ChatObjectCategory] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ChatObjectCategory] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var287 sysname;
SELECT @var287 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObject]') AND [c].[name] = N'ExtraProperties');
IF @var287 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObject] DROP CONSTRAINT [' + @var287 + '];');
UPDATE [Chat_ChatObject] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ChatObject] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ChatObject] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var288 sysname;
SELECT @var288 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ChatObject]') AND [c].[name] = N'ConcurrencyStamp');
IF @var288 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ChatObject] DROP CONSTRAINT [' + @var288 + '];');
UPDATE [Chat_ChatObject] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ChatObject] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ChatObject] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var289 sysname;
SELECT @var289 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_BlobContent]') AND [c].[name] = N'ExtraProperties');
IF @var289 IS NOT NULL EXEC(N'ALTER TABLE [Chat_BlobContent] DROP CONSTRAINT [' + @var289 + '];');
UPDATE [Chat_BlobContent] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_BlobContent] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_BlobContent] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var290 sysname;
SELECT @var290 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_BlobContent]') AND [c].[name] = N'ConcurrencyStamp');
IF @var290 IS NOT NULL EXEC(N'ALTER TABLE [Chat_BlobContent] DROP CONSTRAINT [' + @var290 + '];');
UPDATE [Chat_BlobContent] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_BlobContent] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_BlobContent] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var291 sysname;
SELECT @var291 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Blob]') AND [c].[name] = N'ExtraProperties');
IF @var291 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Blob] DROP CONSTRAINT [' + @var291 + '];');
UPDATE [Chat_Blob] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Blob] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Blob] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var292 sysname;
SELECT @var292 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Blob]') AND [c].[name] = N'ConcurrencyStamp');
IF @var292 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Blob] DROP CONSTRAINT [' + @var292 + '];');
UPDATE [Chat_Blob] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Blob] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Blob] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var293 sysname;
SELECT @var293 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ArticleMessage]') AND [c].[name] = N'ExtraProperties');
IF @var293 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ArticleMessage] DROP CONSTRAINT [' + @var293 + '];');
UPDATE [Chat_ArticleMessage] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_ArticleMessage] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_ArticleMessage] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var294 sysname;
SELECT @var294 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_ArticleMessage]') AND [c].[name] = N'ConcurrencyStamp');
IF @var294 IS NOT NULL EXEC(N'ALTER TABLE [Chat_ArticleMessage] DROP CONSTRAINT [' + @var294 + '];');
UPDATE [Chat_ArticleMessage] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_ArticleMessage] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_ArticleMessage] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

DECLARE @var295 sysname;
SELECT @var295 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Article]') AND [c].[name] = N'ExtraProperties');
IF @var295 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Article] DROP CONSTRAINT [' + @var295 + '];');
UPDATE [Chat_Article] SET [ExtraProperties] = N'' WHERE [ExtraProperties] IS NULL;
ALTER TABLE [Chat_Article] ALTER COLUMN [ExtraProperties] nvarchar(max) NOT NULL;
ALTER TABLE [Chat_Article] ADD DEFAULT N'' FOR [ExtraProperties];
GO

DECLARE @var296 sysname;
SELECT @var296 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Chat_Article]') AND [c].[name] = N'ConcurrencyStamp');
IF @var296 IS NOT NULL EXEC(N'ALTER TABLE [Chat_Article] DROP CONSTRAINT [' + @var296 + '];');
UPDATE [Chat_Article] SET [ConcurrencyStamp] = N'' WHERE [ConcurrencyStamp] IS NULL;
ALTER TABLE [Chat_Article] ALTER COLUMN [ConcurrencyStamp] nvarchar(40) NOT NULL;
ALTER TABLE [Chat_Article] ADD DEFAULT N'' FOR [ConcurrencyStamp];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240717034714_Abp8.2', N'8.0.7');
GO

COMMIT;
GO

