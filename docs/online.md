# 在线(服务)状态（ServerStatus）

## Enums

```C#
namespace IczpNet.Chat.Enums;

/// <summary>
/// 客服状态
/// </summary>
[Description("客服状态")]
public enum ServiceStatus : int
{
    /// <summary>
    /// 离线
    /// </summary>
    [Description("离线")]
    Offline = 0,
    /// <summary>
    /// 在线
    /// </summary>
    [Description("在线")]
    Online = 1,
    /// <summary>
    /// 挂起
    /// </summary>
    [Description("挂起")]
    Pending = 2,
    /// <summary>
    /// 隐身
    /// </summary>
    [Description("隐身")]
    Stealth = 3,
}

```

### 在线状态管理 `IServiceStateManager`

1. 群/公众号 解析为 null
2. 人/Anonymous/ShopWaiter/Customer 解析为 [Online | Offline]
3. Shopper（自己或是子账号任何一下在线就解析为 [Online ]），都不在线都解析为【Offline】