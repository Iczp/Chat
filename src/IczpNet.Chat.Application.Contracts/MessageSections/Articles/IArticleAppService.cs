using IczpNet.Chat.Articles.Dtos;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Articles;

public interface IArticleAppService :
    ICrudAppService<
        ArticleDetailDto,
        ArticleDto,
        Guid,
        ArticleGetListInput,
        ArticleCreateInput,
        ArticleUpdateInput>
{

    //Task<MessageInfo<ArticleItemInfo>> SendMessageAsync(MessageInput<ArticleItemInfo> input);
}
