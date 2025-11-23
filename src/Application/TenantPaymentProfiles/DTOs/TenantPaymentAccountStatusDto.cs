using FitPass.Domain.Entities.Payment;

namespace FitPass.Application.TenantPaymentProfiles.DTOs;

public record TenantPaymentAccountStatusDto
{
    public required bool ChargesEnabled { get; set; }
    public required bool DetailsSubmitted { get; set; }
    public required bool PayoutsEnabled { get; set; }
    public required IReadOnlyList<string> RequirementsDue { get; set; }
    public required IReadOnlyList<string> RequirementsEventuallyDue { get; set; }
}

public static partial class Mappings
{
    extension(TenantPaymentAccountStatus tenantPaymentAccountStatus)
    {
        public TenantPaymentAccountStatusDto MapToDto()
        {
            return new TenantPaymentAccountStatusDto
            {
                ChargesEnabled = tenantPaymentAccountStatus.ChargesEnabled,
                DetailsSubmitted = tenantPaymentAccountStatus.DetailsSubmitted,
                PayoutsEnabled = tenantPaymentAccountStatus.PayoutsEnabled,
                RequirementsDue = tenantPaymentAccountStatus.RequirementsDue,
                RequirementsEventuallyDue = tenantPaymentAccountStatus.RequirementsEventuallyDue
            };
        }
    }
}
