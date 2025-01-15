using IczpNet.AbpTrees;
using IczpNet.Chat.Menus.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Menus;

public interface IMenuAppService :
    ICrudAppService<
        MenuDto,
        MenuDto,
        Guid,
        MenuGetListInput,
        MenuCreateInput,
        MenuUpdateInput>
    ,
    ITreeAppService<
        MenuDto,
        MenuDto,
        Guid,
        MenuGetListInput,
        MenuCreateInput,
        MenuUpdateInput, MenuInfo>
{

    Task<string> TriggerAsync(Guid id);
}
