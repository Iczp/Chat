
using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ServerHosts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.Connections;

[Index(nameof(ChatObjects))]
[Index(nameof(DeviceId))]
[Index(nameof(IpAddress))]
[Index(nameof(ActiveTime), AllDescending = true)]
[Index(nameof(CreationTime), AllDescending = true)]
[Index(nameof(CreationTime), AllDescending = false)]
public class Connection : BaseEntity<string>, IDeviceId
{
    [Key]
    [StringLength(64)]
    public override string Id { get; protected set; }

    /// <summary>
    /// ServerHostId
    /// </summary>
    public virtual string ServerHostId { get;  set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(ServerHostId))]
    public virtual ServerHost ServerHost { get; protected set; }

    /// <summary>
    /// AppUserId
    /// </summary>
    public virtual Guid? AppUserId { get; set; }

    //public virtual Guid? ChatObjectId { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [StringLength(1000)]
    public virtual string ChatObjects { get; protected set; }

    /// <summary>
    /// ClientId/ClientId
    /// </summary>
    [StringLength(64)]
    public virtual string ClientId { get; set; }

    /// <summary>
    /// DeviceId
    /// </summary>
    [StringLength(ChatConsts.DriveIdLength)]
    public virtual string DeviceId { get; set; }

    /// <summary>
    /// IpAddress
    /// </summary>
    [StringLength(36)]
    public virtual string IpAddress { get; set; }

    /// <summary>
    /// BrowserInfo
    /// </summary>
    [StringLength(300)]
    public virtual string BrowserInfo { get; set; }

    /// <summary>
    /// DeviceInfo
    /// </summary>
    [StringLength(300)]
    public virtual string DeviceInfo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual DateTime ActiveTime { get; protected set; } = DateTime.Now;

    /// <summary>
    /// 
    /// </summary>
    public virtual IList<ConnectionChatObject> ConnectionChatObjectList { get; protected set; } = [];

    protected Connection() { }

    public Connection(string id, List<long> chatObjectIdList) : base(id)
    {
        Id = id;
        SetChatObjects(chatObjectIdList);
    }

    internal void SetActiveTime(DateTime activeTime)
    {
        ActiveTime = activeTime;
    }

    public void SetChatObjects(List<long> chatObjectIdList)
    {
        ChatObjects = chatObjectIdList.JoinAsString(",");
        ConnectionChatObjectList = chatObjectIdList.Select(x => new ConnectionChatObject(Id, x)).ToList();
    }
}
