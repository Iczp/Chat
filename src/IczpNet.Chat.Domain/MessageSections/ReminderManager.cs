using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using Volo.Abp.Domain.Services;
using System.Linq;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.MessageSections.MessageReminders;

namespace IczpNet.Chat.MessageSections
{
    public class ReminderManager : DomainService, IReminderManager
    {
        protected IChatObjectManager ChatObjectManager { get; }
        protected IRepository<MessageReminder> Repository { get; }

        public ReminderManager(
            IRepository<MessageReminder> repository, 
            IChatObjectManager chatObjectManager)
        {
            Repository = repository;
            ChatObjectManager = chatObjectManager;
        }

        /// <summary>
        /// 设置【@我】提醒
        /// </summary>
        /// <param name="message"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        public virtual async Task SetRemindAsync(Message message)
        {
            //@XXX
            if (message.MessageType != MessageTypes.Text)
            {
                return;
            }

            //Guid.TryParse(message.Receiver, out Guid roomId);

            var textContent = message.TextContentList.FirstOrDefault();

            var text = textContent.Text;

            var reg = new Regex("@([^ ]+) ?");

            //例如我想提取 @中的NAME值
            var match = reg.Match(text);

            var nameList = new List<string>();

            for (var i = 0; i < match.Groups.Count; i++)
            {
                string value = match.Groups[i].Value;

                if (!value.IsNullOrWhiteSpace())
                {
                    nameList.Add(value);
                }
            }
            if (!nameList.Any())
            {
                return;
            }
            if (nameList.IndexOf("所有人") != -1)
            {
                message.SetRemindAll();

                return;
            }
            var chatObjectIdList = await ChatObjectManager.GetIdListByNameAsync(nameList);

            if (!chatObjectIdList.Any())
            {
                return;
            }
            var roomId = message.Receiver;

            ////验证被@的人是否在群里
            //var memberChatObjectIdList = room.RoomMemberList.Where(x => chatObjectIdList.Contains(x.OwnerId)).Select(x => x.OwnerId).ToList();

            //if (memberChatObjectIdList.Any())
            //{
            //    message.SetRemindChatObject(memberChatObjectIdList);
            //}
        }
    }
}
