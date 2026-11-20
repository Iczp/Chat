namespace IczpNet.Chat.CallCenters;

/// <summary>How an eligible customer service agent is selected.</summary>
public enum CallCenterDistributionStrategy
{
    ShopKeeper = 0,
    LeastActive = 1,
    AllAgents = 2
}

/// <summary>What to do when no agent is available or has capacity.</summary>
public enum CallCenterOverflowStrategy
{
    Queue = 0,
    ShopKeeper = 1,
    AllowOverCapacity = 2
}

public enum CallCenterAssignmentStatus
{
    Queued = 0,
    Assigned = 1
}
