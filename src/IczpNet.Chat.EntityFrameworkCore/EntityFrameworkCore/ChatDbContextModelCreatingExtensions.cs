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
using IczpNet.Chat.MessageReports;
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
        builder.Entity<ConnectionChatObject>(b =>
        {
            b.HasKey(x => new { x.ChatObjectId, x.ConnectionId });
            b.HasOne(x => x.ChatObject).WithMany(x => x.ConnectionChatObjectList).HasForeignKey(x => x.ChatObjectId).OnDelete(DeleteBehavior.NoAction).IsRequired(false);
            b.HasOne(x => x.Connection).WithMany(x => x.ConnectionChatObjectList).HasForeignKey(x => x.ConnectionId).OnDelete(DeleteBehavior.NoAction).IsRequired(false);
        });
        builder.Entity<ChatObjectEntryValue>(b =>
        {
            b.HasKey(x => new { x.OwnerId, x.EntryValueId });
            b.HasOne(x => x.Owner).WithMany(x => x.Entries).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.NoAction).IsRequired(false);
            b.HasOne(x => x.EntryValue).WithMany(x => x.ChatObjectEntryValueList).HasForeignKey(x => x.EntryValueId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
        });

        builder.Entity<ChatObjectCategoryUnit>(b =>
        {
            b.HasKey(x => new { x.ChatObjectId, x.CategoryId });
            b.HasOne(x => x.Category).WithMany(x => x.ChatObjectCategoryUnitList).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            b.HasOne(x => x.ChatObject).WithMany(x => x.ChatObjectCategoryUnitList).HasForeignKey(x => x.ChatObjectId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
        });
        builder.Entity<ArticleMessage>(b =>
        {
            b.HasKey(x => new { x.MessageId, x.ArticleId });
            b.HasOne(x => x.Article).WithMany(x => x.MessageList).HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            b.HasOne(x => x.Message).WithMany(x => x.ArticleMessageList).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
        });

        builder.Entity<HistoryMessage>(b =>
        {
            b.HasKey(x => new { x.MessageId, x.HistoryContentId });
            b.HasOne(x => x.Message).WithMany(x => x.HistoryMessageList).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            b.HasOne(x => x.HistoryContent).WithMany(x => x.HistoryMessageList).HasForeignKey(x => x.HistoryContentId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
        });
        builder.Entity<MessageWord>(b =>
        {
            b.HasKey(x => new { x.MessageId, x.WordId });
            b.HasOne(x => x.Message).WithMany(x => x.MessageWordList).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            b.HasOne(x => x.Word).WithMany(x => x.MessageWordList).HasForeignKey(x => x.WordId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
        });

        builder.Entity<SessionUnitEntryValue>(b =>
        {
            b.HasKey(x => new { x.SessionUnitId, x.EntryValueId });
            b.HasOne(x => x.SessionUnit).WithMany(x => x.Entries).HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            b.HasOne(x => x.EntryValue).WithMany(x => x.SessionUnitEntryValueList).HasForeignKey(x => x.EntryValueId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
        });
        builder.Entity<SessionUnitTag>(b =>
        {
            b.HasKey(x => new { x.SessionUnitId, x.SessionTagId });
            b.HasOne(x => x.SessionUnit).WithMany(x => x.SessionUnitTagList).HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            b.HasOne(x => x.SessionTag).WithMany(x => x.SessionUnitTagList).HasForeignKey(x => x.SessionTagId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);

        });
        builder.Entity<SessionUnitContactTag>(b =>
        {
            b.HasKey(x => new { x.SessionUnitId, x.TagId });
            b.HasOne(x => x.SessionUnit).WithMany(x => x.SessionUnitContactTagList).HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            b.HasOne(x => x.Tag).WithMany(x => x.SessionUnitContactTagList).HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
        });

        builder.Entity<SessionUnitRole>(b =>
        {
            b.HasKey(x => new { x.SessionUnitId, x.SessionRoleId });
            b.HasOne(x => x.SessionUnit).WithMany(x => x.SessionUnitRoleList).HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            b.HasOne(x => x.SessionRole).WithMany(x => x.SessionUnitRoleList).HasForeignKey(x => x.SessionRoleId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
        });
        builder.Entity<SessionUnitOrganization>(b =>
        {
            b.HasKey(x => new { x.SessionUnitId, x.SessionOrganizationId });
            b.HasOne(x => x.SessionUnit).WithMany(x => x.SessionUnitOrganizationList).HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            b.HasOne(x => x.SessionOrganization).WithMany(x => x.SessionUnitOrganizationList).HasForeignKey(x => x.SessionOrganizationId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
        });
        builder.Entity<SessionPermissionRoleGrant>(b =>
        {
            b.HasKey(x => new { x.DefinitionId, x.RoleId });
            b.HasOne(x => x.Definition).WithMany(x => x.RoleGrantList).HasForeignKey(x => x.DefinitionId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            b.HasOne(x => x.Role).WithMany(x => x.GrantList).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
        });
        builder.Entity<SessionPermissionUnitGrant>(b =>
        {
            b.HasKey(x => new { x.DefinitionId, x.SessionUnitId });
            b.HasOne(x => x.Definition).WithMany(x => x.UnitGrantList).HasForeignKey(x => x.DefinitionId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            b.HasOne(x => x.SessionUnit).WithMany(x => x.GrantList).HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
        });

        builder.Entity<Follow>(entity =>
        {
            entity.HasKey(x => new { x.OwnerSessionUnitId, x.DestinationSessionUnitId });

            // OwnerSessionUnit 关系
            entity.HasOne(e => e.OwnerSessionUnit)
                .WithMany(su => su.FollowingList)
                .HasForeignKey(e => e.OwnerSessionUnitId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // DestinationSessionUnit 关系
            entity.HasOne(e => e.DestinationSessionUnit)
                .WithMany(su => su.FollowerList)
                .HasForeignKey(e => e.DestinationSessionUnitId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

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

        builder.Entity<MessageReminder>(b =>
        {
            b.HasKey(x => new { x.SessionUnitId, x.MessageId });
            b.HasIndex(x => x.CreationTime).IsDescending([true]);

            b.HasOne(x => x.Message).WithMany(x => x.MessageReminderList).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            b.HasOne(x => x.SessionUnit).WithMany(x => x.ReminderList).HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);

        });
        builder.Entity<MessageFollower>(b =>
        {
            b.HasKey(x => new { x.SessionUnitId, x.MessageId });
            b.HasOne(x => x.Message).WithMany(x => x.MessageFollowerList).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            b.HasOne(x => x.SessionUnit).WithMany(x => x.MessageFollowerList).HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        builder.Entity<FavoritedRecorder>(b =>
        {
            b.HasKey(x => new { x.SessionUnitId, x.MessageId });
            b.HasOne(x => x.Message).WithMany(x => x.FavoriteList).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            b.HasOne(x => x.SessionUnit).WithMany(x => x.FavoriteList).HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);

        });
        builder.Entity<OpenedRecorder>(b =>
        {
            b.HasKey(x => new { x.SessionUnitId, x.MessageId });
            b.HasOne(x => x.Message).WithMany(x => x.OpenedRecorderList).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            b.HasOne(x => x.SessionUnit).WithMany(x => x.OpenedRecorderList).HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });
        builder.Entity<ReadedRecorder>(b =>
        {
            b.HasKey(x => new { x.SessionUnitId, x.MessageId });
            b.HasOne(x => x.Message).WithMany(x => x.ReadedRecorderList).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            b.HasOne(x => x.SessionUnit).WithMany(x => x.ReadedRecorderList).HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });
        builder.Entity<DeletedRecorder>(b =>
        {

            b.HasKey(x => new { x.SessionUnitId, x.MessageId });
            b.HasOne(x => x.Message).WithMany(x => x.DeletedList).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            b.HasOne(x => x.SessionUnit).WithMany(x => x.DeletedRecorderList).HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            ////ChatGPT 优化 2025.11.20
            //b.HasIndex(x => new { x.SessionUnitId, x.MessageId }).HasDatabaseName("IX_Chat_DeletedRecorder_MessageUnit");
        });

        builder.Entity<Scoped>(b =>
        {
            b.HasKey(x => new { x.SessionUnitId, x.MessageId });
            b.HasOne(x => x.Message).WithMany(x => x.ScopedList).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            b.HasOne(x => x.SessionUnit).WithMany(x => x.ScopedList).HasForeignKey(x => x.SessionUnitId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        builder.Entity<TextContentWord>(b =>
        {
            b.HasKey(x => new { x.TextContentId, x.WordId });
            b.HasOne(x => x.Word).WithMany(x => x.TextContentWordList).HasForeignKey(x => x.WordId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            b.HasOne(x => x.TextContent).WithMany(x => x.TextContentWordList).HasForeignKey(x => x.TextContentId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);

        });

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

            // 唯一约束：同一个会话 OwnerId 只能有一条记录
            b.HasIndex(x => new { x.OwnerId, x.SessionId }).IsUnique();

            // 唯一约束：会话好友 只能有一条记录
            b.HasIndex(x => new { x.OwnerId, x.DestinationId }).IsUnique();

            // 外键
            b.HasOne(x => x.Owner)
                .WithMany(x => x.OwnerSessionUnitList)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict); //DeleteBehavior.Restrict: 删除 ChatObject（用户）时，系统应该阻止删除


            // 外键
            b.HasOne(x => x.Destination)
                .WithMany(x => x.DestinationSessionUnitList)
                .HasForeignKey(x => x.DestinationId)
                .OnDelete(DeleteBehavior.Restrict); //DeleteBehavior.Restrict: 删除 ChatObject（用户）时，系统应该阻止删除

            //b.HasOne(x => x.Box)
            //    .WithMany(x => x.SessionUnitList)
            //    .HasForeignKey(x => x.BoxId)
            //    //.IsRequired()
            //    .OnDelete(DeleteBehavior.SetNull);

        });

        builder.Entity<SessionUnit>().Navigation(x => x.Setting).AutoInclude();

        builder.Entity<Developer>(b =>
        {
            b.HasKey(x => x.OwnerId);
            b.HasOne(x => x.Owner).WithOne(x => x.Developer).HasForeignKey<Developer>(x => x.OwnerId).IsRequired(false);
        });

        builder.Entity<HttpResponse>(b =>
        {
            b.HasKey(x => new { x.HttpRequestId });
            b.HasOne(x => x.HttpRequest).WithOne(x => x.Response).HasForeignKey<HttpResponse>(x => x.HttpRequestId).IsRequired(false);
        });

        builder.Entity<BlobContent>(b =>
        {
            b.HasKey(x => new { x.BlobId });
            b.HasOne(x => x.Blob).WithOne(x => x.Content).HasForeignKey<BlobContent>(x => x.BlobId).OnDelete(DeleteBehavior.NoAction).IsRequired(false);
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
            // 主键，自增 Id
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();

            // 唯一约束：一个 SessionUnit 对同一 message 只能有一条记录
            b.HasIndex(x => new { x.SessionUnitId, x.MessageId }).IsUnique();

            // MessageId 单字段查询（比如通过消息找所有 SessionUnitMessage）
            b.HasIndex(x => x.MessageId).IsDescending(true);

            // 已读查询
            b.HasIndex(x => new { x.SessionUnitId, x.IsRead });

            // 是否关注
            b.HasIndex(x => new { x.SessionUnitId, x.IsFollowing });

            // 是否收藏
            b.HasIndex(x => new { x.SessionUnitId, x.IsFavorited });

            // 创建时间排序（用于按时间分页）
            b.HasIndex(x => x.CreationTime).IsDescending(true);

            // 外键
            b.HasOne(x => x.SessionUnit)
                .WithMany()
                .HasForeignKey(x => x.SessionUnitId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Message)
                .WithMany()
                .HasForeignKey(x => x.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        builder.Entity<MessageReportDay>(b =>
        {
            b.HasIndex(x => new { x.SessionId, x.MessageType }).IsUnique(false);
            b.HasIndex(x => new { x.SessionId, x.DateBucket, x.MessageType }).IsUnique(true);
            b.HasIndex(x => x.SessionId);
            b.HasIndex(x => x.MessageType);
            //b.Property(x => x.MessageType).HasConversion<string>().HasMaxLength(32);
        });
        builder.Entity<MessageReportMonth>(b =>
        {
            b.HasIndex(x => new { x.SessionId, x.MessageType }).IsUnique(false);
            b.HasIndex(x => new { x.SessionId, x.DateBucket, x.MessageType }).IsUnique(true);
            b.HasIndex(x => x.SessionId);
            b.HasIndex(x => x.MessageType);
            //b.Property(x => x.MessageType).HasConversion<string>().HasMaxLength(32);
        });
        builder.Entity<MessageReportHour>(b =>
        {
            b.HasIndex(x => new { x.SessionId, x.MessageType }).IsUnique(false);
            b.HasIndex(x => new { x.SessionId, x.DateBucket, x.MessageType }).IsUnique(true);
            b.HasIndex(x => x.SessionId);
            b.HasIndex(x => x.MessageType);
            //b.Property(x => x.MessageType).HasConversion<string>().HasMaxLength(32);
        });

        builder.Entity<UserDevice>(b =>
        {
            b.HasKey(x => new { x.UserId, x.DeviceId });
            b.HasOne(x => x.Device).WithMany(x => x.UserDeviceList).HasForeignKey(x => x.DeviceId).IsRequired(false);
        });

        builder.Entity<DeviceGroupMap>(b =>
        {
            b.HasKey(x => new { x.DeviceGroupId, x.DeviceId });
            b.HasOne(x => x.Device).WithMany(x => x.DeviceGroupMapList).HasForeignKey(x => x.DeviceId).IsRequired(false);
            b.HasOne(x => x.DeviceGroup).WithMany(x => x.DeviceGroupMapList).HasForeignKey(x => x.DeviceGroupId).IsRequired(false);
        });

        builder.Entity<AppVersionDevice>(b =>
        {
            b.HasKey(x => new { x.AppVersionId, x.DeviceId });
            b.HasOne(x => x.AppVersion).WithMany(x => x.VersionDeviceList).HasForeignKey(x => x.AppVersionId).IsRequired(false);
            b.HasOne(x => x.Device).WithMany(x => x.AppVersionDeviceList).HasForeignKey(x => x.DeviceId).IsRequired(false);
        });

        builder.Entity<AppVersionDeviceGroup>(b =>
        {
            b.HasKey(x => new { x.AppVersionId, x.DeviceGroupId });
            b.HasOne(x => x.DeviceGroup).WithMany(x => x.AppVersionDeviceGroupList).HasForeignKey(x => x.DeviceGroupId).IsRequired(false);
        });

        builder.Entity<AppVersion>(b =>
        {
            b.HasIndex(x => new { x.AppId, x.Platform, x.VersionCode }).IsDescending([false, false, true]).IsUnique();
            b.HasIndex(x => x.VersionCode).IsDescending(true);
            b.HasMany(x => x.AppVersionDeviceGroupList)
                .WithOne(x => x.AppVersion)
                .HasForeignKey(x => x.AppVersionId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

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
