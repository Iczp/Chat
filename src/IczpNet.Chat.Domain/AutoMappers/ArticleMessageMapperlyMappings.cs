using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainArticleContentMapper : MapperBase<ArticleContent, ArticleContentInfo>
{
    public override ArticleContentInfo Map(ArticleContent source)
    {
        if (source is null) return default!;
        var result = new ArticleContentInfo();
        Map(source, result);
        return result;
    }
    public override void Map(ArticleContent source, ArticleContentInfo destination)
    {
        MapFields(source, destination);
        destination.Items = source.GetItems();
    }
    [MapperIgnoreTarget(nameof(ArticleContentInfo.Items))]
    private partial void MapFields(ArticleContent source, ArticleContentInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToArticleMessageMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<ArticleContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;
    public override MessageInfo<ArticleContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var result = new MessageInfo<ArticleContentInfo>();
        Map(source, result);
        return result;
    }
    public override void Map(Message source, MessageInfo<ArticleContentInfo> destination)
    {
        MapFields(source, destination);
        destination.Content = (ArticleContentInfo)source.Content;
    }
    [MapperIgnoreTarget(nameof(MessageInfo<ArticleContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<ArticleContentInfo> destination);
}
