using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Locations.Dto;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.TextTemplates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Caching;
using Volo.Abp.Users;

namespace IczpNet.Chat.Locations;

/// <summary>
/// 共享位置
/// </summary>
////[AbpAuthorize]
public class LocationAppService : ChatAppService, ILocationAppService
{

    /// <summary>
    /// ExpiredSeconds
    /// </summary>
    public const int ExpiredSeconds = 60;

    /// <summary>
    /// Guid:appUserId
    /// </summary>
    protected IDistributedCache<UserLocationCacheItem, Guid> UserLocationCache { get; }

    /// <summary>
    /// Guid:sessionId
    /// </summary>
    protected IDistributedCache<List<SessionLocationCacheItem>, Guid> SessionCache { get; }
    protected IMessageSender MessageSender { get; }
    protected ISessionUnitRepository SessionUnitRepository { get; }

    public LocationAppService(
        IMessageSender messageSender,
        IDistributedCache<UserLocationCacheItem, Guid> userLocationCache,
        IDistributedCache<List<SessionLocationCacheItem>, Guid> sessionCache,
        ISessionUnitRepository sessionUnitRepository)
    {
        MessageSender = messageSender;
        UserLocationCache = userLocationCache;
        SessionCache = sessionCache;
        SessionUnitRepository = sessionUnitRepository;
    }

    /// <summary>
    /// 共享位置
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ShareLocationOutput> SharingAsync(ShareLocationInput input)
    {
        var currentUserId = input.UserLocation.UserId.Value; //CurrentUser.GetId();

        var expiredTime = Clock.Now.AddSeconds(-ExpiredSeconds);

        var distributedCacheEntryOptions = new DistributedCacheEntryOptions()
        {
            SlidingExpiration = TimeSpan.FromSeconds(ExpiredSeconds)
        };

        var userLocation = input.UserLocation;

        userLocation.ActiveTime = Clock.Now;

        await UserLocationCache.SetAsync(currentUserId, input.UserLocation, distributedCacheEntryOptions);

        foreach (var sessionUnitId in input.SessionUnitIdList)
        {
            var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

            // 更新到会话

            var joinType = JoinTypes.Update;

            var sessionId = sessionUnit.SessionId.Value;

            var sessionLocationList = await SessionCache.GetAsync(sessionId);

            if (sessionLocationList == null)
            {
                joinType = JoinTypes.Creator;
                sessionLocationList = new List<SessionLocationCacheItem>();
            }

            var sessionLocationItem = sessionLocationList.FirstOrDefault(d => d.SessionUnitId == sessionUnitId);

            if (sessionLocationItem != null)
            {
                //删除
                sessionLocationList.Remove(sessionLocationItem);
            }
            else
            {
                if (joinType != JoinTypes.Creator)
                {
                    joinType = JoinTypes.Join;
                }
            }

            //删除不活跃用户
            sessionLocationList
                .Where(u => u.ActiveTime < expiredTime)
                .ToList()
                .ForEach(item => sessionLocationList.Remove(item))
                ;

            //加入到分组
            sessionLocationList.Add(new SessionLocationCacheItem()
            {
                SessionUnitId = sessionUnit.Id,
                ChatObjectId = sessionUnit.OwnerId,
                UserId = currentUserId,
                ActiveTime = Clock.Now
            });

            // 更新缓存
            await SessionCache.SetAsync(sessionId, sessionLocationList, distributedCacheEntryOptions);

            if (joinType == JoinTypes.Creator || joinType == JoinTypes.Join)
            {
                //发送消息
                var sendTask = await MessageSender.SendCmdAsync(sessionUnit, new MessageInput<CmdContentInfo>
                {
                    Content = new CmdContentInfo()
                    {
                        Cmd = joinType == JoinTypes.Creator ? MessageKeyNames.CreateShareLocation : MessageKeyNames.JoinShareLocation,
                        Text = $"{new SessionUnitTextTemplate(sessionUnit)} 正在共享位置"
                    },
                });
            }
        }

        return new ShareLocationOutput()
        {

        };
    }

    /// <summary>
    /// 获取媒体位置信息
    /// </summary>
    /// <param name="sessionUnitId">会话单元</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<UserLocationDto>> GetListAsync(Guid sessionUnitId)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        var list = await SessionCache.GetAsync(sessionUnit.SessionId.Value);

        if (list == null)
        {
            return new PagedResultDto<UserLocationDto>();
        }

        var items = new List<UserLocationDto>();

        foreach (var item in list)
        {
            var su = await SessionUnitManager.GetAsync(item.SessionUnitId);

            var userLocation = await UserLocationCache.GetAsync(item.UserId.Value);

            items.Add(new UserLocationDto()
            {
                SessionUnitId = item.SessionUnitId,
                DisplayName = su.DisplayName,
                UserLocation = userLocation,
            });
        }
        return new PagedResultDto<UserLocationDto>(items.Count, items);
    }

    /// <summary>
    /// 停止共享位置（立即）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public Task<bool> StopSharingAsync(StopShareLocationInput input)
    {
        throw new NotImplementedException();
    }
}
