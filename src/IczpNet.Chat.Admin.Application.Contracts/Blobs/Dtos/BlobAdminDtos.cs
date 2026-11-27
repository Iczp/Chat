using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.Blobs.Dtos;

public class BlobAdminGetListInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }

    public string Container { get; set; }

    public bool? IsPublic { get; set; }

    public bool? IsStatic { get; set; }
}

public class BlobAdminDto : EntityDto<Guid>
{
    public string Container { get; set; }

    public string Name { get; set; }

    public string FileName { get; set; }

    public long FileSize { get; set; }

    public string MimeType { get; set; }

    public string Suffix { get; set; }

    public bool IsPublic { get; set; }

    public bool IsStatic { get; set; }

    public long? UploaderChatObjectId { get; set; }

    public int RefCount { get; set; }

    public Guid? StorageObjectId { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? LastModificationTime { get; set; }
}

public class BlobAdminUpdateInput
{
    [MaxLength(256)]
    public string FileName { get; set; }

    public bool? IsPublic { get; set; }

    public bool? IsStatic { get; set; }
}
