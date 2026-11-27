using System.ComponentModel.DataAnnotations;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.Admin.ChatObjects.Dtos;

public class ChatObjectAdminUpdateInput
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    [MaxLength(50)]
    public string Code { get; set; }

    public Genders? Gender { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [MaxLength(1000)]
    public string Portrait { get; set; }

    [MaxLength(1000)]
    public string Thumbnail { get; set; }

    public bool? IsPublic { get; set; }
}

public class ChatObjectAdminSetVerificationMethodInput
{
    public VerificationMethods VerificationMethod { get; set; }
}

public class ChatObjectAdminSetIsEnabledInput
{
    public bool IsEnabled { get; set; }
}
