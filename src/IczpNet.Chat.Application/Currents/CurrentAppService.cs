using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Currents.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.Currents;

public class CurrentAppService(

    ) : ChatAppService, ICurrentAppService
{
    public async Task<List<TabDto>> GetTabsAsync(long chatObjectId)
    {
        await Task.Yield();

        return [
            new TabDto() {
                Name = "全部",
                Code = "all",
                Badge = 0,
                IsDot = false,
             },
            new TabDto(){
                Name = "未读",
                Code="unread",
                Badge=0,
                IsDot = false,
             },
             new TabDto(){
                Name = "关注",
                Code="following",
                Badge=0,
                IsDot = false,
             },
             new TabDto(){
                Name = "@我",
                Code="reminder",
                Badge=0,
                IsDot = false,
             },
             new TabDto(){
                Name = "群聊",
                Code="group",
                Badge=0,
                IsDot = false,
             },
             new TabDto(){
                Name = "广场",
                Code="square",
                Badge=0,
                IsDot = false,
             },
        ];
    }
}
