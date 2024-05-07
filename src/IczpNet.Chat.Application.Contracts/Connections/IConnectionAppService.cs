using IczpNet.Chat.Connections.Dtos;
using System.Threading.Tasks;

namespace IczpNet.Chat.Connections;

public interface IConnectionAppService : ICrudChatAppService<ConnectionDetailDto,
        ConnectionDto,
        string,
        ConnectionGetListInput>
{
    /// <summary>
    /// 设置为活跃的
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ticks"></param>
    /// <returns></returns>
    Task<string> SetActiveAsync(string id, string ticks);

    /// <summary>
    /// 获取在线用户数量
    /// </summary>
    /// <returns></returns>
    Task<GetOnlineCountOutput> GetOnlineCountAsync();
}
