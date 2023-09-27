using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.InvitationCodes;
using IczpNet.Chat.InvitationCodes.Dtos;
using IczpNet.Pusher.ShortIds;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.InvitationCodeServices;

/// <summary>
/// 邀请码
/// </summary>
public class InvitationCodeAppService
    : CrudChatAppService<
        InvitationCode,
        InvitationCodeDetailDto,
        InvitationCodeDto,
        Guid,
        InvitationCodeGetListInput,
        InvitationCodeCreateInput,
        InvitationCodeUpdateInput>,
    IInvitationCodeAppService
{
    protected IShortIdGenerator ShortIdGenerator { get; }
    public InvitationCodeAppService(IRepository<InvitationCode, Guid> repository, IShortIdGenerator shortIdGenerator) : base(repository)
    {
        ShortIdGenerator = shortIdGenerator;
    }

    protected override async Task<IQueryable<InvitationCode>> CreateFilteredQueryAsync(InvitationCodeGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Title.Contains(input.Keyword));
    }

    protected override async Task SetCreateEntityAsync(InvitationCode entity, InvitationCodeCreateInput input)
    {
        var code = await ShortIdGenerator.MakeAsync();

        entity.SetOwnerId(input.OwnerId);

        entity.SetTitle(input.Title);

        entity.SetCode(code);

        entity.SetExpirationTime(DateTime.Now.AddDays(7));

        await base.SetCreateEntityAsync(entity, input);
    }

}
