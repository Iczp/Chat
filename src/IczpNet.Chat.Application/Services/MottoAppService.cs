using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Mottos;
using IczpNet.Chat.Mottos.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.ChatObjects;

namespace IczpNet.Chat.MottoServices
{
    public class MottoAppService
        : CrudChatAppService<
            Motto,
            MottoDetailDto,
            MottoDto,
            Guid,
            MottoGetListInput,
            MottoCreateInput,
            MottoUpdateInput>,
        IMottoAppService
    {
        protected IChatObjectManager ChatObjectManager { get; }
        public MottoAppService(IRepository<Motto, Guid> repository,
            IChatObjectManager chatObjectManager) : base(repository)
        {
            ChatObjectManager = chatObjectManager;
        }

        protected override async Task<IQueryable<Motto>> CreateFilteredQueryAsync(MottoGetListInput input)
        {
            return (await ReadOnlyRepository.GetQueryableAsync())
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Title.Contains(input.Keyword));
        }

        protected override async Task SetCreateEntityAsync(Motto entity, MottoCreateInput input)
        {
            var owner = await ChatObjectManager.GetAsync(input.OwnerId);

            owner.SetMotto(entity);

            await base.SetCreateEntityAsync(entity, input);
        }
    }
}
