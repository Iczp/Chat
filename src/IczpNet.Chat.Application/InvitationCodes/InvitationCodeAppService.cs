using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.InvitationCodes.Dtos;
using IczpNet.Pusher.ShortIds;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.InvitationCodes;

/// <summary>
/// 邀请码
/// </summary>
public class InvitationCodeAppService(IRepository<InvitationCode, Guid> repository, IShortIdGenerator shortIdGenerator)
        : CrudChatAppService<
        InvitationCode,
        InvitationCodeDetailDto,
        InvitationCodeDto,
        Guid,
        InvitationCodeGetListInput,
        InvitationCodeCreateInput,
        InvitationCodeUpdateInput>(repository),
    IInvitationCodeAppService
{
    protected IShortIdGenerator ShortIdGenerator { get; } = shortIdGenerator;

    protected override async Task<IQueryable<InvitationCode>> CreateFilteredQueryAsync(InvitationCodeGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Title.Contains(input.Keyword));
    }

    protected override async Task SetCreateEntityAsync(InvitationCode entity, InvitationCodeCreateInput input)
    {
        var code = await ShortIdGenerator.CreateAsync();

        entity.SetOwnerId(input.OwnerId);

        entity.SetTitle(input.Title);

        entity.SetCode(code);

        entity.SetExpirationTime(DateTime.Now.AddDays(7));

        await base.SetCreateEntityAsync(entity, input);
    }

}
