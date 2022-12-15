using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RoomSections.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageSections
{
    public class MessageChatObjectResolver : DomainService, IMessageChatObjectResolver
    {

        protected IRepository<Room, Guid> RoomRepository { get; }
        public MessageChatObjectResolver(IRepository<Room, Guid> roomRepository)
        {
            RoomRepository = roomRepository;
        }
        public virtual async Task<List<ChatObject>> GetChatObjectListAsync(Message message)
        {
            var result = new List<ChatObject>();

            switch (message.MessageChannel)
            {
                case Enums.MessageChannels.PersonalToPersonal:
                    return new List<ChatObject>() { message.Sender, message.Receiver };
                case Enums.MessageChannels.RoomChannel:
                    await RoomRepository.GetAsync(message.ReceiverId.Value);

                    break;
                case Enums.MessageChannels.SubscriptionChannel:
                    break;
                case Enums.MessageChannels.ServiceChannel:
                    break;
                case Enums.MessageChannels.SquareChannel:
                    break;
                case Enums.MessageChannels.RobotChannel:
                    break;
                case Enums.MessageChannels.ElectronicCommerceChannel:
                    break;
                default:
                    break;
            }
            return result;
        }

        public virtual async Task<IEnumerable<Guid>> GetChatObjectIdListAsync(Message message)
        {
            var result = new List<Guid>();
            switch (message.MessageChannel)
            {
                case Enums.MessageChannels.PersonalToPersonal:
                    return new List<Guid>() { message.Sender.Id, message.Receiver.Id };
                case Enums.MessageChannels.RoomChannel:
                    var roomId = message.ReceiverId.Value;
                    var room = await RoomRepository.GetAsync(roomId);
                    return room.RoomMemberList.Select(x => x.OwnerId).Distinct().ToList();
                case Enums.MessageChannels.SubscriptionChannel:


                    break;
                case Enums.MessageChannels.ServiceChannel:
                    break;
                case Enums.MessageChannels.SquareChannel:
                    break;
                case Enums.MessageChannels.RobotChannel:
                    break;
                case Enums.MessageChannels.ElectronicCommerceChannel:
                    break;
            }
            return result;
        }
    }
}
