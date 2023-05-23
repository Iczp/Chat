using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Words;
using IczpNet.Chat.Words.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.AbpCommons;

namespace IczpNet.Chat.Words
{
    public class WordAppService
        : CrudChatAppService<
            Word,
            WordDetailDto,
            WordDto,
            string,
            WordGetListInput,
            WordCreateInput,
            WordUpdateInput>,
        IWordAppService
    {
        public WordAppService(IRepository<Word, string> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<Word>> CreateFilteredQueryAsync(WordGetListInput input)
        {
            return (await ReadOnlyRepository.GetQueryableAsync())
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Id.Contains(input.Keyword));
        }


        protected override async Task CheckCreateAsync(WordCreateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => x.Id.Equals(input.Id)), $"Already exists [{typeof(Word)}] ");
            await base.CheckCreateAsync(input);
        }

        protected override async Task CheckUpdateAsync(string id, Word entity, WordUpdateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => x.Id.Equals(id) && x.Id.Equals(id)), $"Already exists [{typeof(Word)}] name:{id}");
            await base.CheckUpdateAsync(id, entity, input);
        }
    }
}
