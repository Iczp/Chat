using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
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
                .WhereIf(input.MinCount.HasValue, x => x.Session != null && x.Session.UnitList.Count(x => !x.IsKilled) >= input.MinCount)
                .WhereIf(input.MaxCount.HasValue, x => x.Session != null && x.Session.UnitList.Count(x => !x.IsKilled) < input.MaxCount)
                .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                .WhereIf(input.IsForbiddenAll.HasValue, x => x.IsForbiddenAll == input.IsForbiddenAll)
                .WhereIf(input.MemberOwnerId.HasValue, x => x.Session != null && x.Session.UnitList.Any(d => d.OwnerId == input.MemberOwnerId))
                .WhereIf(input.ForbiddenMemberOwnerId.HasValue, x => x.RoomForbiddenMemberList.Any(d => d.OwnerId == input.ForbiddenMemberOwnerId && d.ExpireTime.HasValue && d.ExpireTime < DateTime.Now))
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword))

                ;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="query"></param>
        ///// <returns></returns>
        //protected override IQueryable<Room> ApplyDefaultSorting(IQueryable<Room> query)
        //{
        //    return query.OrderByDescending(x => x.Session.UnitList.Count(x => !x.IsKilled));
        //}

        protected override Task CheckDeleteAsync(Room entity)
        {
            var memberCount = entity.RoomMemberList.Count;

            Assert.If(memberCount != 0, $"Room's member count: memberCount");

            return base.CheckDeleteAsync(entity);
        }

        [HttpPost]
        public override async Task<RoomDetailDto> CreateAsync(RoomCreateInput input)
        {
            var members = await ChatObjectManager.GetManyAsync(input.ChatObjectIdList);

            var room = await RoomManager.CreateRoomAsync(new Room(GuidGenerator.Create(), input.Name, input.Code, input.Description, input.OwnerId), members);

            return await MapToDtoAsync(room);
        }

        [HttpPost]
        public override async Task<RoomDetailDto> UpdateAsync(Guid id, RoomUpdateInput input)
        {
            var room = await Repository.GetAsync(id);

            room.SetName(input.Name);

            room.Code = input.Code;

            room.Description = input.Description;

            await RoomManager.UpdateRoomAsync(room);

            return await MapToDtoAsync(room);
        }

        [HttpPost]
        public async Task<RoomDetailDto> CreateWithAllUserAsync(string roomName)
        {
            var idList = await ChatObjectManager.GetAllListAsync(ChatObjectTypes.Personal);

            var room = await RoomManager.CreateRoomAsync(new Room(GuidGenerator.Create(), roomName, roomName, "所有人", idList.FirstOrDefault()?.Id), idList);

            return await MapToDtoAsync(room);
        }

        protected Task<RoomDetailDto> MapToDtoAsync(Room room)
        {
            return Task.FromResult(ObjectMapper.Map<Room, RoomDetailDto>(room));
        }

        [HttpPost]
        public virtual async Task<int> JoinRoomAsync(Guid id, RoomJoinInput input)
        {
            var room = await Repository.GetAsync(id);

            var inviter = await ChatObjectManager.GetAsync(input.InviterId);

            var members = await ChatObjectManager.GetManyAsync(input.ChatObjectIdList);

            var addCount = await RoomManager.JoinRoomAsync(room, members, inviter, JoinWays.Invitation);

            return addCount;
        }
    }
}
