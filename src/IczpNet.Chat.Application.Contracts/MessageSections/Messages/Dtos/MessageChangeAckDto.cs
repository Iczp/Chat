namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public record MessageChangeAckDto(
    string Epoch,
    string HeadChangeId,
    string LastAppliedChangeId,
    bool Initialized,
    bool ResetRequired);
