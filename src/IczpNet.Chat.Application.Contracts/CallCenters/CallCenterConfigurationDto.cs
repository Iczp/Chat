using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.CallCenters;

public class CallCenterConfigurationDto
{
    public long ShopKeeperId { get; set; }
    public CallCenterDistributionStrategy DistributionStrategy { get; set; }
    public CallCenterOverflowStrategy OverflowStrategy { get; set; }
    public int MaxActiveConversations { get; set; }
    public int ActiveTimeoutMinutes { get; set; }
    public bool RequireOnline { get; set; }
}

public class UpdateCallCenterConfigurationInput
{
    public CallCenterDistributionStrategy DistributionStrategy { get; set; }
    public CallCenterOverflowStrategy OverflowStrategy { get; set; } = CallCenterOverflowStrategy.AllowOverCapacity;
    [Range(1, 100000)] public int MaxActiveConversations { get; set; } = 20;
    [Range(1, 525600)] public int ActiveTimeoutMinutes { get; set; } = 30;
    public bool RequireOnline { get; set; } = true;
}
