using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Squares.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Squares
{
    public interface ISquareAppService
    {
        Task<PagedResultDto<SquareDto>> GetListAsync(SquareGetListInput input);
    }
}
