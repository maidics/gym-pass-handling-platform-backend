namespace FitPass.Domain.Common;

public interface IHasDomainEvents
{
    public IReadOnlyCollection<BaseEvent> DomainEvents { get; }
    public void AddDomainEvent(BaseEvent domainEvent);
    public void RemoveDomainEvent(BaseEvent domainEvent);
    public void ClearDomainEvents();
}
