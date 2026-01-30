using IczpNet.AbpTrees;
using System;

namespace IczpNet.Chat.SessionOrganizations;

public class SessionOrganizationInfo : TreeInfo<long>
{
    public virtual Guid SessionId { get; set; }

    //public virtual string Code { get; set; }
}
