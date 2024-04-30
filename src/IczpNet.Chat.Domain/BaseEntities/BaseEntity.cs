using IczpNet.AbpCommons.DataFilters;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace IczpNet.Chat.BaseEntities;

public abstract class BaseEntity : AuditedAggregateRoot//, IMultiTenant//, ISorting
{
    //public virtual Guid? TenantId { get; set; }
}

public abstract class BaseEntity<TKey> : FullAuditedAggregateRoot<TKey>//, IMultiTenant//, IIsActive, IIsStatic, IIsEnabled
{
    //public virtual Guid? TenantId { get; set; }
    protected BaseEntity() { }
    protected BaseEntity(TKey id) : base(id) { }
}

public abstract class BaseSpellingEntity<TKey> : FullAuditedAggregateRoot<TKey>, IName//, IMultiTenant//, IIsActive, IIsStatic, IIsEnabled
{
    //public virtual Guid? TenantId { get; set; }

    [StringLength(50)]
    public virtual string Name { get; protected set; }

    protected BaseSpellingEntity() { }

    protected BaseSpellingEntity(TKey id) : base(id) { }

    public virtual void SetName(string name)
    {
        Name = name;
    }
}
