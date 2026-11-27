using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.Admin.ChatObjects.Dtos;

public class ChatObjectAdminCreateInput
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    [MaxLength(50)]
    public string Code { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [MaxLength(64)]
    public string ChatObjectTypeId { get; set; }

    public ChatObjectTypeEnums? ObjectType { get; set; }

    public Genders? Gender { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool IsPublic { get; set; } = true;

    [MaxLength(1000)]
    public string Portrait { get; set; }

    [MaxLength(1000)]
    public string Thumbnail { get; set; }

    public VerificationMethods VerificationMethod { get; set; }

    public List<Guid> CategoryIds { get; set; }
}
