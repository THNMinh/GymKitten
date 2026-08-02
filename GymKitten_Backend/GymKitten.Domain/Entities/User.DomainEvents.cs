using GymKitten.Domain.Common;

namespace GymKitten.Domain.Entities;

/// <summary>
/// Partial extension of the scaffolded User entity to add domain event support.
/// This approach is used because User cannot inherit from the Entity base class
/// without breaking existing EF Core Fluent API mappings.
/// </summary>
public partial class User
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
