using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.FriendshipRequests;
using IczpNet.Chat.SessionSections.FriendshipRequests.Dtos;
using IczpNet.Chat.SessionSections.Sessions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices
{
    [RemoteService(false)]
    public class FriendshipRequestAppService
        : CrudChatAppService<
            FriendshipRequest,
            FriendshipRequestDetailDto,
            FriendshipRequestDto,
            Guid,
            FriendshipRequestGetListInput,
            FriendshipRequestCreateInput,
            FriendshipRequestUpdateInput>,
        IFriendshipRequestAppService
    {

        protected ISessionManager SessionManager { get; }
        public FriendshipRequestAppService(
            IRepository<FriendshipRequest, Guid> repository,
            ISessionManager sessionManager) : base(repository)
        {
            SessionManager = sessionManager;
        }

        protected override async Task<IQueryable<FriendshipRequest>> CreateFilteredQueryAsync(FriendshipRequestGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
                .WhereIf(input.IsHandled.HasValue, x => x.IsHandled == input.IsHandled)
                .WhereIf(input.IsAgreed.HasValue, x => x.IsAgreed == input.IsAgreed)
                .WhereIf(input.StartCreationTime.HasValue, x => x.CreationTime >= input.StartCreationTime)
                .WhereIf(input.StartCreationTime.HasValue, x => x.CreationTime < input.EndCreationTime)
                .WhereIf(input.StartHandlTime.HasValue, x => x.HandlTime >= input.StartHandlTime)
                .WhereIf(input.StartHandlTime.HasValue, x => x.HandlTime < input.EndHandlTime)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Message.Contains(input.Keyword) || x.HandlMessage.Contains(input.Keyword))
                ;
        }

        protected override FriendshipRequest MapToEntity(FriendshipRequestCreateInput createInput)
        {
            return new FriendshipRequest(createInput.OwnerId, createInput.DestinationId, createInput.Message);
        }

        public override async Task<FriendshipRequestDetailDto> CreateAsync(FriendshipRequestCreateInput input)
        {
            Assert.If(await SessionManager.IsFriendshipAsync(input.OwnerId, input.DestinationId), "Already a friend");

            return await base.CreateAsync(input);
        }

        [RemoteService(false)]
        public override Task<FriendshipRequestDetailDto> UpdateAsync(Guid id, FriendshipRequestUpdateInput input)
        {
            return base.UpdateAsync(id, input);
        }

        [RemoteService(false)]
        public override Task DeleteAsync(Guid id)
        {
            return base.DeleteAsync(id);
        }

        [RemoteService(false)]
        public override Task DeleteManyAsync(List<Guid> idList)
        {
            return base.DeleteManyAsync(idList);
        }

        [HttpPost]
        public Task<DateTime?> HandleRequestAsync(HandleRequestInput input)
        {
            return SessionManager.HandleRequestAsync(input.FriendshipRequestId, input.IsAgreed, input.HandleMessage);
        }
    }
}
