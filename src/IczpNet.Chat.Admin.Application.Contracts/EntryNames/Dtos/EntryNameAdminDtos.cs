using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.EntryNames.Dtos;

public class EntryNameAdminGetListInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }

    public Guid? ParentId { get; set; }

    public bool FilterByParentId { get; set; }
}

public class EntryNameAdminDto : EntityDto<Guid>
{
    public string Name { get; set; }

    public Guid? ParentId { get; set; }

    public int Depth { get; set; }

    public string FullPath { get; set; }

    public string FullPathName { get; set; }

    public int ChildrenCount { get; set; }

    public double Sorting { get; set; }

    public string Description { get; set; }

    public string Code { get; set; }

    public string InputType { get; set; }

    public string Help { get; set; }

    public string DefaultValue { get; set; }

    public string Regex { get; set; }

    public int? MaxLength { get; set; }

    public int? MinLength { get; set; }

    public int? MaxCount { get; set; }

    public int? MinCount { get; set; }

    public bool IsChoice { get; set; }

    public bool IsUniqued { get; set; }

    public bool IsRequired { get; set; }

    public bool IsStatic { get; set; }

    public bool IsPublic { get; set; }

    public string ErrorMessage { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? LastModificationTime { get; set; }
}

public class EntryNameAdminCreateInput
{
    [Required]
    [MaxLength(64)]
    public string Name { get; set; }

    public Guid? ParentId { get; set; }

    [MaxLength(20)]
    public string Code { get; set; }

    [MaxLength(20)]
    public string InputType { get; set; }

    [MaxLength(500)]
    public string Help { get; set; }

    [MaxLength(500)]
    public string DefaultValue { get; set; }

    [MaxLength(100)]
    public string Regex { get; set; }

    public int? MaxLength { get; set; }

    public int? MinLength { get; set; }

    public int? MaxCount { get; set; }

    public int? MinCount { get; set; }

    public bool IsChoice { get; set; }

    public bool IsUniqued { get; set; }

    public bool IsRequired { get; set; }

    public bool IsPublic { get; set; } = true;

    [MaxLength(200)]
    public string ErrorMessage { get; set; }

    public double Sorting { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }
}

public class EntryNameAdminUpdateInput : EntryNameAdminCreateInput
{
}
