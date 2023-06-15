using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ContactTags.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.AbpCommons;
using Newtonsoft.Json.Linq;
using IczpNet.Chat.Permissions;

namespace IczpNet.Chat.ContactTags
{
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
        //protected override string GetPolicyName { get; set; } = ChatPermissions.EntryNamePermission.Default;
        //protected override string GetListPolicyName { get; set; } = ChatPermissions.EntryNamePermission.Default;
        //protected override string CreatePolicyName { get; set; } = ChatPermissions.EntryNamePermission.CreateAsync;
        //protected override string UpdatePolicyName { get; set; } = ChatPermissions.EntryNamePermission.Update;
        //protected override string DeletePolicyName { get; set; } = ChatPermissions.EntryNamePermission.Delete;

        public ContactTagAppService(IRepository<ContactTag, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<ContactTag>> CreateFilteredQueryAsync(ContactTagGetListInput input)
        {
            return (await ReadOnlyRepository.GetQueryableAsync())
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Name.Contains(input.Keyword));
        }


        protected override async Task CheckCreateAsync(ContactTagCreateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => x.Name.Equals(input.Name)), $"Already exists [{input.Name}] ");
            await base.CheckCreateAsync(input);
        }

        protected override async Task CheckUpdateAsync(Guid id, ContactTag entity, ContactTagUpdateInput input)
        {
            //Assert.If(await Repository.AnyAsync(x => x.Id.Equals(id) && x.Id.Equals(id)), $"Already exists [{input.Name}] name:{id}");
            await base.CheckUpdateAsync(id, entity, input);
        }
    }
}
