using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Mottos;
using IczpNet.Chat.Mottos.Dtos;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MottoServices;

/// <summary>
/// 个性签名
/// </summary>
public class MottoAppService
    : CrudByChatObjectChatAppService<
        Motto,
        MottoDetailDto,
        MottoDto,
        Guid,
        MottoGetListInput,
        MottoCreateInput,
        MottoUpdateInput>,
    IMottoAppService
{
    public MottoAppService(IRepository<Motto, Guid> repository) : base(repository)
    {

    }

    protected override async Task<IQueryable<Motto>> CreateFilteredQueryAsync(ChatObject chatObject, MottoGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(chatObject, input))
            .Where(x => x.OwnerId == chatObject.Id)
            .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Title.Contains(input.Keyword));
    }

    protected override Task CheckGetPolicyAsync(ChatObject owner, Motto entity)
    {
        //Assert.If(entity.SessionUnitId == owner.Id, "Not my entity");

        return base.CheckGetPolicyAsync(owner, entity);
    }

    protected override Task SetCreateEntityAsync(ChatObject owner, Motto entity, MottoCreateInput input)
    {
        owner.SetMotto(entity);

        entity.OwnerId = owner.Id;

        return base.SetCreateEntityAsync(owner, entity, input);
    }

    protected override Task CheckUpdatePolicyAsync(ChatObject owner, Motto entity, MottoUpdateInput input)
    {
        Assert.If(entity.OwnerId == owner.Id, "Not my entity");

        return base.CheckUpdatePolicyAsync(owner, entity, input);
    }

    protected override Task CheckDeletePolicyAsync(ChatObject owner, Motto entity)
    {
        Assert.If(entity.OwnerId == owner.Id, "Not my entity");

        return base.CheckDeletePolicyAsync(owner, entity);
    }

    protected override Expression<Func<Motto, bool>> GetPredicateDeleteManyAsync(ChatObject owner)
    {
        return x => x.OwnerId == owner.Id;
    }

}
