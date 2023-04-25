using IczpNet.Chat.Management.BaseAppServices;
using IczpNet.Chat.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Encryption;
using Volo.Abp.SimpleStateChecking;

namespace Rctea.IM.Tools
{
    /// <summary>
    /// 数据种子
    /// </summary>
    [Route($"Api/[Controller]/[Action]")]
    public class ToolManagementAppService : ChatManagementAppService
    {
        protected IDataSeeder DataSeeder { get; }
        protected PermissionManagementOptions Options { get; }
        protected IPermissionManager PermissionManager { get; }
        protected IPermissionDefinitionManager PermissionDefinitionManager { get; }
        protected ISimpleStateCheckerManager<PermissionDefinition> SimpleStateCheckerManager { get; }
        protected IStringEncryptionService StringEncryptionService { get; }

        public ToolManagementAppService(IDataSeeder dataSeeder,
            IPermissionManager permissionManager,
            IPermissionDefinitionManager permissionDefinitionManager,
            IOptions<PermissionManagementOptions> options,
            ISimpleStateCheckerManager<PermissionDefinition> simpleStateCheckerManager,
            IStringEncryptionService stringEncryptionService)
        {
            DataSeeder = dataSeeder;
            Options = options.Value;
            PermissionManager = permissionManager;
            PermissionDefinitionManager = permissionDefinitionManager;
            SimpleStateCheckerManager = simpleStateCheckerManager;
            StringEncryptionService = stringEncryptionService;
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

            var groups = (await PermissionDefinitionManager.GetGroupsAsync()).Where(x => x.Name == SessionPermissionDefinitionConsts.GroupName);

            foreach (var group in groups)
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

        [HttpPost]
        public virtual string ToMD5(string input)
        {
            var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(input));

            string hashString = string.Join(string.Empty, hashBytes.Select(x => x.ToString("X2")));

            return hashString;
        }

        [HttpPost]
        public virtual Guid ToGuid(string input)
        {
            //32位大写
            var str = ToMD5(input);

            var ret = new Guid(str);

            return ret;
        }
    }
}
