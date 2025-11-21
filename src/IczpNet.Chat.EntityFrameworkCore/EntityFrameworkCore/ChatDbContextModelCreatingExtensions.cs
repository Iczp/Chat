using IczpNet.AbpCommons.EntityFrameworkCore;
using IczpNet.Chat.AppVersionDeviceGroups;
using IczpNet.Chat.AppVersionDevices;
using IczpNet.Chat.AppVersions;
using IczpNet.Chat.Articles;
using IczpNet.Chat.Attributes;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.ChatObjectCategoryUnits;
using IczpNet.Chat.ChatObjectEntryValues;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Connections;
using IczpNet.Chat.DbTables;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.Developers;
using IczpNet.Chat.DeviceGroupMaps;
using IczpNet.Chat.Devices;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.Follows;
using IczpNet.Chat.HttpRequests;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Counters;
using IczpNet.Chat.MessageSections.MessageFollowers;
using IczpNet.Chat.MessageSections.MessageReminders;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageWords;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.Scopeds;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionPermissionUnitGrants;
using IczpNet.Chat.SessionSections.SessionUnitContactTags;
using IczpNet.Chat.SessionSections.SessionUnitCounters;
using IczpNet.Chat.SessionSections.SessionUnitEntryValues;
using IczpNet.Chat.SessionSections.SessionUnitOrganizations;
using IczpNet.Chat.SessionSections.SessionUnitRoles;
using IczpNet.Chat.SessionSections.SessionUnitTags;
using IczpNet.Chat.SessionUnitMessages;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.TextContentWords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace IczpNet.Chat.EntityFrameworkCore;

public static class ChatDbContextModelCreatingExtensions
{
    public static void ConfigureChat(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        /* Configure all entities here. Example:

        builder.Entity<Question>(b =>
        {
            //Configure table & schema name
            b.ToTable(ChatDbProperties.DbTablePrefix + "Questions", ChatDbProperties.DbSchema);

            b.ConfigureByConvention();

            //Properties
            b.Property(q => q.Title).IsRequired().HasMaxLength(QuestionConsts.MaxTitleLength);

            //Relations
            b.HasMany(question => question.Tags).WithOne().HasForeignKey(qt => qt.QuestionId);

            //Indexes
            b.HasIndex(q => q.CreationTime);
        });
        */



        builder.ConfigEntities<ChatDomainModule>(ChatDbProperties.DbTablePrefix, ChatDbProperties.DbSchema);

        ConfigMessageTemplateEntitys(builder);
        //ForEachEntitys(builder);
        //ConfigKeys(builder);

        builder.Entity<DbTable>(b =>
        {
            b.HasNoKey();
            b.ToTable("TableRow", t => t.ExcludeFromMigrations());
        });

        builder.Entity<ChatObject>(b =>
        {
            //b.Property<long>(nameof(ChatObject.AutoId))
            //    .ValueGeneratedOnAdd()
            //    .HasColumnType("bigint")
            //    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
            b.UseTptMappingStrategy();
        });
        builder.Entity<ConnectionChatObject>(b => { b.HasKey(x => new { x.ChatObjectId, x.ConnectionId }); });
        builder.Entity<ChatObjectEntryValue>(b => { b.HasKey(x => new { x.OwnerId, x.EntryValueId }); });
        builder.Entity<ChatObjectCategoryUnit>(b => { b.HasKey(x => new { x.ChatObjectId, x.CategoryId }); });
        builder.Entity<ArticleMessage>(b => { b.HasKey(x => new { x.MessageId, x.ArticleId }); });

        builder.Entity<HistoryMessage>(b => { b.HasKey(x => new { x.MessageId, x.HistoryContentId }); });
        builder.Entity<MessageWord>(b => { b.HasKey(x => new { x.MessageId, x.WordId }); });

        builder.Entity<SessionUnitEntryValue>(b => { b.HasKey(x => new { x.SessionUnitId, x.EntryValueId }); });
        builder.Entity<SessionUnitTag>(b => { b.HasKey(x => new { x.SessionUnitId, x.SessionTagId }); });
        builder.Entity<SessionUnitContactTag>(b => { b.HasKey(x => new { x.SessionUnitId, x.TagId }); });

        builder.Entity<SessionUnitRole>(b => { b.HasKey(x => new { x.SessionUnitId, x.SessionRoleId }); });
        builder.Entity<SessionUnitOrganization>(b => { b.HasKey(x => new { x.SessionUnitId, x.SessionOrganizationId }); });
        builder.Entity<SessionPermissionRoleGrant>(b => { b.HasKey(x => new { x.DefinitionId, x.RoleId }); });
        builder.Entity<SessionPermissionUnitGrant>(b => { b.HasKey(x => new { x.DefinitionId, x.SessionUnitId }); });

        builder.Entity<Follow>(entity =>
        {
            entity.HasKey(x => new { x.OwnerSessionUnitId, x.DestinationSessionUnitId });

            // OwnerSessionUnit 关系
            entity.HasOne(e => e.OwnerSessionUnit)
                .WithMany(su => su.FollowingList)
                .HasForeignKey(e => e.OwnerSessionUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // DestinationSessionUnit 关系
            entity.HasOne(e => e.DestinationSessionUnit)
                .WithMany(su => su.FollowerList)
                .HasForeignKey(e => e.DestinationSessionUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // ChatObject 关系（可选）
            entity.HasOne(e => e.Owner)
                .WithMany(su => su.FollowingList)
                .HasForeignKey(e => e.OwnerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Destination)
                .WithMany(su => su.FollowerList)
                .HasForeignKey(e => e.DestinationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MessageReminder>(b => { b.HasKey(x => new { x.SessionUnitId, x.MessageId }); });
        builder.Entity<MessageFollower>(b => { b.HasKey(x => new { x.SessionUnitId, x.MessageId }); });

        builder.Entity<FavoritedRecorder>(b => { b.HasKey(x => new { x.SessionUnitId, x.MessageId }); });
        builder.Entity<OpenedRecorder>(b => { b.HasKey(x => new { x.SessionUnitId, x.MessageId }); });
        builder.Entity<ReadedRecorder>(b => { b.HasKey(x => new { x.SessionUnitId, x.MessageId }); });
        builder.Entity<DeletedRecorder>(b =>
        {

            b.HasKey(x => new { x.SessionUnitId, x.MessageId });
            //ChatGPT 优化 2025.11.20
            b.HasIndex(x => new { x.MessageId, x.SessionUnitId }).HasDatabaseName("IX_Chat_DeletedRecorder_MessageUnit");
        });

        builder.Entity<Scoped>(b => { b.HasKey(x => new { x.SessionUnitId, x.MessageId }); });

        builder.Entity<TextContentWord>(b => { b.HasKey(x => new { x.TextContentId, x.WordId }); });

        builder.Entity<ReadedCounter>(b => { b.HasKey(x => new { x.MessageId }); });
        builder.Entity<OpenedCounter>(b => { b.HasKey(x => new { x.MessageId }); });
        builder.Entity<FavoritedCounter>(b => { b.HasKey(x => new { x.MessageId }); });
        builder.Entity<DeletedCounter>(b => { b.HasKey(x => new { x.MessageId }); });


        builder.Entity<SessionUnitCounter>(b => { b.HasKey(x => new { x.SessionUnitId }); });
        builder.Entity<SessionUnitSetting>(b => { b.HasKey(x => new { x.SessionUnitId }); });

        builder.Entity<SessionUnit>(b =>
        {
            b.HasOne(x => x.Setting).WithOne(x => x.SessionUnit).HasForeignKey<SessionUnitSetting>(x => x.SessionUnitId).IsRequired(false);
            b.HasOne(x => x.Counter).WithOne(x => x.SessionUnit).HasForeignKey<SessionUnitCounter>(x => x.SessionUnitId).IsRequired(true);
        });

        builder.Entity<SessionUnit>().Navigation(x => x.Setting).AutoInclude();

        builder.Entity<Developer>(b =>
        {
            b.HasKey(x => x.OwnerId);
            b.HasOne(x => x.Owner).WithOne(x => x.Developer).HasForeignKey<Developer>(x => x.OwnerId).IsRequired(true);
        });

        builder.Entity<HttpResponse>(b =>
        {
            b.HasKey(x => new { x.HttpRequestId });
            b.HasOne(x => x.HttpRequest).WithOne(x => x.Response).HasForeignKey<HttpResponse>(x => x.HttpRequestId).IsRequired(true);
        });

        builder.Entity<BlobContent>(b =>
        {
            b.HasKey(x => new { x.BlobId });
            b.HasOne(x => x.Blob).WithOne(x => x.Content).HasForeignKey<BlobContent>(x => x.BlobId).IsRequired(true);
        });



        builder.Entity<Message>(b =>
            {
                var _prefix = $"{ChatDbProperties.DbTablePrefix}_{nameof(Message)}_MapTo";
                b.HasMany(x => x.CmdContentList).WithMany(x => x.MessageList).UsingEntity(x => x.ToTable($"{_prefix}_{nameof(CmdContent)}", ChatDbProperties.DbSchema));
                b.HasMany(x => x.TextContentList).WithMany(x => x.MessageList).UsingEntity(x => x.ToTable($"{_prefix}_{nameof(TextContent)}", ChatDbProperties.DbSchema));
                b.HasMany(x => x.HtmlContentList).WithMany(x => x.MessageList).UsingEntity(x => x.ToTable($"{_prefix}_{nameof(HtmlContent)}", ChatDbProperties.DbSchema));
                b.HasMany(x => x.ArticleContentList).WithMany(x => x.MessageList).UsingEntity(x => x.ToTable($"{_prefix}_{nameof(ArticleContent)}", ChatDbProperties.DbSchema));
                b.HasMany(x => x.LinkContentList).WithMany(x => x.MessageList).UsingEntity(x => x.ToTable($"{_prefix}_{nameof(LinkContent)}", ChatDbProperties.DbSchema));
                b.HasMany(x => x.ImageContentList).WithMany(x => x.MessageList).UsingEntity(x => x.ToTable($"{_prefix}_{nameof(ImageContent)}", ChatDbProperties.DbSchema));
                b.HasMany(x => x.SoundContentList).WithMany(x => x.MessageList).UsingEntity(x => x.ToTable($"{_prefix}_{nameof(SoundContent)}", ChatDbProperties.DbSchema));
                b.HasMany(x => x.VideoContentList).WithMany(x => x.MessageList).UsingEntity(x => x.ToTable($"{_prefix}_{nameof(VideoContent)}", ChatDbProperties.DbSchema));
                b.HasMany(x => x.FileContentList).WithMany(x => x.MessageList).UsingEntity(x => x.ToTable($"{_prefix}_{nameof(FileContent)}", ChatDbProperties.DbSchema));
                b.HasMany(x => x.LocationContentList).WithMany(x => x.MessageList).UsingEntity(x => x.ToTable($"{_prefix}_{nameof(LocationContent)}", ChatDbProperties.DbSchema));
                b.HasMany(x => x.ContactsContentList).WithMany(x => x.MessageList).UsingEntity(x => x.ToTable($"{_prefix}_{nameof(ContactsContent)}", ChatDbProperties.DbSchema));
                b.HasMany(x => x.RedEnvelopeContentList).WithMany(x => x.MessageList).UsingEntity(x => x.ToTable($"{_prefix}_{nameof(RedEnvelopeContent)}", ChatDbProperties.DbSchema));
                b.HasMany(x => x.HistoryContentList).WithMany(x => x.MessageList).UsingEntity(x => x.ToTable($"{_prefix}_{nameof(HistoryContent)}", ChatDbProperties.DbSchema));
                //b.Property<long>(nameof(Message.AutoId))
                //    .ValueGeneratedOnAdd()
                //    .HasColumnType("bigint")
                //    //.HasColumnType("uniqueidentifier")
                //    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.HasOne(x => x.ReadedCounter).WithOne(x => x.Message).HasForeignKey<ReadedCounter>(x => x.MessageId).IsRequired(true);
                b.HasOne(x => x.OpenedCounter).WithOne(x => x.Message).HasForeignKey<OpenedCounter>(x => x.MessageId).IsRequired(true);
                b.HasOne(x => x.FavoritedCounter).WithOne(x => x.Message).HasForeignKey<FavoritedCounter>(x => x.MessageId).IsRequired(true);
                b.HasOne(x => x.DeletedCounter).WithOne(x => x.Message).HasForeignKey<DeletedCounter>(x => x.MessageId).IsRequired(true);

                //ChatGPT 优化 2025.11.20
                b.HasIndex(x => new { x.SessionId, x.IsDeleted, x.IsPrivate }).HasDatabaseName("IX_ChatMessage_CountQuery").IncludeProperties(x => new { x.Id, x.SenderSessionUnitId, x.ReceiverSessionUnitId });

                // 优化 GetList COUNT 的关键索引
                b.HasIndex(x => new
                {
                    x.SessionId,
                    x.IsDeleted,
                    x.IsPrivate,
                    x.SenderSessionUnitId,
                    x.ReceiverSessionUnitId
                })
                .HasDatabaseName("IX_Message_Session_Count")
                .IncludeProperties(x => new { x.Id });

            });

        builder.Entity<SessionUnitMessage>(b =>
        {
            b.HasKey(x => new { x.SessionUnitId, x.MessageId });

            // 非常关键的联合索引（用于查询消息列表）
            b.HasIndex(x => new { x.SessionUnitId, x.MessageId });

            // FK
            b.HasOne(x => x.SessionUnit).WithMany().HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Message).WithMany().HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade);

            // 用于已读查询
            b.HasIndex(x => new { x.SessionUnitId, x.IsRead });

            // 单独用于 MessageId 查询
            b.HasIndex(x => x.MessageId);

            // 可选：SessionUnit 常用字段
            b.HasIndex(x => new { x.SessionUnitId, x.IsOpened });

        });

        builder.Entity<UserDevice>(b => { b.HasKey(x => new { x.UserId, x.DeviceId }); });

        builder.Entity<DeviceGroupMap>(b => { b.HasKey(x => new { x.DeviceGroupId, x.DeviceId }); });

        builder.Entity<AppVersionDevice>(b => { b.HasKey(x => new { x.AppVersionId, x.DeviceId }); });

        builder.Entity<AppVersionDeviceGroup>(b => { b.HasKey(x => new { x.AppVersionId, x.DeviceGroupId }); });

        builder.Entity<AppVersion>(b =>
        {
            b.HasIndex(x => new { x.AppId, x.Platform, x.VersionCode }).IsDescending([false, false, true]).IsUnique();
            b.HasIndex(x => x.VersionCode).IsDescending(true);
        });
    }

    public static void ForEachEntitys(this ModelBuilder builder)
    {
        var moduleType = typeof(ChatDomainModule);

        string entityNamespace = moduleType.Namespace;

        var entityTypes = moduleType.Assembly.GetExportedTypes()
                .Where(t => t.Namespace.StartsWith(entityNamespace) && !t.IsAbstract
                    && t.GetInterfaces().Any(x => typeof(IEntity).IsAssignableFrom(x) || x.IsGenericType && typeof(IEntity<>).IsAssignableFrom(x.GetGenericTypeDefinition())))
                .Where(t => t.GetCustomAttribute<NotMappedAttribute>() == null);


        //foreach (var t in entityTypes)
        //{
        //    builder.Entity(t, ConfigureEnums);
        //}
    }

    private static void ConfigMessageTemplateEntitys(this ModelBuilder builder)
    {
        var moduleType = typeof(ChatDomainModule);

        string entityNamespace = moduleType.Namespace;

        var entityTypes = moduleType.Assembly.GetExportedTypes()
                .Where(t => t.Namespace.StartsWith(entityNamespace) && !t.IsAbstract
                    && t.GetInterfaces().Any(x => typeof(IContentEntity).IsAssignableFrom(x)));

        var _prefix = $"{ChatDbProperties.DbTablePrefix}_{nameof(Message)}_Template";

        foreach (var t in entityTypes)
        {
            builder.Entity(t, b =>
            {
                var tableAttribute = t.GetCustomAttribute<TableAttribute>();

                if (tableAttribute == null)
                {
                    b.ToTable($"{_prefix}_{t.Name}", ChatDbProperties.DbSchema);
                }
            });
        }

    }

    private static void ConfigKeys(this ModelBuilder builder)
    {
        var moduleType = typeof(ChatDomainModule);

        string entityNamespace = moduleType.Namespace;

        var entityTypes = moduleType.Assembly.GetExportedTypes()
                .Where(t => t.Namespace.StartsWith(entityNamespace) && !t.IsAbstract
                    && t.GetInterfaces().Any(x => typeof(IEntity).IsAssignableFrom(x)));

        foreach (var t in entityTypes)
        {
            builder.Entity(t, b =>
            {
                var keyAttributes = t.GetCustomAttributes<HasKeyAttribute>();

                foreach (var keyAttribute in keyAttributes)
                {
                    b.HasKey(keyAttribute.PropertyNames.ToArray());
                }
            });
        }
    }

    public static void ConfigureEnums(this EntityTypeBuilder b)
    {
        foreach (var prop in b.Metadata.GetProperties())
        {
            if (prop.Name.Equals("ExtraProperties", StringComparison.CurrentCulture))
            {
                continue;
            }

            var propType = prop.ClrType;

            if (propType.IsGenericType)
            {
                propType = Nullable.GetUnderlyingType(propType);
            }

            if (!propType.IsEnum)
            {
                continue;
            }

            b.Property(prop.Name).HasConversion<string>().HasMaxLength(20);

            b.HasIndex(prop.Name);
        }
    }
}
