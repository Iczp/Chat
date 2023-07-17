using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Entries.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits.Dtos
{
    public class SessionUnitOwnerDetailDto : SessionUnitDto
    {
        //public virtual Guid SessionId { get; set; }

        //public virtual long SessionUnitId { get; set; }

        public virtual string DisplayName { get; set; }

        //public virtual string MemberNameSpellingAbbreviation { get; set; }

        //public virtual string Rename { get; set; }

        //public virtual string RenameSpellingAbbreviation { get; set; }

        //public virtual long ReadedMessageId { get; set; }

        //public virtual double Sorting { get; set; }

        public virtual ChatObjectDto Destination { get; set; }

        public virtual List<EntryObjectDto> Entries { get; set; }
    }
}
