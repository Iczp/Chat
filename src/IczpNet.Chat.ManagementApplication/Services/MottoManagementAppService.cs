using IczpNet.Chat.Mottos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.ChatObjects;
using Volo.Abp;
using IczpNet.Chat.Management.BaseAppServices;
using IczpNet.Chat.Management.Mottos.Dtos;
using IczpNet.Chat.Management.Mottos;

namespace IczpNet.Chat.Management.MottoServices
{
    public class MottoManagementAppService
        : CrudChatManagementAppService<
            Motto,
            MottoDetailDto,
            MottoDto,
            Guid,
            MottoAdminGetListInput,
            MottoAdminCreateInput,
            MottoUpdateInput>,
        IMottoManagementAppService
    {
        protected IChatObjectManager ChatObjectManager { get; }
        public MottoManagementAppService(IRepository<Motto, Guid> repository,
            IChatObjectManager chatObjectManager) : base(repository)
        {
            ChatObjectManager = chatObjectManager;
        }

        protected override async Task<IQueryable<Motto>> CreateFilteredQueryAsync(MottoAdminGetListInput input)
        {
            return (await ReadOnlyRepository.GetQueryableAsync())
                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Title.Contains(input.Keyword));
        }

        protected override async Task SetCreateEntityAsync(Motto entity, MottoAdminCreateInput input)
        {
            var owner = await ChatObjectManager.GetAsync(input.OwnerId);

            owner.SetMotto(entity);

            await base.SetCreateEntityAsync(entity, input);
        }

        [RemoteService(false)]
        public override Task<MottoDetailDto> CreateAsync(MottoAdminCreateInput input) => base.CreateAsync(input);

        [RemoteService(false)]
        public override Task<MottoDetailDto> UpdateAsync(Guid id, MottoUpdateInput input) => base.UpdateAsync(id, input);
    }
}
