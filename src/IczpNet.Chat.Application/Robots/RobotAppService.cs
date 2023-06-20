using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Robots.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Robots;

/// <summary>
/// 机器人
/// </summary>
public class RobotAppService : ChatAppService, IRobotAppService
{
    protected IChatObjectRepository Repository { get; }

    public RobotAppService(IChatObjectRepository repository)
    {
        Repository = repository;
    }


    protected virtual Task<IQueryable<ChatObject>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        return Repository.GetQueryableAsync();
    }

    public virtual async Task<PagedResultDto<RobotDto>> GetListAsync(RobotGetListInput input)
    {
        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.ObjectType == ChatObjectTypeEnums.Robot);

        return await GetPagedListAsync<ChatObject, RobotDto>(query, input, x => x.OrderBy(d => d.Name));

    }
}
