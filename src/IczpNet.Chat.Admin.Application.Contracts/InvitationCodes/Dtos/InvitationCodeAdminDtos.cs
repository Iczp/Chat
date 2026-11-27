using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.InvitationCodes.Dtos;

public class InvitationCodeAdminGetListInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }

    public long? OwnerId { get; set; }

    public bool? IsEnabled { get; set; }
}

public class InvitationCodeAdminListItemDto : EntityDto<Guid>
{
    public string Title { get; set; }

    public string MaskedCode { get; set; }

    public DateTime ExpirationTime { get; set; }

    public long? OwnerId { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreationTime { get; set; }
}

public class InvitationCodeAdminDetailDto : InvitationCodeAdminListItemDto
{
    public string Code { get; set; }

    public DateTime? LastModificationTime { get; set; }
}

public class InvitationCodeAdminCreateInput
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    public long? OwnerId { get; set; }

    public DateTime? ExpirationTime { get; set; }
}

public class InvitationCodeAdminUpdateInput
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    public DateTime? ExpirationTime { get; set; }

    public bool? IsEnabled { get; set; }
}

public class InvitationCodeAdminSetIsEnabledInput
{
    public bool IsEnabled { get; set; }
}
