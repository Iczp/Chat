using System;
using System.Collections.Generic;
using System.Text;

namespace IczpNet.Chat.Dashboards.Dtos
{
    public class DbTableDto
    {
        public virtual string TableName { get; set; }

        public virtual long RowCount { get; set; }
    }
}
