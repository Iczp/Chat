using IczpNet.Chat.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.ServerHosts;

[Index(nameof(ActiveTime), AllDescending = true)]
[Index(nameof(CreationTime), AllDescending = true)]
[Index(nameof(CreationTime), AllDescending = false)]
public class ServerHost : BaseEntity<string>
{
    //[Key]
    //[StringLength(128)]
    //public override string Id { get; protected set; }

    [StringLength(128)]
    public virtual string Name { get; set; }

    public virtual DateTime? ActiveTime { get; set; }

    public virtual bool IsEnabled { get; set; }

    protected ServerHost() { }

    public ServerHost(string id, string name = null) : base(id)
    {
        Id = id;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = id;
        }
        Name = name;
    }
}
