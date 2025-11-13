using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ScanCodes;

public class ScanCodeAppService(
    IScanCodeManager scanCodeManager,
    IRepository<ScanCode, Guid> repository) : CrudChatAppService<ScanCode, ScanCodeDetailDto, ScanCodeDto, Guid, ScanCodeGetListInput>(repository), IScanCodeAppService
{

    protected override string GetPolicyName { get; set; } = ChatPermissions.ScanCodePermission.Default;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.ScanCodePermission.Default;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.ScanCodePermission.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.ScanCodePermission.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.ScanCodePermission.Delete;

    public IScanCodeManager ScanCodeManager { get; } = scanCodeManager;

    protected override async Task<IQueryable<ScanCode>> CreateFilteredQueryAsync(ScanCodeGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Action), x => x.ScanHandlerList.Any(d => d.Action == input.Action))
            .WhereIf(!string.IsNullOrWhiteSpace(input.HandlerFullName), x => x.ScanHandlerList.Any(d => d.HandlerFullName == input.HandlerFullName))
            .WhereIf(input.Success.HasValue, x => x.ScanHandlerList.Any(d => d.Success == input.Success))
        ;
    }

    public async Task<ScanCodeResultDto> ScanAsync(string type, string content)
    {
        var entity = await ScanCodeManager.ScanCodeAsync(type, content);

        var output = ObjectMapper.Map<ScanCode, ScanCodeResultDto>(entity);

        return output;
    }
}
