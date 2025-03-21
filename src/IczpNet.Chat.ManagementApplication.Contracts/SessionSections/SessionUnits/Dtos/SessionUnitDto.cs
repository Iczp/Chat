﻿using IczpNet.Chat.Management.ChatObjects.Dtos;
using System;

namespace IczpNet.Chat.Management.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitDto
    {
        public virtual Guid Id { get; set; }

        public virtual Guid SessionId { get; set; }

        public virtual long OwnerId { get; set; }

        public virtual string MemberName { get; set; }

        public virtual string MemberNameSpellingAbbreviation { get; set; }

        public virtual string Rename { get; set; }

        public virtual string RenameSpellingAbbreviation { get; set; }

        //public virtual long LastMessageId { get; set; }

        //public virtual double Sorting { get; set; }

        public virtual ChatObjectDto Destination { get; set; }
    }
}
