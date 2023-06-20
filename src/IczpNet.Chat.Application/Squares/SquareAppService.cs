using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Squares.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Squares;

/// <summary>
/// 聊天广场
/// </summary>
public class SquareAppService : ChatAppService, ISquareAppService
{
    protected IChatObjectRepository Repository { get; }

    public SquareAppService(IChatObjectRepository repository)
    {
        Repository = repository;
    }


    protected virtual Task<IQueryable<ChatObject>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        return Repository.GetQueryableAsync();
    }

    public virtual async Task<PagedResultDto<SquareDto>> GetListAsync(SquareGetListInput input)
    {
        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.ObjectType == ChatObjectTypeEnums.Square);

        return await GetPagedListAsync<ChatObject, SquareDto>(query, input, x => x.OrderBy(d => d.Name));

    }
}
