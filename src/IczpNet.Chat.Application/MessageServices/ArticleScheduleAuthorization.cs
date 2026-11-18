using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Threading;

namespace IczpNet.Chat.MessageServices;

public class ArticleScheduleAuthorization(
    IIdentityUserRepository users,
    IChatObjectManager accounts,
    IPermissionChecker permissions,
    ICancellationTokenProvider cancellationTokenProvider) : IArticleScheduleAuthorization, ITransientDependency
{
    public async Task<ClaimsPrincipal> AuthorizeAsync(Guid userId, long ownerId)
    {
        var user = await users.FindAsync(userId, cancellationToken: cancellationTokenProvider.Token);
        if (user == null || !user.IsActive) throw new UserFriendlyException("任务创建者已停用或删除。");
        var claims = new List<Claim> { new(AbpClaimTypes.UserId, userId.ToString()) };
        claims.AddRange((await users.GetRoleNamesAsync(user.Id, cancellationTokenProvider.Token)).Select(role => new Claim(AbpClaimTypes.Role, role)));
        if (user.TenantId.HasValue) claims.Add(new Claim(AbpClaimTypes.TenantId, user.TenantId.Value.ToString()));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "ArticleSchedule"));
        if (!(await accounts.GetIdListByUserIdAsync(userId)).Contains(ownerId) &&
            !await permissions.IsGrantedAsync(principal, ArticleMessagePermissions.Manage))
            throw new UserFriendlyException("创建者已无权操作该发送主体。");
        return principal;
    }
}
