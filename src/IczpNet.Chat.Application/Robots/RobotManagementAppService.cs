using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.Robots.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IczpNet.Chat.Robots
{
    public class RobotManagementAppService : ChatManagementAppService, IRobotManagementAppService
    {

        protected override string CreatePolicyName { get; set; } = ChatPermissions.RobotManagementPermission.Create;
        protected override string UpdatePolicyName { get; set; } = ChatPermissions.RobotManagementPermission.Update;

        protected IChatObjectRepository Repository { get; }
        protected IChatObjectManager ChatObjectManager { get; }

        public RobotManagementAppService(
            IChatObjectRepository repository, 
            IChatObjectManager chatObjectManager)
        {
            Repository = repository;
            ChatObjectManager = chatObjectManager;
        }

        [HttpPost]
        [Authorize(ChatPermissions.RobotManagementPermission.Create)]
        public async Task<RobotDto> CreateAsync(RobotCreateInput input)
        {
            //await CheckPolicyAsync(CreatePolicyName);
            await Task.CompletedTask;
            throw new System.NotImplementedException();
        }

        [HttpPost]
        [Authorize(ChatPermissions.RobotManagementPermission.Update)]
        public async Task<RobotDto> UpdateAsync(long id, RobotUpdateInput input)
        {
            //await CheckPolicyAsync(UpdatePolicyName);
            await Task.CompletedTask;
            throw new System.NotImplementedException();
        }
    }
}
