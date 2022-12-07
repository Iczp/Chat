using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.OfficialSections.OfficialMembers;
using IczpNet.Chat.OfficialSections.OfficialMembers.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.OfficialServices
{
    public class OfficialMemberAppService
        : CrudChatAppService<
            OfficialMember,
            OfficialMemberDetailDto,
            OfficialMemberDto,
            Guid,
            OfficialMemberGetListInput,
            OfficialMemberCreateInput,
            OfficialMemberUpdateInput>,
        IOfficialMemberAppService
    {
        public OfficialMemberAppService(IRepository<OfficialMember, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<OfficialMember>> CreateFilteredQueryAsync(OfficialMemberGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(input.OfficialId.HasValue, x => x.OfficialId == input.OfficialId)
                //.WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                //.WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                ;
        }
    }
}
