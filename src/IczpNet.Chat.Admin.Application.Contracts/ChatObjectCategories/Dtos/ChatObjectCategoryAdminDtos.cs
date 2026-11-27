using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.ChatObjectCategories.Dtos;

public class ChatObjectCategoryAdminGetListInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }

    public string ChatObjectTypeId { get; set; }

    public Guid? ParentId { get; set; }

    public bool FilterByParentId { get; set; }
}

public class ChatObjectCategoryAdminDto : EntityDto<Guid>
{
    public string Name { get; set; }

    public string ChatObjectTypeId { get; set; }

    public Guid? ParentId { get; set; }

    public int Depth { get; set; }

    public string FullPath { get; set; }

    public string FullPathName { get; set; }

    public int ChildrenCount { get; set; }

    public double Sorting { get; set; }

    public string Description { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? LastModificationTime { get; set; }
}

public class ChatObjectCategoryAdminCreateInput
{
    [Required]
    [MaxLength(64)]
    public string Name { get; set; }

    [MaxLength(50)]
    public string ChatObjectTypeId { get; set; }

    public Guid? ParentId { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    public double Sorting { get; set; }
}

public class ChatObjectCategoryAdminUpdateInput
{
    [Required]
    [MaxLength(64)]
    public string Name { get; set; }

    [MaxLength(50)]
    public string ChatObjectTypeId { get; set; }

    public Guid? ParentId { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    public double Sorting { get; set; }
}
