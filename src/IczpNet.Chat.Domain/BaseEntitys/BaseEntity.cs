using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace IczpNet.Chat.BaseEntitys
{
    public abstract class BaseEntity : AuditedAggregateRoot//, IMultiTenant//, ISorting
    {
        //public virtual Guid? TenantId { get; set; }
    }
    public abstract class BaseEntity<TKey> : FullAuditedAggregateRoot<TKey>, IMultiTenant//, IIsActive, IIsStatic, IIsEnabled
    {
        public virtual Guid? TenantId { get; set; }
        protected BaseEntity() { }
        protected BaseEntity(TKey id) : base(id) { }
    }
}
