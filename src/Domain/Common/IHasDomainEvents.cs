using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitPass.Domain.Common;

public interface IHasDomainEvents
{
    public IReadOnlyCollection<BaseEvent> DomainEvents { get; }
    public void AddDomainEvent(BaseEvent domainEvent);
    public void RemoveDomainEvent(BaseEvent domainEvent);
    public void ClearDomainEvents();
}
