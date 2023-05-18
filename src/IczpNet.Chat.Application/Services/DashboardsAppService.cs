using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Dashboards;
using IczpNet.Chat.Dashboards.Dtos;
using IczpNet.Chat.Favorites;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.MessageReminders;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionRequests;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnitOrganizations;
using IczpNet.Chat.SessionSections.SessionUnitRoles;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnitTags;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Services
{
    public class DashboardsAppService : ChatAppService, IDashboardsAppService
    {
        protected IChatObjectRepository ChatObjectRepository { get; }
        protected ISessionRepository SessionRepository { get; }
        protected ISessionUnitRepository SessionUnitRepository { get; }
        protected IMessageRepository MessageRepository { get; }
        protected IRepository<ReadedRecorder> ReadedRecorderRepository { get; }
        protected IRepository<OpenedRecorder> OpenedRecorderRepository { get; }
        protected IRepository<Follow> FollowRepository { get; }
        protected IRepository<SessionRequest, Guid> SessionRequestRepository { get; }
        protected IRepository<SessionOrganization, long> SessionOrganizationRepository { get; }
        protected IRepository<SessionRole, Guid> SessionRoleRepository { get; }
        protected IRepository<SessionTag, Guid> SessionTagRepository { get; }
        protected IRepository<SessionUnitTag> SessionUnitTagRepository { get; }
        protected IRepository<SessionUnitRole> SessionUnitRoleRepository { get; }
        protected IRepository<SessionUnitOrganization> SessionUnitOrganizationRepository { get; }
        protected IRepository<MessageReminder> MessageReminderRepository { get; }
        protected IRepository<Favorite> FavoriteRepository { get; }

        public DashboardsAppService(
            IChatObjectRepository chatObjectRepository,
            IMessageRepository messageRepository,
            ISessionRepository sessionRepository,
            ISessionUnitRepository sessionUnitRepository,
            IRepository<ReadedRecorder> readedRecorderRepository,
            IRepository<OpenedRecorder> openedRecorderRepository,
            IRepository<Follow> followRepository,
            IRepository<SessionRequest, Guid> sessionRequestRepository,
            IRepository<SessionOrganization, long> sessionOrganizationRepository,
            IRepository<SessionRole, Guid> sessionRoleRepository,
            IRepository<SessionTag, Guid> sessionTagRepository,
            IRepository<SessionUnitTag> sessionUnitTagRepository,
            IRepository<SessionUnitRole> sessionUnitRoleRepository,
            IRepository<SessionUnitOrganization> sessionUnitOrganizationRepository,
            IRepository<MessageReminder> messageReminderRepository,
            IRepository<Favorite> favoriteRepository)
        {
            ChatObjectRepository = chatObjectRepository;
            MessageRepository = messageRepository;
            SessionRepository = sessionRepository;
            SessionUnitRepository = sessionUnitRepository;
            ReadedRecorderRepository = readedRecorderRepository;
            OpenedRecorderRepository = openedRecorderRepository;
            FollowRepository = followRepository;
            SessionRequestRepository = sessionRequestRepository;
            SessionOrganizationRepository = sessionOrganizationRepository;
            SessionRoleRepository = sessionRoleRepository;
            SessionTagRepository = sessionTagRepository;
            SessionUnitTagRepository = sessionUnitTagRepository;
            SessionUnitRoleRepository = sessionUnitRoleRepository;
            SessionUnitOrganizationRepository = sessionUnitOrganizationRepository;
            MessageReminderRepository = messageReminderRepository;
            FavoriteRepository = favoriteRepository;
        }

        [HttpGet]
        [UnitOfWork(true, IsolationLevel.ReadUncommitted)]
        public async Task<DashboardsDto> GetProfileAsync()
        {
            return new DashboardsDto()
            {
                Now = Clock.Now,
                ChatObjectCount = await ChatObjectRepository.GetCountAsync(),
                MessageCount = await MessageRepository.GetCountAsync(),
                SessionCount = await SessionRepository.GetCountAsync(),
                SessionUnitCount = await SessionUnitRepository.GetCountAsync(),
                ReadedRecorderCount = await ReadedRecorderRepository.GetCountAsync(),
                OpenedRecorderCount = await OpenedRecorderRepository.GetCountAsync(),
                FollowCount = await FollowRepository.GetCountAsync(),
                SessionRequestCount = await SessionRequestRepository.GetCountAsync(),
                SessionOrganizationCount = await SessionOrganizationRepository.GetCountAsync(),
                SessionRoleCount = await SessionRoleRepository.GetCountAsync(),
                SessionTagCount = await SessionTagRepository.GetCountAsync(),
                SessionUnitTagCount = await SessionUnitTagRepository.GetCountAsync(),
                SessionUnitRoleCount = await SessionUnitRoleRepository.GetCountAsync(),
                SessionUnitOrganizationCount = await SessionUnitOrganizationRepository.GetCountAsync(),
                MessageReminderCount = await MessageReminderRepository.GetCountAsync(),
                FavoriteCount = await FavoriteRepository.GetCountAsync(),
            };
        }
    }
}
