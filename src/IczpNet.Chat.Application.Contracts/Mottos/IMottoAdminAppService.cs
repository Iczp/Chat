using IczpNet.Chat.Mottos.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Mottos;

public interface IMottoAdminAppService :
    ICrudAppService<
        MottoDetailDto,
        MottoDto,
        Guid,
        MottoAdminGetListInput,
        MottoAdminCreateInput,
        MottoUpdateInput>
{
}
