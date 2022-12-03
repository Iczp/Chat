using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.OfficialSections.OfficialGroups;
using IczpNet.Chat.OfficialSections.OfficialGroups.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
{
    public class OfficialGroupAppService
        : CrudChatAppService<
            OfficialGroup,
            OfficialGroupDetailDto,
            OfficialGroupDto,
            Guid,
            OfficialGroupGetListInput,
            OfficialGroupCreateInput,
            OfficialGroupUpdateInput>,
        IOfficialGroupAppService
    {
        public OfficialGroupAppService(IRepository<OfficialGroup, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<OfficialGroup>> CreateFilteredQueryAsync(OfficialGroupGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                //.WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                //.WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                ;
        }
    }
}
