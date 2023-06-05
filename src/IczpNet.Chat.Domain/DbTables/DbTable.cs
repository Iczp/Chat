using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace IczpNet.Chat.DbTables
{
    [NotMapped]
    public class DbTable : IEntity
    {
        public virtual string TableName { get; set; }

        public virtual long RowCount { get; set; }

        protected DbTable() { }

        public object[] GetKeys()
        {
            return new[] { TableName };
        }
    }
}
