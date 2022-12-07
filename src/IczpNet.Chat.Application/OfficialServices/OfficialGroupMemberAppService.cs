using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.OfficialSections.OfficialGroupMembers;
using IczpNet.Chat.OfficialSections.OfficialGroupMembers.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.OfficialServices
{
    public class OfficialGroupMemberAppService
        : CrudChatAppService<
            OfficialGroupMember,
            OfficialGroupMemberDetailDto,
            OfficialGroupMemberDto,
            Guid,
            OfficialGroupMemberGetListInput,
            OfficialGroupMemberCreateInput,
            OfficialGroupMemberUpdateInput>,
        IOfficialGroupMemberAppService
    {
        public OfficialGroupMemberAppService(IRepository<OfficialGroupMember, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<OfficialGroupMember>> CreateFilteredQueryAsync(OfficialGroupMemberGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(input.OfficialGroupId.HasValue, x => x.OfficialGroupId == input.OfficialGroupId)
                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                ;
        }
    }
}
