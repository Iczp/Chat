
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

    public virtual string ServerHostId { get; set; }

    [ForeignKey(nameof(ServerHostId))]
    public virtual ServerHost ServerHost { get; set; }

    public virtual Guid? AppUserId { get; set; }

    //public virtual Guid? ChatObjectId { get; set; }

    [StringLength(1000)]
    public virtual string ChatObjects { get; protected set; }

    [StringLength(ChatConsts.DriveIdLength)]
    public virtual string DeviceId { get; set; }

    [StringLength(36)]
    public virtual string IpAddress { get; set; }

    [StringLength(300)]
    public virtual string BrowserInfo { get; set; }

    public virtual DateTime ActiveTime { get; protected set; } = DateTime.Now;

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

    public void SetConnectionId(string id)
    {
        Id = id.ToLower();
    }

    public void SetChatObjects(List<long> chatObjectIdList)
    {
        ChatObjects = chatObjectIdList.JoinAsString(",");
        ConnectionChatObjectList = chatObjectIdList.Select(x => new ConnectionChatObject(Id, x)).ToList();
    }
}
