using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.FriendshipTagSections.FriendshipTags.Dtos;

public class FriendshipTagSimpleDto : EntityDto<Guid>
{
    public virtual string Name { get; set; }
}
