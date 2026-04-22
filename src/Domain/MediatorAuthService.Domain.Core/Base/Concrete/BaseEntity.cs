using MediatorAuthService.Domain.Core.Attributes;

namespace MediatorAuthService.Domain.Core.Base.Concrete;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    [NotSortable]
    public Guid CreatedUserId { get; set; }

    [NotSortable]
    public DateTime? ModifiedDate { get; set; }

    [NotSortable]
    public Guid? ModifiedUserId { get; set; }

    [NotSortable]
    public DateTime? DeletedDate { get; set; }

    [NotSortable]
    public Guid? DeletedUserId { get; set; }
}

public abstract class BaseEntity<TId> : BaseEntity where TId : struct
{
    public new TId Id { get; set; }
}