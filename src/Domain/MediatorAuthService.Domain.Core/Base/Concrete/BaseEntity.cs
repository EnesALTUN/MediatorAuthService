namespace MediatorAuthService.Domain.Core.Base.Concrete;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid CreatedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public Guid? ModifiedUserId { get; set; }

    public DateTime? DeletedDate { get; set; }

    public Guid? DeletedUserId { get; set; }
}

public abstract class BaseEntity<TId> : BaseEntity
{
    public new TId Id { get; set; }
}