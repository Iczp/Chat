using System.Collections.Generic;
using System.Text.Json.Serialization;
using IczpNet.Chat.SessionUnits.Dtos;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public record MessageByIdsDto(
    List<MessageByIdDto> Items,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Dictionary<string, SessionUnitMemberSenderDto> Senders = null);
