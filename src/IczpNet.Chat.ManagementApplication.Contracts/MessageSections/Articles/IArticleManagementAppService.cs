using IczpNet.Chat.Management.Articles.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.Articles;

public interface IArticleManagementAppService :
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
