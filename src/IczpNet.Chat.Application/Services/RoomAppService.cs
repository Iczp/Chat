using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
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
        public RoomAppService(IRepository<Room, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<Room>> CreateFilteredQueryAsync(RoomGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                //.WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                ;
        }
    }
}
