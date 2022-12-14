using IczpNet.Chat.Connections.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Connections;

public interface IConnectionAppService : ICrudAppService<ConnectionDto, Guid, ConnectionGetListInput, ConnectionCreateInput>
{
    Task<string> ActiveAsync(Guid id, string ticks);

    Task<GetOnlineCountOutput> GetOnlineCountAsync();
    

}
