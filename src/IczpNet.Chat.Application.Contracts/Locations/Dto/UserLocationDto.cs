using System;
using System.Collections.Generic;
using System.Text;

namespace IczpNet.Chat.Locations.Dto
{
    public class UserLocationDto
    {

        public Guid SessionUnitId { get; set; }

        public string DisplayName { get; set; }

        public UserLocationCacheItem UserLocation { get; set; }
    }
}
