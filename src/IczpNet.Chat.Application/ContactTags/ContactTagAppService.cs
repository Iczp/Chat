using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ContactTags.Dtos;
using IczpNet.Chat.Permissions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ContactTags;

/// <summary>
/// 联系人标签
/// </summary>
public class ContactTagAppService
    : CrudChatAppService<
        ContactTag,
        ContactTagDetailDto,
        ContactTagDto,
        Guid,
        ContactTagGetListInput,
        ContactTagCreateInput,
        ContactTagUpdateInput>,
    IContactTagAppService
{
    protected override string GetListPolicyName { get; set; } = ChatPermissions.ContactTagPermission.GetList;
    protected override string GetPolicyName { get; set; } = ChatPermissions.ContactTagPermission.GetItem;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.ContactTagPermission.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.ContactTagPermission.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.ContactTagPermission.Delete;

    public ContactTagAppService(IRepository<ContactTag, Guid> repository) : base(repository)
    {
    }

    protected override async Task<IQueryable<ContactTag>> CreateFilteredQueryAsync(ContactTagGetListInput input)
    {
        return (await ReadOnlyRepository.GetQueryableAsync())
            .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Name.Contains(input.Keyword));
    }

    protected override Task CheckGetListPolicyAsync(ContactTagGetListInput input)
    {
        return CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(GetListPolicyName));
    }

    protected override async Task CheckGetPolicyAsync(Guid id)
    {
        var entity = await GetEntityByIdAsync(id);

        await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(GetPolicyName));
    }

    protected override async Task CheckCreateAsync(ContactTagCreateInput input)
    {
        Assert.If(await Repository.AnyAsync(x => x.OwnerId == input.OwnerId && x.Name.Equals(input.Name)), $"Already exists [{input.Name}] ");
        await base.CheckCreateAsync(input);
    }

    protected override Task CheckCreatePolicyAsync(ContactTagCreateInput input)
    {
        return CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(CreatePolicyName));
    }

    protected override async Task CheckUpdateAsync(Guid id, ContactTag entity, ContactTagUpdateInput input)
    {
        Assert.If(await Repository.AnyAsync(x => !x.Id.Equals(id) && x.OwnerId == entity.OwnerId && x.Name.Equals(input.Name)), $"Already exists [{input.Name}] name:{id}");
        await base.CheckUpdateAsync(id, entity, input);
    }

    protected override async Task CheckUpdatePolicyAsync(Guid id, ContactTagUpdateInput input)
    {
        var entity = await GetEntityByIdAsync(id);

        await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(UpdatePolicyName));
    }

    protected override async Task CheckDeletePolicyAsync(Guid id)
    {
        var entity = await GetEntityByIdAsync(id);

        await CheckPolicyForUserAsync(entity.OwnerId, () => CheckPolicyAsync(DeletePolicyName));
    }
}
