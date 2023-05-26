using IczpNet.Chat.MessageSections.Messages.Dtos;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitOwnerDto : SessionUnitDto
    {
        public virtual SessionUnitSettingDto Setting { get; set; }

        public virtual MessageDto LastMessage { get; set; }

        public virtual long? LastMessageId { get; set; }

        public virtual double Sorting { get; set; }
    }
}
