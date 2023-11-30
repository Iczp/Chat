using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Entries.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits.Dtos
{
    public class SessionUnitOwnerDetailDto : SessionUnitDestinationDto
    {
        public virtual int? SessionUnitCount { get; set; }

        public virtual ChatObjectDto Destination { get; set; }


        public virtual List<EntryObjectDto> Entries { get; set; }
    }
}
