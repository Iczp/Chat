using IczpNet.AbpCommons.EntityFrameworkCore;
using IczpNet.Chat.Attributes;
using IczpNet.Chat.Messages;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RoomSections.RoomPermissionGrants;
using IczpNet.Chat.RoomSections.RoomRoleRoomMembers;
using IczpNet.Chat.SessionSections.Friends;
using IczpNet.Chat.SessionSections.SessionSettings;
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

        builder.ConfigEntitys<ChatDomainModule>(ChatDbProperties.DbTablePrefix, ChatDbProperties.DbSchema);

        ConfigMessageTemplateEntitys(builder);
        //ForEachEntitys(builder);
        //ConfigKeys(builder);

        //builder.Entity<Friendship>(b =>
        //{
        //    b.HasKey(x => new { x.OwnerId, x.DestinationId });
        //});


        builder.Entity<HistoryMessage>(b =>
        {
            b.HasKey(x => new { x.MessageId, x.HistoryContentId });
        });

        builder.Entity<SessionSetting>(b =>
        {
            b.HasKey(x => new { x.OwnerId, x.SessionId });
        });

        builder.Entity<RoomPermissionGrant>(b =>
        {
            b.HasKey(x => new { x.DefineId, x.RoleId });
        });

        builder.Entity<RoomRoleRoomMember>(b =>
        {
            b.HasKey(x => new { x.RoleId, x.RoomMemberId });
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
        });
    }

    public static void ForEachEntitys(this ModelBuilder builder)
    {
        var moduleType = typeof(ChatDomainModule);

        string entityNamespace = moduleType.Namespace;

        var entityTypes = moduleType.Assembly.GetExportedTypes()
                .Where(t => t.Namespace.StartsWith(entityNamespace) && !t.IsAbstract
                    && t.GetInterfaces().Any(x => typeof(IEntity).IsAssignableFrom(x) || x.IsGenericType && typeof(IEntity<>).IsAssignableFrom(x.GetGenericTypeDefinition())));


        foreach (var t in entityTypes)
        {
            builder.Entity(t, ConfigureEnums);
        }
    }

    private static void ConfigMessageTemplateEntitys(this ModelBuilder builder)
    {
        var moduleType = typeof(ChatDomainModule);

        string entityNamespace = moduleType.Namespace;

        var entityTypes = moduleType.Assembly.GetExportedTypes()
                .Where(t => t.Namespace.StartsWith(entityNamespace) && !t.IsAbstract
                    && t.GetInterfaces().Any(x => typeof(IMessageContent).IsAssignableFrom(x)));

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

            b.Property(prop.Name).HasConversion<string>();
        }
    }
}
