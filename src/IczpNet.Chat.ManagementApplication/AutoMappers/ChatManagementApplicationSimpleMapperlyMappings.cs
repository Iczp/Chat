// Generated from conventional CreateMap declarations. Keep complex maps in the profile until migrated.
using IczpNet.Chat.Articles;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Connections;
using IczpNet.Chat.ChatObjectCategories;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Mottos;
using IczpNet.Chat.Management.ChatObjects.Dtos;
using IczpNet.Chat.Management.Mottos.Dtos;
using IczpNet.Chat.Management.ChatObjectCategorys.Dtos;
using IczpNet.Chat.Management.ChatObjectTypes.Dtos;
using IczpNet.Chat.Management.Articles.Dtos;
using IczpNet.Chat.Management.Management.Connections.Dtos;
using IczpNet.Chat.Management.Connections.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.Management.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatManagementApplicationChatObjectToChatObjectDtoMapper : MapperBase<ChatObject, ChatObjectDto>
{
    public override partial ChatObjectDto Map(ChatObject source);

    public override partial void Map(ChatObject source, ChatObjectDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatManagementApplicationChatObjectToChatObjectSimpleDtoMapper : MapperBase<ChatObject, ChatObjectSimpleDto>
{
    public override partial ChatObjectSimpleDto Map(ChatObject source);

    public override partial void Map(ChatObject source, ChatObjectSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatManagementApplicationMottoToMottoDtoMapper : MapperBase<Motto, MottoDto>
{
    public override partial MottoDto Map(Motto source);

    public override partial void Map(Motto source, MottoDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatManagementApplicationMottoToMottoSimpleDtoMapper : MapperBase<Motto, MottoSimpleDto>
{
    public override partial MottoSimpleDto Map(Motto source);

    public override partial void Map(Motto source, MottoSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatManagementApplicationMottoToMottoDetailDtoMapper : MapperBase<Motto, MottoDetailDto>
{
    public override partial MottoDetailDto Map(Motto source);

    public override partial void Map(Motto source, MottoDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatManagementApplicationChatObjectCategoryToChatObjectCategoryDtoMapper : MapperBase<ChatObjectCategory, ChatObjectCategoryDto>
{
    public override partial ChatObjectCategoryDto Map(ChatObjectCategory source);

    public override partial void Map(ChatObjectCategory source, ChatObjectCategoryDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatManagementApplicationChatObjectCategoryToChatObjectCategoryDetailDtoMapper : MapperBase<ChatObjectCategory, ChatObjectCategoryDetailDto>
{
    public override partial ChatObjectCategoryDetailDto Map(ChatObjectCategory source);

    public override partial void Map(ChatObjectCategory source, ChatObjectCategoryDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatManagementApplicationChatObjectCategoryToChatObjectCategoryInfoMapper : MapperBase<ChatObjectCategory, ChatObjectCategoryInfo>
{
    public override partial ChatObjectCategoryInfo Map(ChatObjectCategory source);

    public override partial void Map(ChatObjectCategory source, ChatObjectCategoryInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatManagementApplicationArticleToArticleDtoMapper : MapperBase<Article, ArticleDto>
{
    public override partial ArticleDto Map(Article source);

    public override partial void Map(Article source, ArticleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatManagementApplicationArticleToArticleDetailDtoMapper : MapperBase<Article, ArticleDetailDto>
{
    public override partial ArticleDetailDto Map(Article source);

    public override partial void Map(Article source, ArticleDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatManagementApplicationConnectionToConnectionDtoMapper : MapperBase<Connection, ConnectionDto>
{
    public override partial ConnectionDto Map(Connection source);

    public override partial void Map(Connection source, ConnectionDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatManagementApplicationConnectionToConnectionDetailDtoMapper : MapperBase<Connection, ConnectionDetailDto>
{
    public override partial ConnectionDetailDto Map(Connection source);

    public override partial void Map(Connection source, ConnectionDetailDto destination);
}
