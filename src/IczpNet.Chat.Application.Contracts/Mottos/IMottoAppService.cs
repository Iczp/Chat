using IczpNet.Chat.Mottos.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Mottos;

public interface IMottoAppService :
    ICrudAppService<
        MottoDetailDto,
        MottoDto,
        Guid,
        MottoGetListInput,
        MottoCreateInput,
        MottoUpdateInput>
{
}
