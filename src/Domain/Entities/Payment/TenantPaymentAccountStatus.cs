namespace FitPass.Domain.Entities.Payment;

public class TenantPaymentAccountStatus
{
    public required bool ChargesEnabled { get; set; }
    public required bool DetailsSubmitted { get; set; }
    public required bool PayoutsEnabled { get; set; }
    public required IReadOnlyList<string> RequirementsDue { get; set; }
    public required IReadOnlyList<string> RequirementsEventuallyDue { get; set; }

    public static TenantPaymentAccountStatus Default()
    {
        return new TenantPaymentAccountStatus
        {
            ChargesEnabled = false,
            DetailsSubmitted = false,
            PayoutsEnabled = false,
            RequirementsDue = [],
            RequirementsEventuallyDue = []
        };
    }
}
