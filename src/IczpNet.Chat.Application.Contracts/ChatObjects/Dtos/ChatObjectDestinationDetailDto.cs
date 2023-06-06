using IczpNet.Chat.EntryNameValues.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.ChatObjects.Dtos
{
    public class ChatObjectDestinationDetailDto : ChatObjectDto
    {
        public virtual string Description { get; set; }

        public virtual List<EntryObjectDto> ViewerEntries { get; set; }

    }
}
