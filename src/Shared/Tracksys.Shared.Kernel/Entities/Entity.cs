namespace Tracksys.Shared.Kernel.Entities;

public abstract class Entity<TId>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    private readonly List<object> _domainEvents = [];
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(object domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => (GetType().ToString() + Id).GetHashCode();
}

public abstract class AuditableEntity<TId> : Entity<TId>
    where TId : notnull
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
