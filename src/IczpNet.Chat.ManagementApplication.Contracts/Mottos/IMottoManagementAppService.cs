using IczpNet.Chat.Management.Mottos.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.Mottos;

public interface IMottoManagementAppService :
    ICrudAppService<
        MottoDetailDto,
        MottoDto,
        Guid,
        MottoAdminGetListInput,
        MottoAdminCreateInput,
        MottoUpdateInput>
{
}
