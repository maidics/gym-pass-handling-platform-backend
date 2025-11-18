
using System.ComponentModel.DataAnnotations.Schema;

namespace FitPass.Domain.Entities.Payment;

public class TenantPaymentProfile : IHasDomainEvents
{
    public required string GymId { get; set; }
    public string? PaymentTenantAccountId { get; set; }
    public TenantPaymentAccountStatus AccountStatus { get; set; } = TenantPaymentAccountStatus.Default();
    public DateTimeOffset? LastUpdatedOnPaymentProvidersSide { get; set; }
    public string? LastUpdatedByOnPaymentProvidersSide { get; set; }
    public DateTimeOffset? LastAccountLinkGeneratedOn { get; set; }
    public string? LastAccountLinkGeneratedBy { get; set; } //turn these into collections later?
    public Gym Gym { get; set; } = null!;

    private readonly List<BaseEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
