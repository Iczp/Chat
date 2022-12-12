using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.GlobalPermissions;
using IczpNet.Chat.RoomSections.Rooms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Encryption;
using Volo.Abp.SimpleStateChecking;
using Volo.Abp.Threading;

namespace Rctea.IM.Tools
{
    /// <summary>
    /// 数据种子
    /// </summary>
    [Route($"Api/[Controller]/[Action]")]
    public class ToolAppService : ChatAppService
    {
        protected IDataSeeder DataSeeder { get; }
        protected PermissionManagementOptions Options { get; }
        protected IPermissionManager PermissionManager { get; }
        protected IPermissionDefinitionManager PermissionDefinitionManager { get; }
        protected ISimpleStateCheckerManager<PermissionDefinition> SimpleStateCheckerManager { get; }
        protected IStringEncryptionService StringEncryptionService { get; }

        protected IRepository<Room, Guid> RoomRepository { get; }
        public ToolAppService(IDataSeeder dataSeeder,
            IPermissionManager permissionManager,
            IPermissionDefinitionManager permissionDefinitionManager,
            IOptions<PermissionManagementOptions> options,
            ISimpleStateCheckerManager<PermissionDefinition> simpleStateCheckerManager,
            IStringEncryptionService stringEncryptionService,
            IRepository<Room, Guid> roomRepository)
        {
            DataSeeder = dataSeeder;
            Options = options.Value;
            PermissionManager = permissionManager;
            PermissionDefinitionManager = permissionDefinitionManager;
            SimpleStateCheckerManager = simpleStateCheckerManager;
            StringEncryptionService = stringEncryptionService;
            RoomRepository = roomRepository;
        }

        [HttpPost]
        public async Task<int> UpdateRoomStateAsync()
        {
            using (DataFilter.Disable<ISoftDelete>())
            {

                var roomIdList = new List<Guid>();

                if (!roomIdList.Any())
                {
                    return 0;
                }

                var roomIdQuery = (await RoomRepository.GetQueryableAsync())
                       .Where(x => roomIdList.Contains(x.Id))
                       .Where(x => !x.IsDeleted)
                       .Select(x => x.Id)
                       ;

                var deleteCount = await AsyncExecuter.CountAsync(roomIdQuery);

                await RoomRepository.DeleteAsync(x => roomIdQuery.Contains(x.Id));

                return deleteCount;
            }
        }

        [HttpPost]
        public string Encrypt(string value)
        {
            // To enrcypt a value
            return StringEncryptionService.Encrypt(value);
        }
        [HttpPost]
        public string Decrpyt(string value)
        {
            // To decrypt a value
            return StringEncryptionService.Decrypt(value);
        }
        /// <summary>
        /// 数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<DateTime> DataSeedAsync()
        {
            await DataSeeder.SeedAsync();
            return Clock.Now;
        }
        [HttpPost]
        public virtual async Task<GetPermissionListResultDto> GetAsync(string providerName, string providerKey)
        {
            var result = new GetPermissionListResultDto
            {
                EntityDisplayName = providerKey,
                Groups = new List<PermissionGroupDto>()
            };

            var multiTenancySide = CurrentTenant.GetMultiTenancySide();

            foreach (var group in PermissionDefinitionManager.GetGroups().Where(x => x.Name == GlobalPermissionConsts.GroupName))
            {
                var groupDto = new PermissionGroupDto
                {
                    Name = group.Name,
                    DisplayName = group.DisplayName.Localize(StringLocalizerFactory),
                    Permissions = new List<PermissionGrantInfoDto>()
                };

                var neededCheckPermissions = new List<PermissionDefinition>();

                foreach (var permission in group.GetPermissionsWithChildren()
                                                .Where(x => x.IsEnabled)
                                                .Where(x => !x.Providers.Any() || x.Providers.Contains(providerName))
                                                .Where(x => x.MultiTenancySide.HasFlag(multiTenancySide)))
                {
                    if (await SimpleStateCheckerManager.IsEnabledAsync(permission))
                    {
                        neededCheckPermissions.Add(permission);
                    }
                }

                if (!neededCheckPermissions.Any())
                {
                    continue;
                }

                var grantInfoDtos = neededCheckPermissions.Select(x => new PermissionGrantInfoDto
                {
                    Name = x.Name,
                    DisplayName = x.DisplayName.Localize(StringLocalizerFactory),
                    ParentName = x.Parent?.Name,
                    AllowedProviders = x.Providers,
                    GrantedProviders = new List<ProviderInfoDto>()
                }).ToList();

                var multipleGrantInfo = await PermissionManager.GetAsync(neededCheckPermissions.Select(x => x.Name).ToArray(), providerName, providerKey);

                foreach (var grantInfo in multipleGrantInfo.Result)
                {
                    var grantInfoDto = grantInfoDtos.First(x => x.Name == grantInfo.Name);

                    grantInfoDto.IsGranted = grantInfo.IsGranted;

                    foreach (var provider in grantInfo.Providers)
                    {
                        grantInfoDto.GrantedProviders.Add(new ProviderInfoDto
                        {
                            ProviderName = provider.Name,
                            ProviderKey = provider.Key,
                        });
                    }

                    groupDto.Permissions.Add(grantInfoDto);
                }

                if (groupDto.Permissions.Any())
                {
                    result.Groups.Add(groupDto);
                }
            }

            return result;
        }
    }
}
