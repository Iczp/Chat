using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.OfficialSections.OfficialGroupMembers;
using IczpNet.Chat.OfficialSections.OfficialGroups;
using IczpNet.Chat.OfficialSections.OfficialGroups.Dtos;
using IczpNet.Chat.OfficialSections.Officials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.OfficialServices
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
        protected IRepository<Official, Guid> OfficialRepository { get; }
        protected IRepository<ChatObject, Guid> ChatObjectRepository { get; }
        protected IChatObjectManager ChatObjectManager { get; }
        public OfficialGroupAppService(
            IRepository<OfficialGroup, Guid> repository,
            IChatObjectManager chatObjectManager,
            IRepository<Official, Guid> officialRepository,
            IRepository<ChatObject, Guid> chatObjectRepository) : base(repository)
        {
            ChatObjectManager = chatObjectManager;
            OfficialRepository = officialRepository;
            ChatObjectRepository = chatObjectRepository;
        }

        protected override async Task<IQueryable<OfficialGroup>> CreateFilteredQueryAsync(OfficialGroupGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(input.OfficialId.HasValue, x => x.OfficialId == input.OfficialId)
                .WhereIf(input.IsPublic.HasValue, x => x.IsPublic == input.IsPublic)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword) || x.Description.Contains(input.Keyword))

                ;
        }

        protected override OfficialGroup MapToEntity(OfficialGroupCreateInput createInput)
        {
            var officialGroupId = GuidGenerator.Create();

            var officialGroupMember = createInput.ChatObjectIdList.Select(x => new OfficialGroupMember(GuidGenerator.Create(), officialGroupId, x)).ToList();

            return new OfficialGroup(officialGroupId, createInput.OfficialId, createInput.Name, createInput.Description, true, officialGroupMember);
        }

        protected override async Task CheckCreateAsync(OfficialGroupCreateInput input)
        {
            Assert.If(!await OfficialRepository.AnyAsync(x => x.Id.Equals(input.OfficialId)), $"Not such Entity[Official] by id:{input.OfficialId}");

            foreach (var checkObjectId in input.ChatObjectIdList)
            {
                Assert.If(!await ChatObjectRepository.AnyAsync(x => x.Id.Equals(checkObjectId)), $"Not such Entity[ChatObject] by id:{checkObjectId}");
            }
            await base.CheckCreateAsync(input);
        }
    }
}
