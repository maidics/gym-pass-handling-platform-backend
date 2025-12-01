using FitPass.Domain.Entities.Payment;
using FitPass.Domain.ValueObjects;

namespace FitPass.Domain.Entities;

public class Gym : BaseAuditableEntity
{
    public required string Name { get; set; }
    public required Address Address { get; set; }
    public required GymStatus Status { get; set; }
    public required GymTier Tier { get; set; }
    public TenantPaymentProfile? PaymentProfile { get; set; }
    public ICollection<GymPassProduct> PassProducts { get; set; } = [];
}
