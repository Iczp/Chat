using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RoomSections.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageSections
{
    public class ChatObjectResolver : DomainService, IChatObjectResolver
    {
        protected IRepository<Room, Guid> RoomRepository { get; }
        public ChatObjectResolver(IRepository<Room, Guid> roomRepository)
        {
            RoomRepository = roomRepository;
        }
        public virtual async Task<List<ChatObject>> GetListAsync(Message message)
        {
            var result = new List<ChatObject>();

            switch (message.Channel)
            {
                case Enums.Channels.PrivateChannel:
                    return new List<ChatObject>() { message.Sender, message.Receiver };
                case Enums.Channels.RoomChannel:
                    await RoomRepository.GetAsync(message.ReceiverId.Value);

                    break;
                case Enums.Channels.SubscriptionChannel:
                    break;
                case Enums.Channels.ServiceChannel:
                    break;
                case Enums.Channels.SquareChannel:
                    break;
                case Enums.Channels.RobotChannel:
                    break;
                case Enums.Channels.ElectronicCommerceChannel:
                    break;
                default:
                    break;
            }
            return result;
        }

        public virtual async Task<List<Guid>> GetIdListAsync(Message message)
        {
            var result = new List<Guid>();
            switch (message.Channel)
            {
                case Enums.Channels.PrivateChannel:
                    return new List<Guid>() { message.Sender.Id, message.Receiver.Id };
                case Enums.Channels.RoomChannel:
                    var roomId = message.ReceiverId.Value;
                    var room = await RoomRepository.GetAsync(roomId);
                    return room.RoomMemberList.Select(x => x.OwnerId).Distinct().ToList();
                case Enums.Channels.SubscriptionChannel:


                    break;
                case Enums.Channels.ServiceChannel:
                    break;
                case Enums.Channels.SquareChannel:
                    break;
                case Enums.Channels.RobotChannel:
                    break;
                case Enums.Channels.ElectronicCommerceChannel:
                    break;
            }
            return result;
        }
    }
}
