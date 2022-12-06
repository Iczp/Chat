using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
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

        public RoomAppService(
            IRepository<Room, Guid> repository,
            IChatObjectManager chatObjectManager) : base(repository)
        {
            ChatObjectManager = chatObjectManager;
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

        protected override async Task<Room> MapToEntityAsync(RoomCreateInput createInput)
        {
            var roomMemberList = new List<RoomMember>();

            var roomId = GuidGenerator.Create();

            foreach (var memberId in createInput.ChatObjectIdList)
            {
                var chatObject = await ChatObjectManager.GetAsync(memberId);

                Assert.If(!await ChatObjectManager.IsAllowJoinRoomMemnerAsync(chatObject.ObjectType), $"Not allowed to join the room,ObjectType:{chatObject.ObjectType},Id:{memberId}");

                roomMemberList.Add(new RoomMember(GuidGenerator.Create(), roomId, chatObject.Id, CurrentChatObject.GetId()));
            }

            return new Room(roomId, createInput.Name, createInput.Code, createInput.Description, roomMemberList, createInput.OwnerId);
        }

        protected override Task CheckDeleteAsync(Room entity)
        {
            var memberCount = entity.RoomMemberList.Count;

            Assert.If(memberCount != 0, $"Room's member count: memberCount");

            return base.CheckDeleteAsync(entity);
        }
    }
}
