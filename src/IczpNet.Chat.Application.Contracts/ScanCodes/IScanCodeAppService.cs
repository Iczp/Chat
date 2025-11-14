
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.ScanCodes;

public interface IScanCodeAppService : ICrudChatAppService<
        ScanCodeDetailDto,
        ScanCodeDto,
        Guid,
        ScanCodeGetListInput>
{

    Task<ScanCodeResultDto> ScanAsync(string type,string content);

}
