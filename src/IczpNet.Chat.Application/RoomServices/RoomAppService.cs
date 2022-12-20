using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;



namespace IczpNet.Chat.RoomServices
{
    public class RoomAppService
        : CrudChatAppService<
            Room,
            RoomDetailDto,
            RoomDto,
            Guid,
            RoomGetListInput,
            RoomCreateInput,
            RoomUpdateInput>,
        IRoomAppService
    {

        //protected IRepository<ChatObject, Guid> ChatObjectRepository { get; } 

        protected IChatObjectManager ChatObjectManager { get; }
        protected IRoomManager RoomManager { get; }

        public RoomAppService(
            IRepository<Room, Guid> repository,
            IChatObjectManager chatObjectManager,
            IRoomManager roomManager) : base(repository)
        {
            ChatObjectManager = chatObjectManager;
            RoomManager = roomManager;
        }

        protected override async Task<IQueryable<Room>> CreateFilteredQueryAsync(RoomGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                .WhereIf(input.MinCount.HasValue, x => x.RoomMemberList.Count >= input.MinCount)
                .WhereIf(input.MaxCount.HasValue, x => x.RoomMemberList.Count < input.MaxCount)
                .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                .WhereIf(input.IsForbiddenAll.HasValue, x => x.IsForbiddenAll == input.IsForbiddenAll)
                .WhereIf(input.MemberOwnerId.HasValue, x => x.RoomMemberList.Any(d => d.OwnerId == input.MemberOwnerId))
                .WhereIf(input.ForbiddenMemberOwnerId.HasValue, x => x.RoomForbiddenMemberList.Any(d => d.OwnerId == input.ForbiddenMemberOwnerId && d.ExpireTime.HasValue && d.ExpireTime < DateTime.Now))
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword))

                ;
        }



        protected override Task CheckDeleteAsync(Room entity)
        {
            var memberCount = entity.RoomMemberList.Count;

            Assert.If(memberCount != 0, $"Room's member count: memberCount");

            return base.CheckDeleteAsync(entity);
        }

        [HttpPost]
        public override async Task<RoomDetailDto> CreateAsync(RoomCreateInput input)
        {
            var room = await RoomManager.CreateRoomAsync(new Room(GuidGenerator.Create(), input.Name, input.Code, input.Description, input.OwnerId), input.ChatObjectIdList);

            return ObjectMapper.Map<Room, RoomDetailDto>(room);
        }
    }
}
