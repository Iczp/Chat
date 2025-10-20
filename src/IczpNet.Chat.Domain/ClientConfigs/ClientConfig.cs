using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.ClientConfigs;

[Index(nameof(DeviceId), nameof(AppUserId), nameof(Key), nameof(Platform), AllDescending = true)]
[Index(nameof(Platform))]
[Index(nameof(Title))]
[Index(nameof(Description))]
public class ClientConfig : BaseEntity<Guid>, IChatOwner<long?>, IDeviceId, IAppUserId
{
    public virtual long? OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; set; }

    [Comment("DeviceId")]
    [StringLength(ChatConsts.DriveIdLength)]
    public virtual string DeviceId { get; set; }

    public Guid? AppUserId { get; set; }

    /// <summary>
    /// IOS | MAC | Win32 | Android | Linux
    /// </summary>
    [StringLength(64)]
    [Comment("Platform")]
    public virtual string Platform { get; set; }

    [StringLength(64)]
    public virtual string Title { get; set; }

    [StringLength(512)]
    public virtual string Description { get; set; }

    /// <summary>
    /// string | number | json | html | md 
    /// </summary>
    [StringLength(64)]
    public virtual string DataType { get; set; }

    [StringLength(128)]
    public virtual string Key { get; set; }

    [StringLength(5000)]
    public virtual string Value { get; set; }

}
