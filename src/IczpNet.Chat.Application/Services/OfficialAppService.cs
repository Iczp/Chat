using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.OfficialSections.Officials;
using IczpNet.Chat.OfficialSections.Officials.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
{
    public class OfficialAppService
        : CrudChatAppService<
            Official,
            OfficialDetailDto,
            OfficialDto,
            Guid,
            OfficialGetListInput,
            OfficialCreateInput,
            OfficialUpdateInput>,
        IOfficialAppService
    {
        public OfficialAppService(IRepository<Official, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<Official>> CreateFilteredQueryAsync(OfficialGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Name.Contains(input.Keyword) || x.Code.Contains(input.Keyword));
            ;
        }

        protected override Official MapToEntity(OfficialCreateInput createInput)
        {
            var entity = base.MapToEntity(createInput);
            entity.SetName(createInput.Name);
            return entity;
        }

        protected override async Task CheckCreateAsync(OfficialCreateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => x.Name.Equals(input.Name)), $"Already exists [{typeof(Official)}] name:{input.Name}");
            await base.CheckCreateAsync(input);
        }

        protected override async Task CheckUpdateAsync(Guid id, Official entity, OfficialUpdateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => !x.Id.Equals(id) && x.Name.Equals(input.Name)), $"Already exists [{typeof(Official)}] name:{input.Name}");
            await base.CheckUpdateAsync(id, entity, input);
        }

        //protected override Task MapToEntityAsync(OfficialUpdateInput updateInput, Official entity)
        //{
        //    entity.SetName(updateInput.Name);
        //    return base.MapToEntityAsync(updateInput, entity);
        //}
    }
}
