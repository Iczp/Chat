using IczpNet.Chat.Management.Connections.Dtos;
using IczpNet.Chat.Management.Management.Connections.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.Connections;

public interface IConnectionManagementAppService : ICrudAppService<ConnectionDto, Guid, ConnectionGetListInput, ConnectionCreateInput>
{
    Task<string> ActiveAsync(Guid id, string ticks);

    Task<GetOnlineCountOutput> GetOnlineCountAsync();
    

}
