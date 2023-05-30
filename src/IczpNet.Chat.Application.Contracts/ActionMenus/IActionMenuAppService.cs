using IczpNet.AbpTrees;
using IczpNet.Chat.ActionMenus.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.ActionMenus
{
    public interface IActionMenuAppService :
        ICrudAppService<
            ActionMenuDto,
            ActionMenuDto,
            long,
            ActionMenuGetListInput,
            ActionMenuCreateInput,
            ActionMenuUpdateInput>
        ,
        ITreeAppService<
            ActionMenuDto,
            ActionMenuDto,
            long,
            ActionMenuGetListInput,
            ActionMenuCreateInput,
            ActionMenuUpdateInput, ActionMenuInfo>
    {
        

    }
}
