using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageChangeAckInput
{
    [Range(1, long.MaxValue)]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long ChatObjectId { get; set; }

    [Required, StringLength(200)]
    public string DeviceId { get; set; }

    [Required, StringLength(64)]
    public string Epoch { get; set; }

    [Required, RegularExpression("^[0-9]{1,19}$")]
    public string LastAppliedMessageChangeId { get; set; }

    public bool Reset { get; set; }
}
