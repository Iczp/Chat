using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.EntryValues.Dtos;

public class EntryValueAdminGetListInput : PagedAndSortedResultRequestDto
{
    public Guid? EntryNameId { get; set; }

    public string Keyword { get; set; }
}

public class EntryValueAdminDto : EntityDto<Guid>
{
    public Guid EntryNameId { get; set; }

    public string Name { get; set; }

    public string Value { get; set; }

    public bool IsStatic { get; set; }

    public bool IsPublic { get; set; }

    public bool IsOption { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? LastModificationTime { get; set; }
}

public class EntryValueAdminCreateInput
{
    [Required]
    public Guid EntryNameId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Value { get; set; }

    public bool IsOption { get; set; } = true;

    public bool IsPublic { get; set; } = true;
}

public class EntryValueAdminUpdateInput
{
    [Required]
    [MaxLength(500)]
    public string Value { get; set; }

    public bool IsOption { get; set; } = true;

    public bool IsPublic { get; set; } = true;
}
