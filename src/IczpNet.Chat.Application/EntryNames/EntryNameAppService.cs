using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.EntryNames.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.AbpCommons;

namespace IczpNet.Chat.EntryNames
{
    public class EntryNameAppService
        : CrudChatAppService<
            EntryName,
            EntryNameDetailDto,
            EntryNameDto,
            Guid,
            EntryNameGetListInput,
            EntryNameCreateInput,
            EntryNameUpdateInput>,
        IEntryNameAppService
    {
        public EntryNameAppService(IRepository<EntryName, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<EntryName>> CreateFilteredQueryAsync(EntryNameGetListInput input)
        {
            return (await ReadOnlyRepository.GetQueryableAsync())
                .WhereIf(input.IsChoice.HasValue, x => x.IsChoice == input.IsChoice)
                .WhereIf(input.IsPublic.HasValue, x => x.IsPublic == input.IsPublic)
                .WhereIf(input.IsStatic.HasValue, x => x.IsStatic == input.IsStatic)
                .WhereIf(input.IsUniqued.HasValue, x => x.IsUniqued == input.IsUniqued)
                
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Name.Contains(input.Keyword));
        }


        protected override async Task CheckCreateAsync(EntryNameCreateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => x.Name.Equals(input.Name)), $"Already exists [{input.Name}] ");
            await base.CheckCreateAsync(input);
        }

        protected override async Task CheckUpdateAsync(Guid id, EntryName entity, EntryNameUpdateInput input)
        {
            await base.CheckUpdateAsync(id, entity, input);
        }
    }
}
