using IczpNet.Chat.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;
using Volo.Abp;

namespace IczpNet.Chat.CallCenters;

/// <summary>Store-specific customer service configuration.</summary>
[Index(nameof(ShopKeeperId), IsUnique = true)]
public class CallCenterConfiguration : BaseEntity<Guid>
{
    public long ShopKeeperId { get; private set; }
    public CallCenterDistributionStrategy DistributionStrategy { get; private set; }
    public CallCenterOverflowStrategy OverflowStrategy { get; private set; } = CallCenterOverflowStrategy.AllowOverCapacity;
    public int MaxActiveConversations { get; private set; } = 20;
    public int ActiveTimeoutMinutes { get; private set; } = 30;
    public bool RequireOnline { get; private set; } = true;

    protected CallCenterConfiguration() { }

    public CallCenterConfiguration(Guid id, long shopKeeperId) : base(id)
    {
        if (shopKeeperId <= 0) throw new ArgumentOutOfRangeException(nameof(shopKeeperId));
        ShopKeeperId = shopKeeperId;
    }

    public void Configure(CallCenterDistributionStrategy distributionStrategy, CallCenterOverflowStrategy overflowStrategy,
        int maxActiveConversations, int activeTimeoutMinutes, bool requireOnline)
    {
        if (!Enum.IsDefined(distributionStrategy) || !Enum.IsDefined(overflowStrategy))
            throw new UserFriendlyException("无效的客服分配策略。");
        if (maxActiveConversations < 1 || maxActiveConversations > 100000)
            throw new UserFriendlyException("客服接待上限必须在 1 到 100000 之间。");
        if (activeTimeoutMinutes < 1 || activeTimeoutMinutes > 525600)
            throw new UserFriendlyException("活跃超时必须在 1 到 525600 分钟之间。");
        DistributionStrategy = distributionStrategy;
        OverflowStrategy = overflowStrategy;
        MaxActiveConversations = maxActiveConversations;
        ActiveTimeoutMinutes = activeTimeoutMinutes;
        RequireOnline = requireOnline;
    }
}
