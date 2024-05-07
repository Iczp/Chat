using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Connections;
using IczpNet.Chat.Connections.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services;

/// <summary>
/// 在线人数管理
/// </summary>
public class ConnectionAppService
    : CrudChatAppService<
        Connection,
        ConnectionDetailDto,
        ConnectionDto,
        string,
        ConnectionGetListInput>,
    IConnectionAppService
{
    protected IConnectionManager ConnectionManager { get; }

    public ConnectionAppService(
        IRepository<Connection, string> repository,
        IConnectionManager connectionManager) : base(repository)
    {
        ConnectionManager = connectionManager;
    }

    protected override async Task<IQueryable<Connection>> CreateFilteredQueryAsync(ConnectionGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            //.WhereIf(input.SessionUnitId.HasValue, x => x.SessionUnitId == input.SessionUnitId)
            //.WhereIf(input.Type.HasValue, x => x.Type == input.Type)
            //.WhereIf(input.MinCount.HasValue, x => x.ConnectionMemberList.Count >= input.MinCount)
            //.WhereIf(input.MaxCount.HasValue, x => x.ConnectionMemberList.Count < input.MaxCount)
            //.WhereIf(input.Type.HasValue, x => x.Type == input.Type)
            //.WhereIf(input.IsForbiddenAll.HasValue, x => x.IsForbiddenAll == input.IsForbiddenAll)
            //.WhereIf(input.MemberOwnerId.HasValue, x => x.ConnectionMemberList.Any(d => d.SessionUnitId == input.MemberOwnerId))
            //.WhereIf(input.ForbiddenMemberOwnerId.HasValue, x => x.ConnectionForbiddenMemberList.Any(d => d.SessionUnitId == input.ForbiddenMemberOwnerId && d.ExpireTime.HasValue && d.ExpireTime < DateTime.Now))
            //.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Keyword))

            ;
    }

    /// <summary>
    /// 设置为活跃的
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ticks"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<string> SetActiveAsync(string id, string ticks)
    {
        await ConnectionManager.UpdateActiveTimeAsync(id);
        return ticks;
    }

    //[RemoteService(false)]
    //public override Task DeleteAsync(string id)
    //{
    //    return base.DeleteAsync(id);
    //}

    /// <summary>
    /// 获取在线用户数量
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<GetOnlineCountOutput> GetOnlineCountAsync()
    {
        var currentTime = Clock.Now;
        var count = await ConnectionManager.GetOnlineCountAsync(currentTime);
        return new GetOnlineCountOutput { Count = count, CurrentTime = currentTime };
    }
}
