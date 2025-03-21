﻿using IczpNet.Chat.Articles.Dtos;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Articles;

/// <summary>
/// 文章
/// </summary>
public class ArticleAppService(
    IRepository<Article, Guid> repository,
    //IChatObjectManager chatObjectManager,
    IMessageManager messageManager)
        : CrudChatAppService<
        Article,
        ArticleDetailDto,
        ArticleDto,
        Guid,
        ArticleGetListInput,
        ArticleCreateInput,
        ArticleUpdateInput>(repository),
    IArticleAppService
{

    //protected IChatObjectRepository ChatObjectRepository { get; } 

    //protected IChatObjectManager ChatObjectManager { get; }

    protected IMessageManager MessageManager { get; } = messageManager;

    protected override async Task<IQueryable<Article>> CreateFilteredQueryAsync(ArticleGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            //.WhereIf(input.SessionUnitId.HasValue, x => x.SessionUnitId == input.SessionUnitId)
            //.WhereIf(input.Type.HasValue, x => x.Type == input.Type)
            //.WhereIf(input.MinCount.HasValue, x => x.ArticleMemberList.Count >= input.MinCount)
            //.WhereIf(input.MaxCount.HasValue, x => x.ArticleMemberList.Count < input.MaxCount)
            //.WhereIf(input.Type.HasValue, x => x.Type == input.Type)
            //.WhereIf(input.IsForbiddenAll.HasValue, x => x.IsForbiddenAll == input.IsForbiddenAll)
            //.WhereIf(input.MemberOwnerId.HasValue, x => x.ArticleMemberList.Any(d => d.SessionUnitId == input.MemberOwnerId))
            //.WhereIf(input.ForbiddenMemberOwnerId.HasValue, x => x.ArticleForbiddenMemberList.Any(d => d.SessionUnitId == input.ForbiddenMemberOwnerId && d.ExpireTime.HasValue && d.ExpireTime < DateTime.Now))
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Keyword))

            ;
    }

    //public async Task<MessageInfo<ArticleItemInfo>> SendMessageAsync(MessageInput<ArticleItemInfo> input)
    //{
    //    var article = await Repository.InsertAsync(new Article()
    //    {
    //        Title = "Title" + input.Content.Title
    //    }, true);

    //    return await MessageManager.SendMessageAsync<ArticleItemInfo>(input, x =>
    //    {
    //        article.AddMessage(new ArticleMessage(x));
    //        x.SetContentProvider(typeof(ArticleProvider));
    //    });
    //}
}
