using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageSections.Messages;

namespace IczpNet.Chat.BaseAppServices;

public abstract class ChatAppService : ApplicationService
{
    protected ICurrentChatObject CurrentChatObject => LazyServiceProvider.LazyGetRequiredService<ICurrentChatObject>();
    protected ChatAppService()
    {
        LocalizationResource = typeof(ChatResource);
        ObjectMapperContext = typeof(ChatApplicationModule);
    }

    protected virtual async Task<PagedResultDto<TOuputDto>> GetPagedListAsync<TEntity, TOuputDto>(IQueryable<TEntity> query, int maxResultCount = 10, int skipCount = 0, string sorting = null)
    {
        var totalCount = await AsyncExecuter.CountAsync(query);

        if (!sorting.IsNullOrWhiteSpace())
        {
            query = query.OrderBy(sorting);
        }
        query = query.PageBy(skipCount, maxResultCount);

        var entities = await AsyncExecuter.ToListAsync(query);

        var items = ObjectMapper.Map<List<TEntity>, List<TOuputDto>>(entities);

        return new PagedResultDto<TOuputDto>(totalCount, items);
    }

    protected virtual  Task<PagedResultDto<TOuputDto>> GetPagedListAsync<TEntity, TOuputDto>(IQueryable<TEntity> query, PagedAndSortedResultRequestDto input)
    {
        return GetPagedListAsync<TEntity, TOuputDto>(query, input.MaxResultCount, input.SkipCount, input.Sorting);
    }
}
