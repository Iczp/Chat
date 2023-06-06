using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.EntryValues.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.AbpCommons;

namespace IczpNet.Chat.EntryValues
{
    public class EntryValueAppService
        : CrudChatAppService<
            EntryValue,
            EntryValueDetailDto,
            EntryValueDto,
            Guid,
            EntryValueGetListInput,
            EntryValueCreateInput,
            EntryValueUpdateInput>,
        IEntryValueAppService
    {
        public EntryValueAppService(IRepository<EntryValue, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<EntryValue>> CreateFilteredQueryAsync(EntryValueGetListInput input)
        {
            return (await ReadOnlyRepository.GetQueryableAsync())
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Value.Contains(input.Keyword));
        }


        protected override async Task CheckCreateAsync(EntryValueCreateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => x.Value.Equals(input.Value)), $"Already exists [{input.Value}] ");
            await base.CheckCreateAsync(input);
        }

        protected override async Task CheckUpdateAsync(Guid id, EntryValue entity, EntryValueUpdateInput input)
        {
            await base.CheckUpdateAsync(id, entity, input);
        }
    }
}
