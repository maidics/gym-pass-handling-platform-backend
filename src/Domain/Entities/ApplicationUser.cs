using System.ComponentModel.DataAnnotations.Schema;

namespace FitPass.Domain.Entities;

public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser, IHasDomainEvents
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required ICollection<UserGymMembership>? UserGymMemberships { get; set; }
    public required GymStaffAssigment? GymStaffAssigment { get; set; }
    public ICollection<Request> Requests { get; set; } = [];
    //returns a bool value on wether or not the user is a gym member aka purchased a pass before
    public bool IsGymMember => UserGymMemberships != null && UserGymMemberships.Count > 0;

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
