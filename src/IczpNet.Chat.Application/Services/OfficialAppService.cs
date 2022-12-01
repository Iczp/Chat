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

                ;
        }
    }
}
